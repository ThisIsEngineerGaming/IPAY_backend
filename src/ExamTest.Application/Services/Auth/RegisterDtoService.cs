using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.Domain.Entities.Auth;
using FluentValidation;
using System.Threading.Tasks;

namespace ExamTest.Application.Services.Auth
{
    public class RegisterDtoService : IAuth<RegisterDto>
    {
        private readonly IValidator<RegisterDto> _validator;
        private readonly IUser<Seller> _sellerService;
        private readonly IPasswordHashingService _passwordHasher;

        public RegisterDtoService(
            IValidator<RegisterDto> validator,
            IUser<Seller> sellerService,
            IPasswordHashingService passwordHasher)
        {
            _validator = validator;
            _sellerService = sellerService;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            var result = await _validator.ValidateAsync(registerDto);
            if (!result.IsValid)
                return false;

            var seller = new Seller
            {
                Email = registerDto.Email.Trim().ToLowerInvariant(),
                Name = registerDto.Name.Trim(),
                Password = _passwordHasher.Hash(registerDto.Password)
            };

            return await _sellerService.CreateAsync(seller) is not null;
        }
    }
}

