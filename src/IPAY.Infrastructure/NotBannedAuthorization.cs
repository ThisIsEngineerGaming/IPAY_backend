using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using IPAY.Application.Services.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users; // поправьте namespace под реальное расположение ICustomerService

namespace IPAY.Infrastructure.Authorization // поправьте namespace под вашу структуру проекта
{
    public class NotBannedRequirement : IAuthorizationRequirement { }

    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly IUser<Customer> _customerService;

        public NotBannedHandler(IUser<Customer> customerService)
        {
            _customerService = customerService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, NotBannedRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return;

            var id = Convert.ToInt32(userId);

            var customer = await _customerService.GetByIdAsync(id);
            if (customer is not null && !customer.IsBanned)
            {
                context.Succeed(requirement);
            }
        }
    }
}
