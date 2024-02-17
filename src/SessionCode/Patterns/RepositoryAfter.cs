using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SessionCode.Artifacts;

namespace SessionCode.Patterns;

public class RepositoryAfter(ICustomerRepository customerRepository)
{
    public async Task DoStuff(Guid customerId)
    {
        var customer = await customerRepository.GetByIdWithOrders(customerId);
        // Do Stuff
        await customerRepository.Save(customer);
    }
}

#region Supporting Classes

public interface ICustomerRepository
{
    Task<Customer?> GetById(Guid id);
    Task<Customer?> GetByIdWithOrders(Guid id);
    Task Save(Customer customer);

    Task<IReadOnlyList<Customer>> Get(CustomerSpecification spec);
}

public class CustomerRepository(CustomerDbContext context, TimeProvider timeProvider, ILogger<CustomerRepository> logger) : ICustomerRepository
{
    public async Task<Customer?> GetById(Guid id)
    {
        return await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByIdWithOrders(Guid id)
    {
        return await context.Customers.
            AsNoTracking().
            Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Save(Customer customer)
    {
        try
        {
            customer.LastChanged = timeProvider.GetUtcNow();
            context.Update(customer);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e,"Oh das ist schlecht bei {CustomerId}", customer.Id, customer );
        }
        
    }

    public async Task<IReadOnlyList<Customer>> Get(CustomerSpecification spec)
    {
        var query = context.Customers.AsNoTracking();
        if (spec.LastChangedUntil.HasValue)
        {
            query = query.Where(c => c.LastChanged <= spec.LastChangedUntil.Value);
        }
        
        // More filters

        return await query.ToListAsync();
    }
}

public class CustomerSpecification
{
    public DateTime? LastChangedUntil { get; set; }
    public List<Guid> Ids { get; set; }
    public bool? HasOrders { get; set; }
}

#endregion