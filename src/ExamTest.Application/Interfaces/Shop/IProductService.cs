using System.Collections.Generic;
using System.Threading.Tasks;
using ExamTest.Domain.Entities.Shop;

namespace ExamTest.Application.Interfaces.Shop
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(int id, Product product);
        Task DeleteAsync(int id);

        // "createCard(container:div)" on the diagram is a DOM/UI concern for the
        // frontend to implement - there's nothing meaningful for a backend to do there,
        // so it's intentionally left out here.
    }
}
