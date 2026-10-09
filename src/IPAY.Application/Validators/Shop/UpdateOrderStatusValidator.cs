using FluentValidation;
using IPAY.Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Application.Validators.Shop
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Некорректный статус заказа");
        }
    }
}
