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
        private readonly ILogger<AuthController>? _logger;

        private readonly IGuestService _guest;

        private readonly IPasswordHasher<Customer> _passwordHasher;

        public AdminController( IUser<Customer> customer,  IGuestService guest,IPasswordHasher<Customer> passwordHasher, ILogger<AuthController>? logger = null)
        {
            _logger = logger;
            _customer = customer;
            _guest = guest;
            _passwordHasher= passwordHasher;
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


        // --- CREATE ---
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer is null)
                return BadRequest("Не повинно бути нулем");

            // пароль хешируем, если передали сырой
            if (!string.IsNullOrWhiteSpace(customer.Password))
                customer.Password = _passwordHasher.HashPassword(customer,customer.Password);

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
                existing.Password = _passwordHasher.HashPassword(customer, customer.Password);

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


        // --- SESSION (публичный: создать / найти гостя) ---
        [HttpPost("guests/session")]
        public async Task<IActionResult> EnsureGuestSession([FromBody] GuestSessionDto? dto)
        {
            var guest = await _guest.GetOrCreateAsync(dto?.SessionKey, dto?.Name);

            if (guest.IsBanned)
                return StatusCode(403, "Заблоковано");

            return Ok(guest);
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
