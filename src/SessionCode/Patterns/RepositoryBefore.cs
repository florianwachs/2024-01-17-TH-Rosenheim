using Microsoft.EntityFrameworkCore;
using SessionCode.Artifacts;

namespace SessionCode.Patterns;

#region Step1

public class RepositoryBefore1(CustomerDbContext context)
{
    public async Task DoStuff(Guid customerId)
    {
        var customer = await context.Customers.FindAsync(customerId);
        // Do Stuff
        await context.SaveChangesAsync();
    }
}

#endregion

#region Step2

public class RepositoryBefore2(CustomerDbContext context)
{
    public async Task DoStuff1(Guid customerId)
    {
        var customer = await context.Customers.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == customerId);
        // DoStuff

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // Log
            throw;
        }
    }
}

#endregion