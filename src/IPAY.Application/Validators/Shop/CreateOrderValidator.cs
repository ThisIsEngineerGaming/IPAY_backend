
using FluentValidation;
using IPAY.Application.DTOs.Shop;

namespace IPAY.Application.Validators.Shop
{


    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Заказ должен содержать хотя бы один товар")
                .Must(items => items.Count <= 50).WithMessage("Слишком много товаров в заказе");

            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());
        }
    }




}
