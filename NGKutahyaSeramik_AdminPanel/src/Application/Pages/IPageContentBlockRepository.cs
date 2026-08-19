using Domain.Entities;

namespace Application.Pages;

public interface IPageContentBlockRepository
{
    Task<PageContentBlock?> GetByIdAsync(int id);

    Task<IReadOnlyList<PageContentBlock>> GetByPageIdAsync(int pageId);

    Task AddAsync(PageContentBlock block);

    void Remove(PageContentBlock block);

    void RemoveRange(IEnumerable<PageContentBlock> blocks);
}
