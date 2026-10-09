using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Application.Interfaces.Auth;

namespace IPAY.Application.Services.Auth
{


    public class GuestService:IGuestService
        {
            private readonly IRepository<Guest> _repo;
            private readonly IMapper _mapper;

            public GuestService(IRepository<Guest> repo, IMapper mapper)
            {
                _repo = repo;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<GuestDto>> GetAllAsync()
            {
                var all = await _repo.GetAllAsync();
                return _mapper.Map<IReadOnlyList<GuestDto>>(all);
            }

            public async Task<GuestDto?> GetByIdAsync(int id)
            {
                var guest = await _repo.GetByIdAsync(id);
                return guest is null ? null : _mapper.Map<GuestDto>(guest);
            }

            /// <summary>
            /// Найти по SessionKey или создать нового. Всегда возвращает DTO.
            /// </summary>
            public async Task<GuestDto> GetOrCreateAsync(string? sessionKey, string? name = null)
            {
                if (!string.IsNullOrWhiteSpace(sessionKey))
                {
                    var all = await _repo.GetAllAsync();
                    var existing = all.FirstOrDefault(g =>
                        string.Equals(g.SessionKey, sessionKey, StringComparison.Ordinal));

                    if (existing is not null)
                        return _mapper.Map<GuestDto>(existing);
                }

                var guest = new Guest
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "Guest" : name.Trim(),
                    Role = UserRole.Guest,
                    SessionKey = string.IsNullOrWhiteSpace(sessionKey)
                        ? Guid.NewGuid().ToString("N")
                        : sessionKey,
                    IsBanned = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repo.AddAsync(guest);
                return _mapper.Map<GuestDto>(created!);
            }

            public async Task<GuestDto?> CreateAsync(GuestSessionDto dto)
            {
                var guest = _mapper.Map<Guest>(dto);
                var created = await _repo.AddAsync(guest);
                return created is null ? null : _mapper.Map<GuestDto>(created);
            }

            public async Task<GuestDto?> UpdateAsync(int id, UpdateGuestDto dto)
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest is null) return null;

                _mapper.Map(dto, guest);
                await _repo.UpdateAsync(id, guest);
                return _mapper.Map<GuestDto>(guest);
            }


            public async Task<bool> DeleteAsync(int id)
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest is null) return false;

                await _repo.DeleteAsync(id);
                return true;
            }
        }
    
}
