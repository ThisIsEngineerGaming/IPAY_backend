using IPAY.Domain.Enums;
using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Interfaces.ForRepos;
using FluentValidation;
using System.Threading.Tasks;

namespace IPAY.Application.Services.Auth
{
    public class RegisterDtoService : IAuthDto<RegisterDto>
    {
        private readonly IValidator<RegisterDto> _validator;
        private readonly IUser<Customer> _customerService;
        private readonly IPasswordHashingService _passwordHasher;

        

        private readonly IMapper _mapper;

        public RegisterDtoService(
            IValidator<RegisterDto> validator,
            IUser<Customer> customerService,
            IPasswordHashingService passwordHasher,
            IMapper mapper)
        {
            _validator = validator;
            _customerService = customerService;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            var result = await _validator.ValidateAsync(registerDto);
            if (!result.IsValid)
                return false;

            var user = _mapper.Map<Customer>(registerDto);
            if (user is null)
                return false;

            // login looks up users by a trimmed, case-insensitive email, so stored values must not carry stray spaces/casing.
            user.Email = registerDto.Email.Trim().ToLowerInvariant();
            user.Name = registerDto.Name.Trim();
            user.Password = _passwordHasher.Hash(registerDto.Password);

            // Explicit on top of the AutoMapper rule in AuthMapping: every
            // self-registration is a Customer. Sellers are promoted later via
            // AuthController's seller-request/role-change endpoints, never here.
            user.Role = UserRole.Customer;

            return await _customerService.CreateAsync(user) is not null;
        }
    }
}

