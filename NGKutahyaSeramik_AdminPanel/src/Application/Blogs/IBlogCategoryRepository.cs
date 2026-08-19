using Domain.Entities;

namespace Application.Blogs;

public interface IBlogCategoryRepository
{
    Task<BlogCategory?> GetByIdAsync(int id);

    Task<IReadOnlyList<BlogCategory>> GetAllAsync();

    Task AddAsync(BlogCategory blogCategory);

    void Remove(BlogCategory blogCategory);
}
