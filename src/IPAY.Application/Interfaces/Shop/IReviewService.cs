// src/IPAY.Application/Interfaces/Shop/IReviewService.cs

using IPAY.Application.DTOs.Shop;

namespace IPAY.Application.Interfaces.Shop
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateAsync(CreateReviewDto dto, int userId);
        Task<ReviewDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId);
        Task DeleteAsync(int id, int userId, bool isAdmin = false);
    }
}
