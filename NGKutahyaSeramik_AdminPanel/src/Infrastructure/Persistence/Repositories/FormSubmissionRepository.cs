using Application.Forms;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>Görev talimatı gereği tüm kayıtları belleğe çekip filtrelemek YASAK — bu repository
/// gerçek SQL seviyesinde `.Where`/`.OrderBy`/`.Skip`/`.Take` kullanan projedeki ilk örnek.</summary>
public class FormSubmissionRepository : IFormSubmissionRepository
{
    private readonly AppDbContext _dbContext;

    public FormSubmissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FormSubmission?> GetByIdAsync(int id) =>
        _dbContext.FormSubmissions.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<(IReadOnlyList<FormSubmission> Items, int TotalCount)> GetPagedAsync(FormSubmissionQuery query)
    {
        var filtered = _dbContext.FormSubmissions.AsNoTracking().AsQueryable();

        if (query.FormType.HasValue)
        {
            filtered = filtered.Where(f => f.FormType == query.FormType.Value);
        }

        if (query.IsRead.HasValue)
        {
            filtered = filtered.Where(f => f.IsRead == query.IsRead.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            filtered = filtered.Where(f => f.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            filtered = filtered.Where(f => f.CreatedAt <= query.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            filtered = filtered.Where(f =>
                f.FullName.Contains(term) || f.Email.Contains(term) || f.Phone.Contains(term));
        }

        var totalCount = await filtered.CountAsync();

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        var items = await filtered
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(FormSubmission submission) =>
        await _dbContext.FormSubmissions.AddAsync(submission);

    public void Remove(FormSubmission submission) =>
        _dbContext.FormSubmissions.Remove(submission);
}
