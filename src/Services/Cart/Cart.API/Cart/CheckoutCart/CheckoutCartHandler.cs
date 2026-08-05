
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Cart.API.Basket.CheckoutCart;

public record CheckoutCartCommand(CartCheckoutDto CartCheckoutDto) 
    : ICommand<CheckoutCartResult>;
public record CheckoutCartResult(bool IsSuccess);

public class CheckoutCartCommandValidator 
    : AbstractValidator<CheckoutCartCommand>
{
    public CheckoutCartCommandValidator()
    {
        RuleFor(x => x.CartCheckoutDto).NotNull().WithMessage("CartCheckoutDto can't be null");
        RuleFor(x => x.CartCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required");
    }
}

public class CheckoutCartCommandHandler
    (ICartRepository repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutCartCommand, CheckoutCartResult>
{
    public async Task<CheckoutCartResult> Handle(CheckoutCartCommand command, CancellationToken cancellationToken)
    {
        // get existing basket with total price
        // Set totalprice on basketcheckout event message
        // send basket checkout event to rabbitmq using masstransit
        // delete the basket

        var basket = await repository.GetCart(command.CartCheckoutDto.UserName, cancellationToken);
        if (basket == null)
        {
            return new CheckoutCartResult(false);
        }

        var eventMessage = command.CartCheckoutDto.Adapt<CartCheckoutEvent>();
        eventMessage.TotalPrice = basket.TotalPrice;

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        await repository.DeleteCart(command.CartCheckoutDto.UserName, cancellationToken);

        return new CheckoutCartResult(true);
    }
}
