using MediatR;
using SessionCode.Artifacts;

namespace SessionCode.Patterns;

#region Definition
public record CustomerQuery : IRequest<CustomerQueryResult>
{
    public bool? HasOrders { get; set; }
    public string? StartsWith { get; set; }
}

public record CustomerQueryResult(Customer? Customer);

public class CustomerQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<CustomerQuery, CustomerQueryResult>
{
    public async Task<CustomerQueryResult> Handle(CustomerQuery request, CancellationToken cancellationToken)
    {
        var customers = await customerRepository.Get(new());

        return new(customers.FirstOrDefault());
    }
}

#endregion

#region Usage

public class MediatorSample(IMediator mediator)
{
    public async Task DoStuff()
    {
        var result = await mediator.Send(new CustomerQuery());
        
    }
}

#endregion