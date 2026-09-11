using ExamTest.Application.Interfaces.Auth;
using ExamTest.Domain.Entities.Auth;
using ExamTest.Domain.Entities.Media;
using ExamTest.Domain.Interfaces.ForRepos;
using Google.Rpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamTest.Application.Services.Auth
{
   public class SellerService:IUser<Seller>
    {
        private readonly IRepository<Seller> _sellerRepository;

        public SellerService(IRepository<Seller> sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public Task<IReadOnlyList<Seller>> GetAllAsync() => _sellerRepository.GetAllAsync();

        public Task<Seller?> GetByIdAsync(int id) => _sellerRepository.GetByIdAsync(id);

        public async Task<IReadOnlyList<Seller>> GetBySellersIdAsync(int sellerId)
        {
            var all = await _sellerRepository.GetAllAsync();
            return all.Where(e => e.Id == sellerId).ToList();
        }

        public async Task<Seller?> CreateAsync(Seller seller)
        {
            var all = await _sellerRepository.GetAllAsync();

            bool emailTaken = all.Any(s =>
                !string.IsNullOrEmpty(s.Email) &&
                s.Email.Equals(seller.Email, StringComparison.OrdinalIgnoreCase));

            if (emailTaken)
                return null;

            return await _sellerRepository.AddAsync(seller);
        }

        public Task UpdateAsync(int id, Seller seller) => _sellerRepository.UpdateAsync(id, seller);

        public Task DeleteAsync(int id) => _sellerRepository.DeleteAsync(id);
    }
}
