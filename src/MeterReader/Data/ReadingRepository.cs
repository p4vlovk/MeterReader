namespace MeterReader.Data;

public class ReadingRepository : IReadingRepository
{
    private readonly ReadingContext context;

    public ReadingRepository(ReadingContext context)
        => this.context = context;

    public async Task<IEnumerable<Customer>> GetCustomersAsync()
        => await this.context
            .Customers
            .Include(c => c.Address)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IEnumerable<Customer>> GetCustomersWithReadingsAsync()
        => await this.context
            .Customers
            .Include(c => c.Address)
            .Include(c => c.Readings)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Customer?> GetCustomerAsync(int id)
        => await this.context
            .Customers
            .Include(c => c.Address)
            .OrderBy(c => c.Name)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

    public async Task<Customer?> GetCustomerWithReadingsAsync(int id)
        => await this.context
            .Customers
            .Include(c => c.Address)
            .Include(c => c.Readings)
            .OrderBy(c => c.Name)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

    public void AddEntity<T>(T model)
        where T : notnull
        => this.context.Add(model);

    public void DeleteEntity<T>(T model)
        where T : notnull
        => this.context.Remove(model);

    public async Task<bool> SaveAllAsync()
        => await this.context.SaveChangesAsync() > 0;
}