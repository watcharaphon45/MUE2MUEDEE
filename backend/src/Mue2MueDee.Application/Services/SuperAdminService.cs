using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public interface ISuperAdminService
{
    Task<List<SuperAdminStoreListItemDto>> GetStoresAsync(CancellationToken ct = default);
    Task<SuperAdminStoreListItemDto> CreateStoreWithOwnerAsync(CreateStoreWithOwnerRequest request, CancellationToken ct = default);
    Task<bool> ToggleStoreStatusAsync(Guid storeId, CancellationToken ct = default);
    Task<SuperAdminStatsDto> GetStatsAsync(CancellationToken ct = default);
}

public class SuperAdminService : ISuperAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public SuperAdminService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<List<SuperAdminStoreListItemDto>> GetStoresAsync(CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var stores = await _context.Stores
            .AsNoTracking()
            .Include(s => s.UserStores)
                .ThenInclude(us => us.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        return stores.Select(s =>
        {
            var ownerUs = s.UserStores.FirstOrDefault(us => us.Role == StoreRole.Owner && us.IsActive);
            var owner = ownerUs?.User;

            return new SuperAdminStoreListItemDto(
                s.Id,
                s.Name,
                s.Code,
                s.Phone,
                s.Email,
                s.Address,
                s.PrimaryColor,
                s.IsActive,
                s.CreatedAt,
                owner?.Id,
                owner?.FullName,
                owner?.Username,
                owner?.Email,
                owner?.PhoneNumber,
                s.UserStores.Count(us => us.IsActive)
            );
        }).ToList();
    }

    public async Task<SuperAdminStoreListItemDto> CreateStoreWithOwnerAsync(CreateStoreWithOwnerRequest request, CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        if (string.IsNullOrWhiteSpace(request.StoreName))
            throw new ArgumentException("กรุณาระบุชื่อร้านค้า");

        if (string.IsNullOrWhiteSpace(request.OwnerFullName))
            throw new ArgumentException("กรุณาระบุชื่อ-นามสกุลของเจ้าของร้าน");

        if (string.IsNullOrWhiteSpace(request.OwnerUsername))
            throw new ArgumentException("กรุณาระบุ Username ของเจ้าของร้าน");

        if (string.IsNullOrWhiteSpace(request.OwnerEmail))
            throw new ArgumentException("กรุณาระบุอีเมลของเจ้าของร้าน");

        if (string.IsNullOrWhiteSpace(request.OwnerPassword) || request.OwnerPassword.Length < 6)
            throw new ArgumentException("รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร");

        var normalizedUsername = request.OwnerUsername.Trim().ToLower();
        var normalizedEmail = request.OwnerEmail.Trim().ToLower();

        // Check duplicate user
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername, ct))
        {
            throw new InvalidOperationException($"ชื่อผู้ใช้ '{request.OwnerUsername}' มีอยู่ในระบบแล้ว กรุณาเลือกชื่อผู้ใช้อื่น");
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct))
        {
            throw new InvalidOperationException($"อีเมล '{request.OwnerEmail}' มีอยู่ในระบบแล้ว กรุณาใช้อีเมลอื่น");
        }

        // Generate / sanitize Store Code
        var storeCode = !string.IsNullOrWhiteSpace(request.StoreCode)
            ? request.StoreCode.Trim().ToUpper()
            : GenerateStoreCode(request.StoreName);

        // Ensure store code is unique
        var baseCode = storeCode;
        var codeCounter = 1;
        while (await _context.Stores.AnyAsync(s => s.Code == storeCode, ct))
        {
            storeCode = $"{baseCode}{codeCounter++}";
        }

        var store = new Store
        {
            Name = request.StoreName.Trim(),
            Code = storeCode,
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            PrimaryColor = string.IsNullOrWhiteSpace(request.PrimaryColor) ? "#4f46e5" : request.PrimaryColor.Trim(),
            IsActive = true,
            CreatedBy = _currentUser.Username ?? "superadmin"
        };

        var owner = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            FullName = request.OwnerFullName.Trim(),
            PhoneNumber = request.OwnerPhoneNumber?.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.OwnerPassword),
            IsActive = true,
            IsSuperAdmin = false,
            CreatedBy = _currentUser.Username ?? "superadmin"
        };

        var userStore = new UserStore
        {
            Store = store,
            User = owner,
            Role = StoreRole.Owner,
            IsActive = true,
            CreatedBy = _currentUser.Username ?? "superadmin"
        };

        _context.Stores.Add(store);
        _context.Users.Add(owner);
        _context.UserStores.Add(userStore);

        await _context.SaveChangesAsync(ct);

        return new SuperAdminStoreListItemDto(
            store.Id,
            store.Name,
            store.Code,
            store.Phone,
            store.Email,
            store.Address,
            store.PrimaryColor,
            store.IsActive,
            store.CreatedAt,
            owner.Id,
            owner.FullName,
            owner.Username,
            owner.Email,
            owner.PhoneNumber,
            1
        );
    }

    public async Task<bool> ToggleStoreStatusAsync(Guid storeId, CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeId, ct);
        if (store == null) throw new KeyNotFoundException("ไม่พบข้อมูลร้านค้า");

        store.IsActive = !store.IsActive;
        store.UpdatedAt = DateTimeOffset.UtcNow;
        store.UpdatedBy = _currentUser.Username ?? "superadmin";

        await _context.SaveChangesAsync(ct);
        return store.IsActive;
    }

    public async Task<SuperAdminStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var totalStores = await _context.Stores.CountAsync(ct);
        var activeStores = await _context.Stores.CountAsync(s => s.IsActive, ct);
        var totalOwners = await _context.UserStores
            .Where(us => us.Role == StoreRole.Owner && us.IsActive)
            .Select(us => us.UserId)
            .Distinct()
            .CountAsync(ct);
        var totalUsers = await _context.Users.CountAsync(ct);

        return new SuperAdminStatsDto(totalStores, activeStores, totalOwners, totalUsers);
    }

    private void EnsureSuperAdmin()
    {
        if (!_currentUser.IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("ต้องมีสิทธิ์ Super Admin เท่านั้นในการทำรายการนี้");
        }
    }

    private static string GenerateStoreCode(string name)
    {
        var clean = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (clean.Length >= 4)
        {
            return clean.Substring(0, Math.Min(6, clean.Length));
        }
        return $"STORE{new Random().Next(1000, 9999)}";
    }
}
