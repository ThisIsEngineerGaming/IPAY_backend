

using AutoMapper;
using FluentValidation;
using IPAY.Application.DTOs.Shop;
using IPAY.Application.Interfaces.Shop;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;

namespace IPAY.Application.Services.Shop
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepo _productRepository;
        private readonly IMapper _mapper;


        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepo productRepository,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _mapper = mapper;
  
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto, int userId)
        {
   

            var product = await _productRepository.GetProductByIdAsync(dto.ProductId)
                ?? throw new KeyNotFoundException($"Product with id {dto.ProductId} not found");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            review = await _reviewRepository.AddAsync(review);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review is null ? null : _mapper.Map<ReviewDto>(review);
        }

        public async Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }

        public async Task DeleteAsync(int id, int userId, bool isAdmin = false)
        {
            var review = await _reviewRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Review with id {id} not found");

            // Удалять может автор или админ
            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own review");

            await _reviewRepository.DeleteAsync(id);
        }
    }
}
