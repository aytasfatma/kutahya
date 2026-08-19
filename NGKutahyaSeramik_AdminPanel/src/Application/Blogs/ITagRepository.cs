using Domain.Entities;

namespace Application.Blogs;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string name);

    Task<IReadOnlyList<Tag>> GetAllAsync();

    Task AddAsync(Tag tag);
}
