using Domain.Entities;

namespace Application.News;

public interface INewsCategoryRepository
{
    Task<NewsCategory?> GetByIdAsync(int id);

    Task<IReadOnlyList<NewsCategory>> GetAllAsync();

    Task AddAsync(NewsCategory newsCategory);

    void Remove(NewsCategory newsCategory);
}
