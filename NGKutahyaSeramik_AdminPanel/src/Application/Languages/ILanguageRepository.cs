using Domain.Entities;

namespace Application.Languages;

public interface ILanguageRepository
{
    Task<Language?> GetByIdAsync(int id);

    Task<IReadOnlyList<Language>> GetAllAsync();
}
