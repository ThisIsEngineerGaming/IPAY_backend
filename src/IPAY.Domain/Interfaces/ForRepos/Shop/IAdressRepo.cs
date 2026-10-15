using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<IReadOnlyList<Address>> GetByUserIdAsync(int userId);
        Task<Address?> GetByUserIdAndIdAsync(int userId, int addressId);
    }
}
