using Domain.Entities;

namespace Application.Dealers;

public interface IDealerRepository
{
    Task<Dealer?> GetByIdAsync(int id);

    Task<IReadOnlyList<Dealer>> GetAllAsync();

    Task<IReadOnlyList<Dealer>> GetFilteredAsync(DealerQuery query);

    Task AddAsync(Dealer dealer);

    void Remove(Dealer dealer);
}
