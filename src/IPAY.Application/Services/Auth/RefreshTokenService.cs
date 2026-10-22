using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos.Auth;
using Microsoft.Extensions.Configuration;

namespace IPAY.Application.Services.Auth
{
    //refreshing token service
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IJWT _jwt;
        private readonly IRefreshTokenRepository _repo;
        private readonly IUser<Customer> _customers;
        private readonly IGuestService _guests;
        private readonly IConfiguration _config;

        public RefreshTokenService(
            IJWT jwt,
            IRefreshTokenRepository repo,
            IUser<Customer> customers,
            IGuestService guests,
            IConfiguration config)
        {
            _jwt = jwt;
            _repo = repo;
            _customers = customers;
            _guests = guests;
            _config = config;
        }
        //lambda's for calculating the time
        private int AccessMinutes =>
            int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 20;

        private int RefreshDays =>
            int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 14;
        
        public async Task<(string rawRefresh, DateTime accessExpires)> IssuePairAsync(
            int userId, string email, UserRole role, string? ip = null)
        {
            var raw = _jwt.GenerateRefreshToken();
            var entity = new RefreshToken
            {
                Id = _jwt.HashToken(raw),
                UserId = userId,
                Email = email,
                Role = role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshDays),
                CreatedByIp = ip
            };
            await _repo.AddAsync(entity);

            return (raw, DateTime.UtcNow.AddMinutes(AccessMinutes));
        }
        //refreshing token
        public async Task<AuthResponse?> RefreshAsync(string rawRefreshToken, string? ip = null)
        {
            if (string.IsNullOrWhiteSpace(rawRefreshToken))
                return null;

            var id = _jwt.HashToken(rawRefreshToken);
            var existing = await _repo.GetByIdAsync(id);

            // reuse detection: already revoked → kill all sessions for this user
            if (existing is null)
                return null;

            if (existing.IsRevoked)
            {
                await _repo.RevokeAllForUserAsync(existing.UserId);
                return null;
            }

            if (existing.ExpiresAt < DateTime.UtcNow)
                return null;

            if (!Enum.TryParse<UserRole>(existing.Role, out var role))
                return null;

            // load user / guest and check ban
            string email = existing.Email;
            string? name = null;

            if (role == UserRole.Guest)
            {
                var guest = await _guests.GetByIdAsync(existing.UserId);
                if (guest is null || guest.IsBanned)
                    return null;
                name = guest.Name;
                email = $"guest-{guest.Id}@guest.local";
            }
            else
            {
                var user = await _customers.GetByIdAsync(existing.UserId);
                if (user is null || user.IsBanned || user.Id is null)
                    return null;
                email = user.Email ?? existing.Email;
                name = user.Name;
                role = user.Role; // role could have changed
            }

            // rotate
            var (newRaw, accessExpires) = await IssuePairAsync(
                existing.UserId, email, role, ip);

            existing.IsRevoked = true;
            existing.ReplacedByTokenId = _jwt.HashToken(newRaw);
            await _repo.UpdateAsync(existing);

            var access = _jwt.GenerateAccessToken(
                existing.UserId.ToString(), email, role);

            return new AuthResponse
            {
                AccessToken = access,
                RefreshToken = newRaw,
                TokenType = "Bearer",
                ExpiresAt = accessExpires,
                User = new LoginDto { Email = email, Name = name }
            };
        }
        //revoking a token
        public async Task RevokeAsync(string rawRefreshToken)
        {
            if (string.IsNullOrWhiteSpace(rawRefreshToken)) return;//if null
            var existing = await _repo.GetByIdAsync(_jwt.HashToken(rawRefreshToken));//taking
            if (existing is null || existing.IsRevoked) return;
            existing.IsRevoked = true;//making revoke
            await _repo.UpdateAsync(existing);
        }
        //revoking for all users
        public Task RevokeAllForUserAsync(int userId) =>
            _repo.RevokeAllForUserAsync(userId);
        //creating a new token
        public async Task<AuthResponse> CreateAuthResponseAsync(
               int userId,
               string email,
               string? name,
               UserRole role,
               string? ip = null)
        {
            var access = _jwt.GenerateAccessToken(userId.ToString(), email, role);
            var (rawRefresh, expires) = await IssuePairAsync(userId, email, role, ip);

            return new AuthResponse
            {
                AccessToken = access,
                RefreshToken = rawRefresh,
                TokenType = "Bearer",
                ExpiresAt = expires,
                User = new LoginDto { Email = email, Name = name }
            };
        }
    }
}
