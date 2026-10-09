using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public interface IStickerTemplateService
{
    Task<List<StickerTemplateDto>> GetAllAsync();
    Task<StickerTemplateDto> GetByIdAsync(Guid id);
    Task<StickerTemplateDto?> GetDefaultAsync();
    Task<StickerTemplateDto> CreateAsync(CreateStickerTemplateDto dto);
    Task<StickerTemplateDto> UpdateAsync(Guid id, UpdateStickerTemplateDto dto);
    Task<StickerTemplateDto> DuplicateAsync(Guid id);
    Task SetDefaultAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task ResetToSystemDefaultsAsync();
}
