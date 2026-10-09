using FluentValidation;
using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;
using static IPAY.Application.Validators.Shop.ProductValiadtor;

namespace IPAY.Application.Validators.Shop
{
    public class CreateSellerProductDtoValidator : AbstractValidator<CreateSellerProductDto>
    {
        public CreateSellerProductDtoValidator()
        {
            RuleFor(p => p.Name).NotEmpty().MaximumLength(120);

            RuleFor(p => p.Price)
                .Must(BeFinite).WithMessage("Цена должна быть числом.")
                .GreaterThan(0)
                .Must(HaveMax2Decimals).WithMessage("Не более 2 знаков после запятой.");

            RuleFor(p => p.DiscountedPrice)
                .Must(BeFinite)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(p => p.Price)
                    .WithMessage("Скидочная цена не может превышать обычную.")
                .Must(HaveMax2Decimals);

            RuleFor(p => p.Rating).Must(BeFinite).InclusiveBetween(0, 5);

            RuleFor(p => p.ImageUrl)
                .NotEmpty().MaximumLength(2048)
                .Must(BeValidHttpUrl).WithMessage("ImageUrl должен быть http/https ссылкой.");

            RuleFor(p => p.Manufacturer).NotEmpty().MaximumLength(100);
            RuleFor(p => p.CategoryId).GreaterThan(0);

            RuleFor(p => p.DiscountPercent).Must(BeFinite).InclusiveBetween(0, 100);
        }
    }
}
