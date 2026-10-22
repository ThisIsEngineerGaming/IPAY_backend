using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Services.Auth
{
    //customer service inherited by interface IUser<T>
   public class CustomerService:IUser<Customer>
    {
        private readonly IRepository<Customer> _customerRepository;//here used a basic 

        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }
        //get all
        public Task<IReadOnlyList<Customer>> GetAllAsync() => _customerRepository.GetAllAsync();
        //get by id
        public Task<Customer?> GetByIdAsync(int id) => _customerRepository.GetByIdAsync(id);
        //this method allow us to find custoomer by email
        public async Task<Customer?> GetByEmail(string email)
        {
            var customers = await _customerRepository.GetAllAsync();//for searching
            //when cheking if email is suitable
            foreach (var customer in customers) { 
                if (!string.IsNullOrEmpty(customer.Email) &&
                    customer.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return customer;
                  
                    
                }
                
            }
            return null;
        }
        //making a list, where user has a seller id

        public async Task<IReadOnlyList<Customer>> GetByCustomersIdAsync(int sellerId)
        {
            var all = await _customerRepository.GetAllAsync();
            return all.Where(e => e.Id == sellerId).ToList();//process of filtering
        }
        //allows to create
        public async Task<Customer?> CreateAsync(Customer customer)
        {
            var all = await _customerRepository.GetAllAsync();
            //cheking if email is not taken by another user
            bool emailTaken = all.Any(s =>
                !string.IsNullOrEmpty(s.Email) &&
                s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

            if (emailTaken)
                return null;

            return await _customerRepository.AddAsync(customer);
        }
        //updates user
        public Task UpdateAsync(int id, Customer customer) => _customerRepository.UpdateAsync(id, customer);
        //deletes user
        public Task DeleteAsync(int id) => _customerRepository.DeleteAsync(id);
    }
}
