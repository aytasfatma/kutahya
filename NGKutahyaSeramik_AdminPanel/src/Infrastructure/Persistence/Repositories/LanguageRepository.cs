using Application.Languages;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _dbContext;

    public LanguageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Language?> GetByIdAsync(int id) =>
        _dbContext.Languages.FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IReadOnlyList<Language>> GetAllAsync() =>
        await _dbContext.Languages.AsNoTracking().ToListAsync();
}
