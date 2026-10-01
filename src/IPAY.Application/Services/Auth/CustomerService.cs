using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Services.Auth
{
   public class CustomerService:IUser<Customer>
    {
        private readonly IRepository<Customer> _customerRepository;

        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync() => _customerRepository.GetAllAsync();

        public Task<Customer?> GetByIdAsync(int id) => _customerRepository.GetByIdAsync(id);

        public async Task<Customer?> GetByEmail(string email)
        {
            var customers = await _customerRepository.GetAllAsync();
            foreach (var customer in customers) { 
                if (!string.IsNullOrEmpty(customer.Email) &&
                    customer.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return customer;
                  
                    
                }
                
            }
            return null;
        }

        public async Task<IReadOnlyList<Customer>> GetByCustomersIdAsync(int sellerId)
        {
            var all = await _customerRepository.GetAllAsync();
            return all.Where(e => e.Id == sellerId).ToList();
        }

        public async Task<Customer?> CreateAsync(Customer customer)
        {
            var all = await _customerRepository.GetAllAsync();

            bool emailTaken = all.Any(s =>
                !string.IsNullOrEmpty(s.Email) &&
                s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

            if (emailTaken)
                return null;

            return await _customerRepository.AddAsync(customer);
        }

        public Task UpdateAsync(int id, Customer customer) => _customerRepository.UpdateAsync(id, customer);

        public Task DeleteAsync(int id) => _customerRepository.DeleteAsync(id);
    }
}
