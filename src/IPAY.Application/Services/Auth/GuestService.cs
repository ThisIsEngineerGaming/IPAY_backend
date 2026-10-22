using AutoMapper;
using IPAY.Application.DTOs.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using IPAY.Domain.Interfaces.ForRepos;
using IPAY.Application.Interfaces.Auth;

namespace IPAY.Application.Services.Auth
{

    //guets service
    public class GuestService:IGuestService
        {
            private readonly IRepository<Guest> _repo;//also a basic CRUD Repository
            private readonly IMapper _mapper;//here we are using mapper to "transfer" the data type

            public GuestService(IRepository<Guest> repo, IMapper mapper)
            {
                _repo = repo;
                _mapper = mapper;
            }
            //get all
            public async Task<IReadOnlyList<GuestDto>> GetAllAsync()
            {
                var all = await _repo.GetAllAsync();
                return _mapper.Map<IReadOnlyList<GuestDto>>(all);//for frontend we need to transfer object into dto
            }
            //get by id
            public async Task<GuestDto?> GetByIdAsync(int id)
            {
                var guest = await _repo.GetByIdAsync(id);
                return guest is null ? null : _mapper.Map<GuestDto>(guest);//if it is not null we will recieve dto
            }

            /// <summary>
            /// Найти по SessionKey или создать нового. Всегда возвращает DTO.
            /// </summary>
            //this method will be used if we not sure user
            public async Task<GuestDto> GetOrCreateAsync(string? sessionKey, string? name = null)//we are creating session key to differ them
            {
                if (!string.IsNullOrWhiteSpace(sessionKey))
                {
                    var all = await _repo.GetAllAsync();
                    var existing = all.FirstOrDefault(g =>
                        string.Equals(g.SessionKey, sessionKey, StringComparison.Ordinal));

                    if (existing is not null)
                        return _mapper.Map<GuestDto>(existing);//if we have already this user - returning him
                }
                //if not - creating
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
            //only create
            public async Task<GuestDto?> CreateAsync(GuestSessionDto dto)
            {
                var guest = _mapper.Map<Guest>(dto);
                var created = await _repo.AddAsync(guest);
                return created is null ? null : _mapper.Map<GuestDto>(created);
            }
            //updating
            public async Task<GuestDto?> UpdateAsync(int id, UpdateGuestDto dto)
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest is null) return null;

                _mapper.Map(dto, guest);
                await _repo.UpdateAsync(id, guest);
                return _mapper.Map<GuestDto>(guest);
            }

            //deleting
            public async Task<bool> DeleteAsync(int id)
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest is null) return false;

                await _repo.DeleteAsync(id);
                return true;
            }
        }
    
}
