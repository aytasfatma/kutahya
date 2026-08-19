using Application.Categories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private const string TrLanguageCode = "TR";

    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Category?> GetByIdAsync(int id) =>
        _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Category>> GetAllAsync() =>
        await _dbContext.Categories.AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<CategoryOptionDto>> GetOptionItemsAsync() =>
        await _dbContext.Categories
            .AsNoTracking()
            .Select(c => new CategoryOptionDto
            {
                Id = c.Id,
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                DisplayName = c.Name
            })
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.DisplayName)
            .ThenBy(c => c.Id)
            .ToListAsync();

    public async Task<IReadOnlyList<Category>> GetByParentIdAsync(int? parentCategoryId) =>
        await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .ToListAsync();

    public Task<bool> HasChildrenAsync(int categoryId) =>
        _dbContext.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);

    public async Task AddAsync(Category category) =>
        await _dbContext.Categories.AddAsync(category);

    public void Remove(Category category) =>
        _dbContext.Categories.Remove(category);
}
