using Domain.Entities;

namespace Application.Forms;

public interface IFormSubmissionRepository
{
    Task<FormSubmission?> GetByIdAsync(int id);

    /// <summary>Gerçek SQL seviyesinde filtreleme + sayfalama (bkz. FormSubmissionQuery) — tüm
    /// kayıtları çekip in-memory filtrelemek YASAK (görev talimatı, form kayıtları büyüyen veri seti).</summary>
    Task<(IReadOnlyList<FormSubmission> Items, int TotalCount)> GetPagedAsync(FormSubmissionQuery query);

    Task AddAsync(FormSubmission submission);

    void Remove(FormSubmission submission);
}
