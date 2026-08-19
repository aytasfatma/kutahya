using Domain.Entities;

namespace Application.Pages;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(int id);

    Task<IReadOnlyList<Page>> GetAllAsync();

    Task AddAsync(Page page);

    void Remove(Page page);
}
