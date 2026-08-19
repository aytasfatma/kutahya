using Domain.Entities;

namespace Application.Dealers;

/// <summary>
/// Madde 25 — Bayi Yönetimi. Projedeki ilk servis: Translation KULLANMIYOR (Madde 25.1'in hiçbir alanı
/// multi-lang değil) ve IFileStorageService KULLANMIYOR (dokümanda görsel/logo alanı yok). Standalone —
/// hiçbir başka modüle FK/M2M ilişkisi yok (Banner'dan bile daha yalın).
/// </summary>
public class DealerService
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DealerService(IDealerRepository dealerRepository, IUnitOfWork unitOfWork)
    {
        _dealerRepository = dealerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DealerDto>> GetAllAsync()
    {
        var dealers = await _dealerRepository.GetAllAsync();
        return dealers.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<DealerDto>> GetFilteredAsync(DealerQuery query)
    {
        var dealers = await _dealerRepository.GetFilteredAsync(query);
        return dealers.Select(MapToDto).ToList();
    }

    public async Task<DealerDto?> GetByIdAsync(int id)
    {
        var dealer = await _dealerRepository.GetByIdAsync(id);
        return dealer is null ? null : MapToDto(dealer);
    }

    public async Task<DealerOperationResult> CreateAsync(CreateDealerRequest request)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return DealerOperationResult.Failure(validation);
        }

        var dealer = new Dealer(
            request.Name.Trim(),
            request.City.Trim(),
            request.Category,
            Normalize(request.District),
            Normalize(request.Address),
            Normalize(request.Phone),
            Normalize(request.Fax),
            Normalize(request.Email),
            Normalize(request.WorkingHours),
            request.Latitude,
            request.Longitude,
            Normalize(request.Region),
            Normalize(request.RegionName),
            request.Brands);

        await _dealerRepository.AddAsync(dealer);
        await _unitOfWork.SaveChangesAsync();

        return DealerOperationResult.Success();
    }

    public async Task<DealerOperationResult> UpdateAsync(int id, UpdateDealerRequest request)
    {
        var dealer = await _dealerRepository.GetByIdAsync(id);
        if (dealer is null)
        {
            return DealerOperationResult.Failure("Bayi/showroom kaydı bulunamadı.");
        }

        var validation = Validate(request);
        if (validation is not null)
        {
            return DealerOperationResult.Failure(validation);
        }

        dealer.UpdateDetails(
            request.Name.Trim(),
            request.City.Trim(),
            request.Category,
            Normalize(request.District),
            Normalize(request.Address),
            Normalize(request.Phone),
            Normalize(request.Fax),
            Normalize(request.Email),
            Normalize(request.WorkingHours),
            request.Latitude,
            request.Longitude,
            Normalize(request.Region),
            Normalize(request.RegionName),
            request.Brands);

        await _unitOfWork.SaveChangesAsync();

        return DealerOperationResult.Success();
    }

    public async Task<DealerOperationResult> ToggleActiveAsync(int id)
    {
        var dealer = await _dealerRepository.GetByIdAsync(id);
        if (dealer is null)
        {
            return DealerOperationResult.Failure("Bayi/showroom kaydı bulunamadı.");
        }

        if (dealer.IsActive)
        {
            dealer.Deactivate();
        }
        else
        {
            dealer.Activate();
        }

        await _unitOfWork.SaveChangesAsync();
        return DealerOperationResult.Success();
    }

    public async Task<DealerOperationResult> DeleteAsync(int id)
    {
        var dealer = await _dealerRepository.GetByIdAsync(id);
        if (dealer is null)
        {
            return DealerOperationResult.Failure("Bayi/showroom kaydı bulunamadı.");
        }

        _dealerRepository.Remove(dealer);
        await _unitOfWork.SaveChangesAsync();

        return DealerOperationResult.Success();
    }

    private static string? Validate(DealerRequestBase request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Bayi/showroom adı zorunludur.";
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            return "Şehir zorunludur.";
        }

        var hasLatitude = request.Latitude.HasValue;
        var hasLongitude = request.Longitude.HasValue;

        if (hasLatitude != hasLongitude)
        {
            return "Enlem ve boylam birlikte girilmeli veya ikisi de boş bırakılmalıdır.";
        }

        if (hasLatitude && (request.Latitude!.Value < -90 || request.Latitude.Value > 90))
        {
            return "Enlem değeri -90 ile 90 arasında olmalıdır.";
        }

        if (hasLongitude && (request.Longitude!.Value < -180 || request.Longitude.Value > 180))
        {
            return "Boylam değeri -180 ile 180 arasında olmalıdır.";
        }

        return null;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DealerDto MapToDto(Dealer dealer) => new()
    {
        Id = dealer.Id,
        Name = dealer.Name,
        City = dealer.City,
        Category = dealer.Category,
        District = dealer.District,
        Address = dealer.Address,
        Phone = dealer.Phone,
        Fax = dealer.Fax,
        Email = dealer.Email,
        WorkingHours = dealer.WorkingHours,
        Latitude = dealer.Latitude,
        Longitude = dealer.Longitude,
        Region = dealer.Region,
        RegionName = dealer.RegionName,
        IsActive = dealer.IsActive,
        Brands = dealer.Brands
    };
}
