using Domain.Entities;

namespace Application.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);

    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<IReadOnlyList<CategoryOptionDto>> GetOptionItemsAsync();

    Task<IReadOnlyList<Category>> GetByParentIdAsync(int? parentCategoryId);

    Task<bool> HasChildrenAsync(int categoryId);

    Task AddAsync(Category category);

    void Remove(Category category);
}
