using MediatR;

namespace BuildingBlocks.CQRS;


// Command that does not return any response
public interface ICommand : ICommand<Unit>
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}