using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.DTOs.Auth
{
    public  class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }= string.Empty;

        // Only populated when this DTO is reused as AuthResponse.User (a login
        // *request* has no Name - LoginDtoService fills this in from the
        // matched Customer before building the response).
        public string? Name { get; set; }

    }
}
