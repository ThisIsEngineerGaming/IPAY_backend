using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Domain.Interfaces.ForRepos.Shop
{
    //adress repo(took over base repo)
    public interface IAddressRepository : IRepository<Address>
    {
        Task<IReadOnlyList<Address>> GetByUserIdAsync(int userId);
        Task<Address?> GetByUserIdAndIdAsync(int userId, int addressId);
    }
}
