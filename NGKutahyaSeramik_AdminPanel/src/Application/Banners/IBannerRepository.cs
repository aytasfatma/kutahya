using Domain.Entities;

namespace Application.Banners;

public interface IBannerRepository
{
    Task<Banner?> GetByIdAsync(int id);

    Task<IReadOnlyList<Banner>> GetAllAsync();

    Task AddAsync(Banner banner);

    void Remove(Banner banner);
}
