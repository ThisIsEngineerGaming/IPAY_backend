using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Mappings.Auth;
using IPAY.Application.Services.Auth;
using IPAY.Application.Validators.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Infrastructure.Authorization;
using IPAY.UnitTests.Support;
using IPAY.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace IPAY.UnitTests.Integration
{
    /// <summary>
    /// Registration -> login -> token, with the REAL validators, password hashing, JWT service,
    /// customer service and AuthController wired together. Only the database (in-memory) and the
    /// Google / emailed-code collaborators (not part of this flow) are replaced.
    /// </summary>
    [TestFixture]
    public class AuthFlowIntegrationTests
    {
        private const string Key = "integration-test-signing-key-that-is-long-enough-123456";
        private const string Issuer = "ipay-tests";
        private const string Audience = "ipay-tests-clients";

        private InMemoryRepository<Customer> _customerRepo = null!;
        private CustomerService _customers = null!;
        private AuthController _auth = null!;

        private const string StrongPassword = "Sup3r$ecret!";

        [SetUp]
        public void SetUp()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = Key,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience
            }).Build();

            _customerRepo = new InMemoryRepository<Customer>(c => c.Id ?? 0, (c, id) => c.Id = id);
            _customers = new CustomerService(_customerRepo);
            var hasher = new PasswordHashingService();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AuthMapping>()).CreateMapper();

            var register = new RegisterDtoService(new RegisterValidator(), _customers, hasher, mapper);
            var login = new LoginDtoService(new LoginValidator(), new JwtService(config), _customers, hasher);

            _auth = new AuthController(
                register, login, _customers,
                new Mock<IGoogleLogin>().Object, new Mock<ITwoFactorService>().Object);
        }

        private static RegisterDto NewUser(string email = "alice@example.com", string password = StrongPassword) => new()
        {
            Name = "Alice", Email = email, Password = password, ConfirmPassword = password
        };

        private static ClaimsPrincipal ValidateToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = Issuer,
                ValidateAudience = true, ValidAudience = Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        }

        [Test]
        public async Task RegisteredUser_CanLogIn_AndGetsATokenTheApiWouldAccept()
        {
            Assert.That(await _auth.Registration(NewUser()), Is.InstanceOf<OkObjectResult>());

            var result = await _auth.Login(new LoginDto { Email = "alice@example.com", Password = StrongPassword });

            var response = (AuthResponse)((OkObjectResult)result.Result!).Value!;
            var principal = ValidateToken(response.AccessToken);
            Assert.That(principal.FindFirstValue(ClaimTypes.Email), Is.EqualTo("alice@example.com"));
            Assert.That(principal.FindFirstValue(ClaimTypes.Role), Is.EqualTo("Customer"));
            Assert.That(int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!), Is.GreaterThan(0));
        }

        [Test]
        public async Task Login_IsCaseInsensitiveAboutTheEmail()
        {
            await _auth.Registration(NewUser("alice@example.com"));

            var result = await _auth.Login(new LoginDto { Email = "ALICE@Example.com", Password = StrongPassword });

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Registration_StoresAHashedPassword_NeverThePlainOne()
        {
            await _auth.Registration(NewUser());

            var stored = (await _customerRepo.GetAllAsync()).Single();

            Assert.That(stored.Password, Is.Not.EqualTo(StrongPassword));
            Assert.That(stored.Password, Does.StartWith("PBKDF2-SHA256$"));
        }

        [Test]
        public async Task Registration_AlwaysCreatesACustomer_AndMarksEmailUnverified()
        {
            await _auth.Registration(NewUser());

            var stored = (await _customerRepo.GetAllAsync()).Single();

            Assert.That(stored.Role, Is.EqualTo(UserRole.Customer));
            Assert.That(stored.EmailVerified, Is.False);
        }

        [Test]
        public async Task Registration_WithAnEmailAlreadyTaken_IsRejected_EvenWithDifferentCasing()
        {
            await _auth.Registration(NewUser("alice@example.com"));

            var second = await _auth.Registration(NewUser("ALICE@example.com"));

            Assert.That(second, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(await _customerRepo.GetAllAsync(), Has.Count.EqualTo(1));
        }

        [TestCase("Sup3r$ecret!")]
        [TestCase("short1!A")] // exactly 8 characters with every required class
        public async Task Registration_WithAStrongPassword_Succeeds(string password)
        {
            Assert.That(await _auth.Registration(NewUser(password: password)), Is.InstanceOf<OkObjectResult>());
        }

        [TestCase("password")]      // no uppercase / digit / symbol
        [TestCase("PASSWORD1!")]    // no lowercase
        [TestCase("Pass1!")]        // too short
        public async Task Registration_WithAWeakPassword_IsRejectedAndNothingIsStored(string weak)
        {
            var result = await _auth.Registration(NewUser(password: weak));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(await _customerRepo.GetAllAsync(), Is.Empty);
        }

        [Test]
        public async Task Login_WithWrongPassword_IsRejected_WithoutSayingWhichPartWasWrong()
        {
            await _auth.Registration(NewUser());

            var wrongPassword = await _auth.Login(new LoginDto { Email = "alice@example.com", Password = "Wr0ng$Password" });
            var unknownEmail = await _auth.Login(new LoginDto { Email = "nobody@example.com", Password = StrongPassword });

            Assert.That(wrongPassword.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(unknownEmail.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)wrongPassword.Result!).Value,
                Is.EqualTo(((BadRequestObjectResult)unknownEmail.Result!).Value));
        }

        [Test]
        public async Task SellerRequest_PromotesTheCallerToSeller_AndTheNextLoginTokenCarriesTheNewRole()
        {
            await _auth.Registration(NewUser());
            var id = (await _customerRepo.GetAllAsync()).Single().Id!.Value;

            await _auth.WithUser(id, "Customer").MakeRequest();
            var result = await _auth.Login(new LoginDto { Email = "alice@example.com", Password = StrongPassword });

            var token = ((AuthResponse)((OkObjectResult)result.Result!).Value!).AccessToken;
            Assert.That(ValidateToken(token).FindFirstValue(ClaimTypes.Role), Is.EqualTo("Seller"));
        }

        [Test]
        public async Task BannedUser_IsDeniedByTheNotBannedPolicy_AndAllowedAgainWhenUnbanned()
        {
            await _auth.Registration(NewUser());
            var user = (await _customerRepo.GetAllAsync()).Single();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, user.Id!.Value.ToString()) }, "test"));
            var handler = new NotBannedHandler(_customers);

            async Task<bool> Allowed()
            {
                var ctx = new AuthorizationHandlerContext(
                    new IAuthorizationRequirement[] { new NotBannedRequirement() }, principal, null);
                await handler.HandleAsync(ctx);
                return ctx.HasSucceeded;
            }

            Assert.That(await Allowed(), Is.True);

            user.IsBanned = true;
            await _customers.UpdateAsync(user.Id!.Value, user);
            Assert.That(await Allowed(), Is.False);

            user.IsBanned = false;
            await _customers.UpdateAsync(user.Id!.Value, user);
            Assert.That(await Allowed(), Is.True);
        }
    }
}
