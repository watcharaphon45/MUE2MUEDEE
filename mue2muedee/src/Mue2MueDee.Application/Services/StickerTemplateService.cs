using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Application.Services;

public class StickerTemplateService : IStickerTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;

    public StickerTemplateService(IApplicationDbContext context, ICurrentStore currentStore)
    {
        _context = context;
        _currentStore = currentStore;
    }

    private Guid RequireStoreId()
    {
        if (!_currentStore.StoreId.HasValue || _currentStore.StoreId.Value == Guid.Empty)
        {
            throw AppException.Forbidden("กรุณาเลือกร้านค้าก่อนทำรายการ");
        }
        return _currentStore.StoreId.Value;
    }

    public async Task<List<StickerTemplateDto>> GetAllAsync()
    {
        var storeId = RequireStoreId();
        var templates = await _context.StickerTemplates
            .Where(t => t.StoreId == storeId)
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.ColumnsCount)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return templates.Select(MapToDto).ToList();
    }

    public async Task<StickerTemplateDto> GetByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var template = await _context.StickerTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);

        if (template == null)
        {
            throw AppException.NotFound($"ไม่พบรูปแบบสติ๊กเกอร์รหัส {id}");
        }

        return MapToDto(template);
    }

    public async Task<StickerTemplateDto?> GetDefaultAsync()
    {
        var storeId = RequireStoreId();
        var template = await _context.StickerTemplates
            .Where(t => t.StoreId == storeId && t.IsDefault)
            .FirstOrDefaultAsync();

        if (template == null)
        {
            template = await _context.StickerTemplates
                .Where(t => t.StoreId == storeId)
                .OrderBy(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        return template != null ? MapToDto(template) : null;
    }

    public async Task<StickerTemplateDto> CreateAsync(CreateStickerTemplateDto dto)
    {
        var storeId = RequireStoreId();
        ValidateDto(dto);

        if (dto.IsDefault)
        {
            await UnsetOtherDefaultsAsync(storeId, null);
        }
        else
        {
            // If this is the only template for the store, make it default automatically
            var count = await _context.StickerTemplates.CountAsync(t => t.StoreId == storeId);
            if (count == 0)
            {
                dto.IsDefault = true;
            }
        }

        var template = new StickerTemplate
        {
            StoreId = storeId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsDefault = dto.IsDefault,
            IsContinuousRoll = dto.IsContinuousRoll,
            PaperWidthMm = dto.PaperWidthMm,
            PaperHeightMm = dto.PaperHeightMm,
            LabelWidthMm = dto.LabelWidthMm,
            LabelHeightMm = dto.LabelHeightMm,
            ColumnsCount = dto.ColumnsCount,
            RowsCount = dto.RowsCount,
            HorizontalGapMm = dto.HorizontalGapMm,
            VerticalGapMm = dto.VerticalGapMm,
            MarginTopMm = dto.MarginTopMm,
            MarginBottomMm = dto.MarginBottomMm,
            MarginLeftMm = dto.MarginLeftMm,
            MarginRightMm = dto.MarginRightMm,
            ShowStoreLogo = dto.ShowStoreLogo,
            StoreLogoUrl = dto.StoreLogoUrl,
            MonochromeLogo = dto.MonochromeLogo,
            ShowStoreName = dto.ShowStoreName,
            StoreName = dto.StoreName,
            ShowProductTitle = dto.ShowProductTitle,
            ShowConditionGrade = dto.ShowConditionGrade,
            ShowSerialNumber = dto.ShowSerialNumber,
            ShowSku = dto.ShowSku,
            ShowPrice = dto.ShowPrice,
            ShowShutterCount = dto.ShowShutterCount,
            ShowFooterText = dto.ShowFooterText,
            FooterText = dto.FooterText,
            CodeStyle = dto.CodeStyle,
            EncodeValue = dto.EncodeValue,
            BarcodeHeightMm = dto.BarcodeHeightMm,
            BarcodeWidthRatio = dto.BarcodeWidthRatio,
            QrSizeMm = dto.QrSizeMm,
            ShowBarcodeText = dto.ShowBarcodeText
        };

        _context.StickerTemplates.Add(template);
        await _context.SaveChangesAsync();

        return MapToDto(template);
    }

    public async Task<StickerTemplateDto> UpdateAsync(Guid id, UpdateStickerTemplateDto dto)
    {
        var storeId = RequireStoreId();
        ValidateDto(dto);

        var template = await _context.StickerTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);

        if (template == null)
        {
            throw AppException.NotFound($"ไม่พบรูปแบบสติ๊กเกอร์รหัส {id}");
        }

        if (dto.IsDefault && !template.IsDefault)
        {
            await UnsetOtherDefaultsAsync(storeId, id);
        }

        template.Name = dto.Name.Trim();
        template.Description = dto.Description?.Trim();
        template.IsDefault = dto.IsDefault;
        template.IsContinuousRoll = dto.IsContinuousRoll;
        template.PaperWidthMm = dto.PaperWidthMm;
        template.PaperHeightMm = dto.PaperHeightMm;
        template.LabelWidthMm = dto.LabelWidthMm;
        template.LabelHeightMm = dto.LabelHeightMm;
        template.ColumnsCount = dto.ColumnsCount;
        template.RowsCount = dto.RowsCount;
        template.HorizontalGapMm = dto.HorizontalGapMm;
        template.VerticalGapMm = dto.VerticalGapMm;
        template.MarginTopMm = dto.MarginTopMm;
        template.MarginBottomMm = dto.MarginBottomMm;
        template.MarginLeftMm = dto.MarginLeftMm;
        template.MarginRightMm = dto.MarginRightMm;
        template.ShowStoreLogo = dto.ShowStoreLogo;
        template.StoreLogoUrl = dto.StoreLogoUrl;
        template.MonochromeLogo = dto.MonochromeLogo;
        template.ShowStoreName = dto.ShowStoreName;
        template.StoreName = dto.StoreName;
        template.ShowProductTitle = dto.ShowProductTitle;
        template.ShowConditionGrade = dto.ShowConditionGrade;
        template.ShowSerialNumber = dto.ShowSerialNumber;
        template.ShowSku = dto.ShowSku;
        template.ShowPrice = dto.ShowPrice;
        template.ShowShutterCount = dto.ShowShutterCount;
        template.ShowFooterText = dto.ShowFooterText;
        template.FooterText = dto.FooterText;
        template.CodeStyle = dto.CodeStyle;
        template.EncodeValue = dto.EncodeValue;
        template.BarcodeHeightMm = dto.BarcodeHeightMm;
        template.BarcodeWidthRatio = dto.BarcodeWidthRatio;
        template.QrSizeMm = dto.QrSizeMm;
        template.ShowBarcodeText = dto.ShowBarcodeText;

        await _context.SaveChangesAsync();

        return MapToDto(template);
    }

    public async Task<StickerTemplateDto> DuplicateAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var original = await _context.StickerTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);

        if (original == null)
        {
            throw AppException.NotFound($"ไม่พบรูปแบบสติ๊กเกอร์รหัส {id}");
        }

        var copy = new StickerTemplate
        {
            StoreId = storeId,
            Name = $"{original.Name} (สำเนา)",
            Description = original.Description,
            IsDefault = false,
            IsContinuousRoll = original.IsContinuousRoll,
            PaperWidthMm = original.PaperWidthMm,
            PaperHeightMm = original.PaperHeightMm,
            LabelWidthMm = original.LabelWidthMm,
            LabelHeightMm = original.LabelHeightMm,
            ColumnsCount = original.ColumnsCount,
            RowsCount = original.RowsCount,
            HorizontalGapMm = original.HorizontalGapMm,
            VerticalGapMm = original.VerticalGapMm,
            MarginTopMm = original.MarginTopMm,
            MarginBottomMm = original.MarginBottomMm,
            MarginLeftMm = original.MarginLeftMm,
            MarginRightMm = original.MarginRightMm,
            ShowStoreLogo = original.ShowStoreLogo,
            StoreLogoUrl = original.StoreLogoUrl,
            MonochromeLogo = original.MonochromeLogo,
            ShowStoreName = original.ShowStoreName,
            StoreName = original.StoreName,
            ShowProductTitle = original.ShowProductTitle,
            ShowConditionGrade = original.ShowConditionGrade,
            ShowSerialNumber = original.ShowSerialNumber,
            ShowSku = original.ShowSku,
            ShowPrice = original.ShowPrice,
            ShowShutterCount = original.ShowShutterCount,
            ShowFooterText = original.ShowFooterText,
            FooterText = original.FooterText,
            CodeStyle = original.CodeStyle,
            EncodeValue = original.EncodeValue,
            BarcodeHeightMm = original.BarcodeHeightMm,
            BarcodeWidthRatio = original.BarcodeWidthRatio,
            QrSizeMm = original.QrSizeMm,
            ShowBarcodeText = original.ShowBarcodeText
        };

        _context.StickerTemplates.Add(copy);
        await _context.SaveChangesAsync();

        return MapToDto(copy);
    }

    public async Task SetDefaultAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var template = await _context.StickerTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);

        if (template == null)
        {
            throw AppException.NotFound($"ไม่พบรูปแบบสติ๊กเกอร์รหัส {id}");
        }

        await UnsetOtherDefaultsAsync(storeId, id);
        template.IsDefault = true;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var template = await _context.StickerTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);

        if (template == null)
        {
            throw AppException.NotFound($"ไม่พบรูปแบบสติ๊กเกอร์รหัส {id}");
        }

        var wasDefault = template.IsDefault;
        _context.StickerTemplates.Remove(template);
        await _context.SaveChangesAsync();

        // If deleted template was default, appoint another one as default
        if (wasDefault)
        {
            var next = await _context.StickerTemplates
                .Where(t => t.StoreId == storeId)
                .OrderBy(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                next.IsDefault = true;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task ResetToSystemDefaultsAsync()
    {
        var storeId = RequireStoreId();
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeId);
        var storeName = store?.Name ?? "MUE2MUEDEE";
        var storeLogo = store?.LogoUrl ?? "/logo.png";

        var existing = await _context.StickerTemplates
            .Where(t => t.StoreId == storeId)
            .ToListAsync();

        if (existing.Count > 0)
        {
            _context.StickerTemplates.RemoveRange(existing);
        }

        // Add 4 system templates
        var defaults = CreateDefaultTemplates(storeId, storeName, storeLogo);
        _context.StickerTemplates.AddRange(defaults);
        await _context.SaveChangesAsync();
    }

    public static List<StickerTemplate> CreateDefaultTemplates(Guid storeId, string storeName, string? storeLogoUrl)
    {
        return new List<StickerTemplate>
        {
            new StickerTemplate
            {
                StoreId = storeId,
                Name = "ม้วนเดี่ยวมาตรฐาน (1 คอลัมน์ - 50x30 mm)",
                Description = "เหมาะสำหรับเครื่องพิมพ์ความร้อนแบบม้วน (Xprinter, Gprinter, TSC) ขนาด 50x30 มม.",
                IsDefault = true,
                IsContinuousRoll = true,
                PaperWidthMm = 50.0m,
                PaperHeightMm = null,
                LabelWidthMm = 50.0m,
                LabelHeightMm = 30.0m,
                ColumnsCount = 1,
                RowsCount = null,
                HorizontalGapMm = 0.0m,
                VerticalGapMm = 2.0m,
                MarginTopMm = 0.0m,
                MarginBottomMm = 0.0m,
                MarginLeftMm = 0.0m,
                MarginRightMm = 0.0m,
                ShowStoreLogo = true,
                StoreLogoUrl = storeLogoUrl ?? "/logo.png",
                MonochromeLogo = true,
                ShowStoreName = true,
                StoreName = storeName,
                ShowProductTitle = true,
                ShowConditionGrade = true,
                ShowSerialNumber = true,
                ShowSku = true,
                ShowPrice = true,
                ShowShutterCount = false,
                ShowFooterText = true,
                FooterText = "รับประกัน 30 วัน",
                CodeStyle = "barcode_qr",
                EncodeValue = "serial",
                BarcodeHeightMm = 6.0m,
                BarcodeWidthRatio = 1.2m,
                QrSizeMm = 10.0m,
                ShowBarcodeText = false
            },
            new StickerTemplate
            {
                StoreId = storeId,
                Name = "ม้วนคู่ 2 คอลัมน์ (2 คอลัมน์ - 35x25 mm)",
                Description = "ดวงสติ๊กเกอร์คู่ 2 แถวขนานกันบนม้วน 75-80 มม. ดวงละ 35x25 มม. ระยะห่าง 4 มม.",
                IsDefault = false,
                IsContinuousRoll = true,
                PaperWidthMm = 74.0m,
                PaperHeightMm = null,
                LabelWidthMm = 35.0m,
                LabelHeightMm = 25.0m,
                ColumnsCount = 2,
                RowsCount = null,
                HorizontalGapMm = 4.0m,
                VerticalGapMm = 2.0m,
                MarginTopMm = 0.0m,
                MarginBottomMm = 0.0m,
                MarginLeftMm = 0.0m,
                MarginRightMm = 0.0m,
                ShowStoreLogo = true,
                StoreLogoUrl = storeLogoUrl ?? "/logo.png",
                MonochromeLogo = true,
                ShowStoreName = true,
                StoreName = storeName,
                ShowProductTitle = true,
                ShowConditionGrade = true,
                ShowSerialNumber = true,
                ShowSku = true,
                ShowPrice = true,
                ShowShutterCount = false,
                ShowFooterText = false,
                FooterText = "",
                CodeStyle = "barcode_only",
                EncodeValue = "serial",
                BarcodeHeightMm = 5.0m,
                BarcodeWidthRatio = 1.0m,
                QrSizeMm = 8.0m,
                ShowBarcodeText = false
            },
            new StickerTemplate
            {
                StoreId = storeId,
                Name = "ม้วน 3 คอลัมน์ (3 คอลัมน์ - 25x15 mm)",
                Description = "ดวงขนาดเล็ก 3 ดวงต่อแถว กว้าง 80 มม. สำหรับป้ายราคาสายคล้องหรืออุปกรณ์เสริมชิ้นเล็ก",
                IsDefault = false,
                IsContinuousRoll = true,
                PaperWidthMm = 80.0m,
                PaperHeightMm = null,
                LabelWidthMm = 25.0m,
                LabelHeightMm = 15.0m,
                ColumnsCount = 3,
                RowsCount = null,
                HorizontalGapMm = 2.0m,
                VerticalGapMm = 2.0m,
                MarginTopMm = 0.0m,
                MarginBottomMm = 0.0m,
                MarginLeftMm = 0.5m,
                MarginRightMm = 0.5m,
                ShowStoreLogo = false,
                StoreLogoUrl = null,
                MonochromeLogo = true,
                ShowStoreName = false,
                StoreName = storeName,
                ShowProductTitle = true,
                ShowConditionGrade = false,
                ShowSerialNumber = true,
                ShowSku = false,
                ShowPrice = true,
                ShowShutterCount = false,
                ShowFooterText = false,
                CodeStyle = "qr_only",
                EncodeValue = "serial",
                BarcodeHeightMm = 4.0m,
                BarcodeWidthRatio = 0.9m,
                QrSizeMm = 8.0m,
                ShowBarcodeText = false
            },
            new StickerTemplate
            {
                StoreId = storeId,
                Name = "กระดาษ A4 สติ๊กเกอร์ (3 คอลัมน์ x 8 แถว = 24 ดวง - 70x36 mm)",
                Description = "กระดาษสติกเกอร์สำเร็จรูปขนาด A4 (210x297 มม.) แถวละ 3 ดวง ทั้งหมด 24 ดวงต่อแผ่น สำหรับเครื่องพิมพ์ทั่วไป",
                IsDefault = false,
                IsContinuousRoll = false,
                PaperWidthMm = 210.0m,
                PaperHeightMm = 297.0m,
                LabelWidthMm = 70.0m,
                LabelHeightMm = 36.0m,
                ColumnsCount = 3,
                RowsCount = 8,
                HorizontalGapMm = 0.0m,
                VerticalGapMm = 0.0m,
                MarginTopMm = 4.5m,
                MarginBottomMm = 4.5m,
                MarginLeftMm = 0.0m,
                MarginRightMm = 0.0m,
                ShowStoreLogo = true,
                StoreLogoUrl = storeLogoUrl ?? "/logo.png",
                MonochromeLogo = true,
                ShowStoreName = true,
                StoreName = storeName,
                ShowProductTitle = true,
                ShowConditionGrade = true,
                ShowSerialNumber = true,
                ShowSku = true,
                ShowPrice = true,
                ShowShutterCount = false,
                ShowFooterText = true,
                FooterText = "MUE2MUEDEE รับประกัน 30 วัน",
                CodeStyle = "barcode_qr",
                EncodeValue = "serial",
                BarcodeHeightMm = 7.0m,
                BarcodeWidthRatio = 1.3m,
                QrSizeMm = 12.0m,
                ShowBarcodeText = false
            }
        };
    }

    private async Task UnsetOtherDefaultsAsync(Guid storeId, Guid? excludeId)
    {
        var currentDefaults = await _context.StickerTemplates
            .Where(t => t.StoreId == storeId && t.IsDefault && (!excludeId.HasValue || t.Id != excludeId.Value))
            .ToListAsync();

        foreach (var item in currentDefaults)
        {
            item.IsDefault = false;
        }
    }

    private static void ValidateDto(CreateStickerTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw AppException.BadRequest("กรุณาระบุชื่อรูปแบบสติ๊กเกอร์ (Template Name)");
        }

        if (dto.ColumnsCount < 1 || dto.ColumnsCount > 10)
        {
            throw AppException.BadRequest("จำนวนคอลัมน์ต้องอยู่ระหว่าง 1 ถึง 10 คอลัมน์");
        }

        if (dto.LabelWidthMm <= 0 || dto.LabelHeightMm <= 0)
        {
            throw AppException.BadRequest("ขนาดความกว้างและความยาวของดวงสติ๊กเกอร์ต้องมากกว่า 0 มม.");
        }

        if (dto.PaperWidthMm <= 0)
        {
            throw AppException.BadRequest("ความกว้างของหน้ากระดาษหรือม้วนสติ๊กเกอร์ต้องมากกว่า 0 มม.");
        }

        if (!dto.IsContinuousRoll && (!dto.PaperHeightMm.HasValue || dto.PaperHeightMm.Value <= 0))
        {
            throw AppException.BadRequest("สำหรับกระดาษแบบแผ่น (Sheet) กรุณาระบุความสูงของหน้ากระดาษ");
        }

        decimal totalContentWidth = (dto.ColumnsCount * dto.LabelWidthMm)
            + ((dto.ColumnsCount - 1) * dto.HorizontalGapMm)
            + dto.MarginLeftMm
            + dto.MarginRightMm;

        if (totalContentWidth > dto.PaperWidthMm)
        {
            throw AppException.BadRequest($"ความกว้างรวมของสติ๊กเกอร์และระยะขอบ ({totalContentWidth:N1} mm) เกินขนาดหน้ากว้างกระดาษ ({dto.PaperWidthMm:N1} mm)");
        }
    }

    private static StickerTemplateDto MapToDto(StickerTemplate t)
    {
        return new StickerTemplateDto
        {
            Id = t.Id,
            StoreId = t.StoreId,
            Name = t.Name,
            Description = t.Description,
            IsDefault = t.IsDefault,
            IsContinuousRoll = t.IsContinuousRoll,
            PaperWidthMm = t.PaperWidthMm,
            PaperHeightMm = t.PaperHeightMm,
            LabelWidthMm = t.LabelWidthMm,
            LabelHeightMm = t.LabelHeightMm,
            ColumnsCount = t.ColumnsCount,
            RowsCount = t.RowsCount,
            HorizontalGapMm = t.HorizontalGapMm,
            VerticalGapMm = t.VerticalGapMm,
            MarginTopMm = t.MarginTopMm,
            MarginBottomMm = t.MarginBottomMm,
            MarginLeftMm = t.MarginLeftMm,
            MarginRightMm = t.MarginRightMm,
            ShowStoreLogo = t.ShowStoreLogo,
            StoreLogoUrl = t.StoreLogoUrl,
            MonochromeLogo = t.MonochromeLogo,
            ShowStoreName = t.ShowStoreName,
            StoreName = t.StoreName,
            ShowProductTitle = t.ShowProductTitle,
            ShowConditionGrade = t.ShowConditionGrade,
            ShowSerialNumber = t.ShowSerialNumber,
            ShowSku = t.ShowSku,
            ShowPrice = t.ShowPrice,
            ShowShutterCount = t.ShowShutterCount,
            ShowFooterText = t.ShowFooterText,
            FooterText = t.FooterText,
            CodeStyle = t.CodeStyle,
            EncodeValue = t.EncodeValue,
            BarcodeHeightMm = t.BarcodeHeightMm,
            BarcodeWidthRatio = t.BarcodeWidthRatio,
            QrSizeMm = t.QrSizeMm,
            ShowBarcodeText = t.ShowBarcodeText,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}
