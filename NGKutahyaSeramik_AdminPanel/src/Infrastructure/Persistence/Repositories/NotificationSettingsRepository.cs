using Application.Forms;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;
public sealed class NotificationSettingsRepository : INotificationSettingsRepository
{
    private readonly AppDbContext _db; public NotificationSettingsRepository(AppDbContext db) => _db = db;
    public Task<NotificationSettings?> GetAsync() => _db.NotificationSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
    public Task AddAsync(NotificationSettings settings) => _db.NotificationSettings.AddAsync(settings).AsTask();
}
