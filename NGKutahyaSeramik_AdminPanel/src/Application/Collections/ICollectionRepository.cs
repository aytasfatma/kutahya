using Domain.Entities;

namespace Application.Collections;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(int id);

    Task<IReadOnlyList<Collection>> GetAllAsync();

    Task<IReadOnlyList<CollectionOptionDto>> GetOptionItemsAsync();

    Task AddAsync(Collection collection);

    void Remove(Collection collection);
}
