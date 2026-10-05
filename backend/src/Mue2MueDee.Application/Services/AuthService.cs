using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<LoginResponse> SwitchStoreAsync(SwitchStoreRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task RevokeTokenAsync(string token, string? ipAddress = null, CancellationToken ct = default);
    Task<LoginResponse> RegisterStoreWithOwnerAsync(RegisterRequest request, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthUserDto?> GetCurrentUserProfileAsync(CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentStore _currentStore;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentUser currentUser,
        ICurrentStore currentStore)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
        _currentStore = currentStore;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var normalizedInput = request.UsernameOrEmail.Trim().ToLower();
        var user = await _context.Users
            .Include(u => u.UserStores)
                .ThenInclude(us => us.Store)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedInput || u.Username.ToLower() == normalizedInput, ct);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("อีเมล/ชื่อผู้ใช้ หรือรหัสผ่านไม่ถูกต้อง");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("อีเมล/ชื่อผู้ใช้ หรือรหัสผ่านไม่ถูกต้อง");
        }

        // Determine target store
        UserStore? selectedUserStore = null;
        var activeUserStores = user.UserStores.Where(us => us.IsActive && us.Store.IsActive).ToList();

        if (request.StoreId.HasValue)
        {
            selectedUserStore = activeUserStores.FirstOrDefault(us => us.StoreId == request.StoreId.Value);
            if (selectedUserStore == null)
            {
                throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงร้านนี้");
            }
        }
        else
        {
            // Default to first store or owner store
            selectedUserStore = activeUserStores.OrderBy(us => us.Role == StoreRole.Owner ? 0 : 1).FirstOrDefault();
        }

        return await GenerateAuthResponseAsync(user, selectedUserStore, activeUserStores, ipAddress, ct);
    }

    public async Task<LoginResponse> SwitchStoreAsync(SwitchStoreRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("กรุณาเข้าสู่ระบบ");
        }

        var user = await _context.Users
            .Include(u => u.UserStores)
                .ThenInclude(us => us.Store)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, ct);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("ไม่พบข้อมูลผู้ใช้");
        }

        var activeUserStores = user.UserStores.Where(us => us.IsActive && us.Store.IsActive).ToList();
        var targetStore = activeUserStores.FirstOrDefault(us => us.StoreId == request.StoreId);

        if (targetStore == null)
        {
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงร้านนี้");
        }

        return await GenerateAuthResponseAsync(user, targetStore, activeUserStores, ipAddress, ct);
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.UserStores)
                    .ThenInclude(us => us.Store)
            .FirstOrDefaultAsync(r => r.TokenHash == request.RefreshToken, ct);

        if (token == null || !token.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token ไม่ถูกต้องหรือหมดอายุแล้ว");
        }

        // Revoke current token
        token.RevokedAt = DateTimeOffset.UtcNow;
        token.RevokedByIp = ipAddress;

        var user = token.User;
        var activeUserStores = user.UserStores.Where(us => us.IsActive && us.Store.IsActive).ToList();
        
        // Retain current store if possible
        UserStore? selectedUserStore = null;
        if (_currentStore.StoreId.HasValue)
        {
            selectedUserStore = activeUserStores.FirstOrDefault(us => us.StoreId == _currentStore.StoreId.Value);
        }
        selectedUserStore ??= activeUserStores.FirstOrDefault();

        return await GenerateAuthResponseAsync(user, selectedUserStore, activeUserStores, ipAddress, ct);
    }

    public async Task RevokeTokenAsync(string token, string? ipAddress = null, CancellationToken ct = default)
    {
        var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == token, ct);
        if (existingToken != null && existingToken.IsActive)
        {
            existingToken.RevokedAt = DateTimeOffset.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<LoginResponse> RegisterStoreWithOwnerAsync(RegisterRequest request, string? ipAddress = null, CancellationToken ct = default)
    {
        var emailLower = request.Email.Trim().ToLower();
        var usernameLower = request.Username.Trim().ToLower();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower || u.Username.ToLower() == usernameLower, ct))
        {
            throw new InvalidOperationException("อีเมลหรือชื่อผู้ใช้นี้ถูกใช้งานแล้ว");
        }

        var storeCode = !string.IsNullOrWhiteSpace(request.StoreCode) 
            ? request.StoreCode.Trim().ToUpper() 
            : request.StoreName.Trim().Substring(0, Math.Min(4, request.StoreName.Trim().Length)).ToUpper();

        var store = new Store
        {
            Name = request.StoreName.Trim(),
            Code = storeCode,
            Phone = request.PhoneNumber,
            Email = request.Email,
            IsActive = true
        };

        var user = new User
        {
            Email = emailLower,
            Username = usernameLower,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        var userStore = new UserStore
        {
            User = user,
            Store = store,
            Role = StoreRole.Owner,
            IsActive = true
        };

        _context.Stores.Add(store);
        _context.Users.Add(user);
        _context.UserStores.Add(userStore);

        await _context.SaveChangesAsync(ct);

        var activeUserStores = new List<UserStore> { userStore };
        return await GenerateAuthResponseAsync(user, userStore, activeUserStores, ipAddress, ct);
    }

    public async Task<AuthUserDto?> GetCurrentUserProfileAsync(CancellationToken ct = default)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var user = await _context.Users.FindAsync(new object[] { _currentUser.UserId.Value }, ct);
        if (user == null) return null;

        return new AuthUserDto(user.Id, user.Username, user.Email, user.FullName, user.AvatarUrl);
    }

    private async Task<LoginResponse> GenerateAuthResponseAsync(
        User user,
        UserStore? selectedUserStore,
        List<UserStore> activeUserStores,
        string? ipAddress,
        CancellationToken ct)
    {
        var storeId = selectedUserStore?.StoreId;
        var role = selectedUserStore?.Role;

        var accessToken = _jwtTokenService.GenerateAccessToken(user, storeId, role);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenString,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(ct);

        var availableStores = activeUserStores.Select(us => new UserStoreInfoDto(
            us.StoreId,
            us.Store.Name,
            us.Store.Code,
            us.Store.LogoUrl,
            us.Role,
            us.Store.PrimaryColor
        )).ToList();

        var authUser = new AuthUserDto(user.Id, user.Username, user.Email, user.FullName, user.AvatarUrl);

        return new LoginResponse(
            accessToken,
            refreshTokenString,
            DateTimeOffset.UtcNow.AddHours(2),
            authUser,
            storeId,
            role,
            availableStores
        );
    }
}
