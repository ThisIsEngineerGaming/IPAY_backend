using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{

    [ApiController]
    [Route("api/admin")]
    public class AdminController:ControllerBase
    {
        private readonly IUser<Customer> _customer;
        private readonly ILogger<AdminController>? _logger;

        private readonly IGuestService _guest;

        private readonly IPasswordHashingService _passwordHasher;

        private readonly IJWT _jwt;

        public AdminController(
            IUser<Customer> customer,
            IGuestService guest,
            IPasswordHashingService passwordHasher,
            IJWT jwt,
            ILogger<AdminController>? logger = null)
        {
            _customer = customer;
            _guest = guest;
            _passwordHasher = passwordHasher;
            _jwt = jwt;
            _logger = logger;
        }

        [HttpPost("guests/session")]
        public async Task<IActionResult> EnsureGuestSession([FromBody] GuestSessionDto? dto)
        {
            var guest = await _guest.GetOrCreateAsync(dto?.SessionKey, dto?.Name);

            if (guest.IsBanned)
                return StatusCode(403, "Заблоковано");

            // email у гостя нет — в claim кладём заглушку
            var token = _jwt.GenerateToken(
                guest.Id.ToString(),
                $"guest-{guest.Id}@guest.local",
                UserRole.Guest);

            return Ok(new
            {
                guest.Id,
                guest.Name,
                guest.Role,
                guest.IsBanned,
                guest.SessionKey,
                guest.CreatedAt,
                accessToken = token,
                tokenType = "Bearer",
                expiresAt = DateTime.UtcNow.AddHours(4)
            });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id}/ban")]
        public async Task<IActionResult> BannedByAdmin(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin)
            {
                return Forbid();
            }
            user.IsBanned = true;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("moderator/{id}/ban")]
        public async Task<IActionResult> BannedByModerator(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin || user.Role == UserRole.Moderator)
            {
                return Forbid();
            }
            user.IsBanned = true;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id}/unban")]
        public async Task<IActionResult> UnBannedByAdmin(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin)
            {
                return Forbid();
            }
            user.IsBanned = false;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("moderator/{id}/unban")]
        public async Task<IActionResult> UnBannedByModerator(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            else if (user.Role == UserRole.Admin || user.Role == UserRole.Moderator)
            {
                return Forbid();
            }
            user.IsBanned = false;
            var result = _customer.UpdateAsync(id, user);

            return Ok(result);


        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromQuery] UserRole role)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
                return NotFound();

            user.Role = role;
            await _customer.UpdateAsync(id, user); // сохранить в БД/Firestore

            return Ok(user);
        }

        [Authorize(Policy = "ModeratorOnly")]
        [HttpPatch("users/moderator/{id}/role")]
        public async Task<IActionResult> ChangeRoleByModerator(int id, [FromQuery] UserRole role)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
                return NotFound();

            // Same protection as the ban endpoints: a moderator must not be able to touch admins or other
            // moderators (without this, role=Customer would have demoted an admin).
            if (user.Role == UserRole.Admin || user.Role == UserRole.Moderator)
                return Forbid();
            if (role != UserRole.Moderator && role != UserRole.Admin)
            {
                user.Role = role;
                await _customer.UpdateAsync(id, user); // сохранить в БД/Firestore

                return Ok(user);
            }
            else
            {
                return BadRequest("Ти не маешь права!");
            }

        }


        // --- Role management for the admin page ------------------------------------------------
        // Who may do what:
        //   make a Seller    -> an Admin or a Moderator
        //   make a Moderator -> an Admin only
        // Admins and moderators themselves can never be changed through these endpoints.
        // The promoted person's current login keeps the old role until they sign in again,
        // because the role is baked into the JWT.

        // Everyone the admin page lists. Admin and moderator both need it.
        [Authorize(Policy = "ModeratorOrAdmin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var all = await _customer.GetAllAsync();
            return Ok(all.OrderBy(c => c.Id).Select(ToAdminUser).ToList());
        }

        [Authorize(Policy = "ModeratorOrAdmin")]
        [HttpPost("users/{id:int}/make-seller")]
        public Task<IActionResult> MakeSeller(int id) => PromoteAsync(id, UserRole.Seller);

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("users/{id:int}/make-moderator")]
        public Task<IActionResult> MakeModerator(int id) => PromoteAsync(id, UserRole.Moderator);

        private async Task<IActionResult> PromoteAsync(int id, UserRole newRole)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
                return NotFound("User not found.");

            if (user.Role == newRole)
                return BadRequest($"This user is already a {newRole.ToString().ToLowerInvariant()}.");

            // 403 with a message (not Forbid(), whose body is empty) so the admin page can show the reason.
            if (user.Role is UserRole.Admin or UserRole.Moderator)
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Admins and moderators can't be changed here.");

            if (user.IsBanned)
                return BadRequest("This account is banned. Unban it before changing its role.");

            user.Role = newRole;
            await _customer.UpdateAsync(id, user);

            return Ok(ToAdminUser(user));
        }

        private static AdminUserDto ToAdminUser(Customer c) => new()
        {
            Id = c.Id ?? 0,
            Name = c.Name ?? string.Empty,
            Email = c.Email ?? string.Empty,
            Phone = c.Phone ?? string.Empty,
            Role = c.Role.ToString(),
            IsBanned = c.IsBanned
        };

        // --- CREATE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer is null)
                return BadRequest("Не повинно бути нулем");

            // пароль хешируем, если передали сырой
            if (!string.IsNullOrWhiteSpace(customer.Password))
                customer.Password = _passwordHasher.Hash(customer.Password);

            customer.Email = customer.Email?.Trim().ToLowerInvariant();
            customer.Role = customer.Role == default ? UserRole.Customer : customer.Role;
            customer.EmailVerified ??= true; // админ создаёт — можно сразу verified

            var result = await _customer.CreateAsync(customer);
            if (result is null)
                return BadRequest("Email вже зайнятий або не вдалося створити");

            return Ok(result);
        }

        // --- READ ALL ---
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var list = await _customer.GetAllAsync();
            return Ok(list);
        }

        // --- READ BY ID ---
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("customers/{id:int}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var user = await _customer.GetByIdAsync(id);
            if (user is null)
                return NotFound("Користувача не знайдено");

            return Ok(user);
        }

        // --- UPDATE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("customers/{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (customer is null)
                return BadRequest("Не повинно бути нулем");

            var existing = await _customer.GetByIdAsync(id);
            if (existing is null)
                return NotFound("Користувача не знайдено");

            // обновляем только нужные поля
            if (!string.IsNullOrWhiteSpace(customer.Name))
                existing.Name = customer.Name.Trim();

            if (!string.IsNullOrWhiteSpace(customer.Email))
                existing.Email = customer.Email.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(customer.Password))
                existing.Password = _passwordHasher.Hash(customer.Password);

            existing.Role = customer.Role;
            existing.IsBanned = customer.IsBanned;
            existing.EmailVerified = customer.EmailVerified ?? existing.EmailVerified;

            await _customer.UpdateAsync(id, existing);
            return Ok(existing);
        }

        // --- DELETE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("customers/{id:int}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var existing = await _customer.GetByIdAsync(id);
            if (existing is null)
                return NotFound("Користувача не знайдено");

            await _customer.DeleteAsync(id);
            return Ok("Видалено");
        }



        // --- READ ALL ---
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("guests")]
        public async Task<IActionResult> GetAllGuests()
        {
            return Ok(await _guest.GetAllAsync());
        }

        // --- READ BY ID ---
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("guests/{id:int}")]
        public async Task<IActionResult> GetGuest(int id)
        {
            var g = await _guest.GetByIdAsync(id);
            if (g is null)
                return NotFound("Гостя не знайдено");

            return Ok(g);
        }

        // --- CREATE (админ) ---
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("guests")]
        public async Task<IActionResult> CreateGuest([FromBody] GuestSessionDto dto)
        {
            if (dto is null)
                return BadRequest("Не повинно бути нулем");

            var created = await _guest.CreateAsync(dto);
            if (created is null)
                return BadRequest("Не вдалося створити гостя");

            return Ok(created);
        }

        // --- UPDATE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("guests/{id:int}")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDto dto)
        {
            if (dto is null)
                return BadRequest("Не повинно бути нулем");

            var updated = await _guest.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound("Гостя не знайдено");

            return Ok(updated);
        }

        // --- DELETE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("guests/{id:int}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var ok = await _guest.DeleteAsync(id);
            if (!ok)
                return NotFound("Гостя не знайдено");

            return Ok("Видалено");
        }






    }
}
