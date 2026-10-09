using System;
using System.Collections.Generic;
using System.Text;
using IPAY.Application.DTOs.Auth;

namespace IPAY.Application.Interfaces.Auth
{




        public interface IGuestService
        {
            Task<IReadOnlyList<GuestDto>> GetAllAsync();

            Task<GuestDto?> GetByIdAsync(int id);

            /// <summary>
            /// Найти по SessionKey или создать нового.
            /// </summary>
            Task<GuestDto> GetOrCreateAsync(string? sessionKey, string? name = null);

            Task<GuestDto?> CreateAsync(GuestSessionDto dto);

            Task<GuestDto?> UpdateAsync(int id, UpdateGuestDto dto);

            Task<bool> DeleteAsync(int id);
        }
    }

