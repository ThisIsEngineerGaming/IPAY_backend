using FluentValidation;
using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Validators.Shop
{
    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId должен быть больше 0");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Количество должно быть больше 0")
                .LessThanOrEqualTo(100).WithMessage("Количество не может быть больше 100");
        }
    }
}
