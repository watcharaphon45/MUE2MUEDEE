using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public interface IStoreService
{
    Task<StoreDto> GetCurrentStoreAsync(CancellationToken ct = default);
    Task<StoreDto> UpdateCurrentStoreAsync(UpdateStoreRequest request, CancellationToken ct = default);
    Task<List<StoreUserDto>> GetStoreUsersAsync(CancellationToken ct = default);
    Task<StoreUserDto> AddUserToStoreAsync(AddUserToStoreRequest request, CancellationToken ct = default);
    Task<StoreUserDto> UpdateUserStoreRoleAsync(Guid targetUserId, UpdateUserStoreRoleRequest request, CancellationToken ct = default);
}

public class StoreService : IStoreService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public StoreService(
        IApplicationDbContext context,
        ICurrentStore currentStore,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentStore = currentStore;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<StoreDto> GetCurrentStoreAsync(CancellationToken ct = default)
    {
        EnsureStoreContext();
        var store = await _context.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == _currentStore.StoreId!.Value, ct);

        if (store == null) throw new KeyNotFoundException("ไม่พบข้อมูลร้านค้า");

        return MapToDto(store);
    }

    public async Task<StoreDto> UpdateCurrentStoreAsync(UpdateStoreRequest request, CancellationToken ct = default)
    {
        EnsureStoreOwnerOrAdmin();
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == _currentStore.StoreId!.Value, ct);
        if (store == null) throw new KeyNotFoundException("ไม่พบข้อมูลร้านค้า");

        store.Name = request.Name.Trim();
        store.LogoUrl = request.LogoUrl;
        store.Phone = request.Phone;
        store.Email = request.Email;
        store.Address = request.Address;
        store.TaxId = request.TaxId;
        store.Facebook = request.Facebook;
        store.Line = request.Line;
        store.Website = request.Website;
        store.PrimaryColor = string.IsNullOrWhiteSpace(request.PrimaryColor) ? "#4f46e5" : request.PrimaryColor;
        store.UpdatedAt = DateTimeOffset.UtcNow;
        store.UpdatedBy = _currentUser.Username;

        await _context.SaveChangesAsync(ct);
        return MapToDto(store);
    }

    public async Task<List<StoreUserDto>> GetStoreUsersAsync(CancellationToken ct = default)
    {
        EnsureStoreContext();
        var users = await _context.UserStores
            .AsNoTracking()
            .Include(us => us.User)
            .Where(us => us.StoreId == _currentStore.StoreId!.Value)
            .Select(us => new StoreUserDto(
                us.UserId,
                us.User.Username,
                us.User.FullName,
                us.User.Email,
                us.User.PhoneNumber,
                us.User.AvatarUrl,
                us.Role,
                us.IsActive && us.User.IsActive,
                us.CreatedAt
            ))
            .ToListAsync(ct);

        return users;
    }

    public async Task<StoreUserDto> AddUserToStoreAsync(AddUserToStoreRequest request, CancellationToken ct = default)
    {
        EnsureStoreOwnerOrAdmin();

        var emailLower = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

        if (user == null)
        {
            var username = emailLower.Split('@')[0];
            // Check if username taken
            var baseUsername = username;
            var counter = 1;
            while (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower(), ct))
            {
                username = $"{baseUsername}{counter++}";
            }

            user = new User
            {
                Email = emailLower,
                Username = username,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(request.TemporaryPassword),
                IsActive = true,
                CreatedBy = _currentUser.Username
            };
            _context.Users.Add(user);
        }

        var existingUserStore = await _context.UserStores
            .FirstOrDefaultAsync(us => us.StoreId == _currentStore.StoreId!.Value && us.UserId == user.Id, ct);

        if (existingUserStore != null)
        {
            throw new InvalidOperationException("ผู้ใช้นี้สังกัดในร้านค้านี้อยู่แล้ว");
        }

        var userStore = new UserStore
        {
            User = user,
            StoreId = _currentStore.StoreId!.Value,
            Role = request.Role,
            IsActive = true,
            CreatedBy = _currentUser.Username
        };

        _context.UserStores.Add(userStore);
        await _context.SaveChangesAsync(ct);

        return new StoreUserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.AvatarUrl,
            userStore.Role,
            userStore.IsActive,
            userStore.CreatedAt
        );
    }

    public async Task<StoreUserDto> UpdateUserStoreRoleAsync(Guid targetUserId, UpdateUserStoreRoleRequest request, CancellationToken ct = default)
    {
        EnsureStoreOwner();

        var userStore = await _context.UserStores
            .Include(us => us.User)
            .FirstOrDefaultAsync(us => us.StoreId == _currentStore.StoreId!.Value && us.UserId == targetUserId, ct);

        if (userStore == null) throw new KeyNotFoundException("ไม่พบผู้ใช้งานในร้านค้านี้");

        userStore.Role = request.Role;
        userStore.IsActive = request.IsActive;
        userStore.UpdatedAt = DateTimeOffset.UtcNow;
        userStore.UpdatedBy = _currentUser.Username;

        await _context.SaveChangesAsync(ct);

        return new StoreUserDto(
            userStore.UserId,
            userStore.User.Username,
            userStore.User.FullName,
            userStore.User.Email,
            userStore.User.PhoneNumber,
            userStore.User.AvatarUrl,
            userStore.Role,
            userStore.IsActive,
            userStore.CreatedAt
        );
    }

    private void EnsureStoreContext()
    {
        if (!_currentStore.StoreId.HasValue)
        {
            throw new UnauthorizedAccessException("ไม่พบข้อมูลร้านค้าปัจจุบัน กรุณาเลือกร้านค้าก่อนทำรายการ");
        }
    }

    private void EnsureStoreOwnerOrAdmin()
    {
        EnsureStoreContext();
        if (_currentStore.Role != StoreRole.Owner && _currentStore.Role != StoreRole.Admin)
        {
            throw new UnauthorizedAccessException("ต้องมีสิทธิ์ Owner หรือ Admin ในการดำเนินการนี้");
        }
    }

    private void EnsureStoreOwner()
    {
        EnsureStoreContext();
        if (_currentStore.Role != StoreRole.Owner)
        {
            throw new UnauthorizedAccessException("ต้องมีสิทธิ์ Owner ในการดำเนินการนี้");
        }
    }

    private static StoreDto MapToDto(Store s) => new(
        s.Id,
        s.Name,
        s.Code,
        s.LogoUrl,
        s.Phone,
        s.Email,
        s.Address,
        s.TaxId,
        s.Facebook,
        s.Line,
        s.Website,
        s.PrimaryColor,
        s.IsActive,
        s.CreatedAt
    );
}
