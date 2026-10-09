namespace Mue2MueDee.Application.DTOs;

public class StickerTemplateDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }

    public bool IsContinuousRoll { get; set; }
    public decimal PaperWidthMm { get; set; }
    public decimal? PaperHeightMm { get; set; }

    public decimal LabelWidthMm { get; set; }
    public decimal LabelHeightMm { get; set; }

    public int ColumnsCount { get; set; }
    public int? RowsCount { get; set; }
    public decimal HorizontalGapMm { get; set; }
    public decimal VerticalGapMm { get; set; }

    public decimal MarginTopMm { get; set; }
    public decimal MarginBottomMm { get; set; }
    public decimal MarginLeftMm { get; set; }
    public decimal MarginRightMm { get; set; }

    public bool ShowStoreLogo { get; set; }
    public string? StoreLogoUrl { get; set; }
    public bool MonochromeLogo { get; set; }

    public bool ShowStoreName { get; set; }
    public string? StoreName { get; set; }

    public bool ShowProductTitle { get; set; }
    public bool ShowConditionGrade { get; set; }
    public bool ShowSerialNumber { get; set; }
    public bool ShowSku { get; set; }
    public bool ShowPrice { get; set; }
    public bool ShowShutterCount { get; set; }
    public bool ShowFooterText { get; set; }
    public string? FooterText { get; set; }

    public string CodeStyle { get; set; } = "barcode_qr";
    public string EncodeValue { get; set; } = "serial";

    public decimal BarcodeHeightMm { get; set; }
    public decimal BarcodeWidthRatio { get; set; }
    public decimal QrSizeMm { get; set; }
    public bool ShowBarcodeText { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateStickerTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }

    public bool IsContinuousRoll { get; set; } = true;
    public decimal PaperWidthMm { get; set; } = 50.0m;
    public decimal? PaperHeightMm { get; set; }

    public decimal LabelWidthMm { get; set; } = 50.0m;
    public decimal LabelHeightMm { get; set; } = 30.0m;

    public int ColumnsCount { get; set; } = 1;
    public int? RowsCount { get; set; }
    public decimal HorizontalGapMm { get; set; } = 0.0m;
    public decimal VerticalGapMm { get; set; } = 2.0m;

    public decimal MarginTopMm { get; set; } = 0.0m;
    public decimal MarginBottomMm { get; set; } = 0.0m;
    public decimal MarginLeftMm { get; set; } = 0.0m;
    public decimal MarginRightMm { get; set; } = 0.0m;

    public bool ShowStoreLogo { get; set; } = true;
    public string? StoreLogoUrl { get; set; }
    public bool MonochromeLogo { get; set; } = true;

    public bool ShowStoreName { get; set; } = true;
    public string? StoreName { get; set; }

    public bool ShowProductTitle { get; set; } = true;
    public bool ShowConditionGrade { get; set; } = true;
    public bool ShowSerialNumber { get; set; } = true;
    public bool ShowSku { get; set; } = true;
    public bool ShowPrice { get; set; } = true;
    public bool ShowShutterCount { get; set; } = false;
    public bool ShowFooterText { get; set; } = true;
    public string? FooterText { get; set; } = "รับประกัน 30 วัน";

    public string CodeStyle { get; set; } = "barcode_qr";
    public string EncodeValue { get; set; } = "serial";

    public decimal BarcodeHeightMm { get; set; } = 6.0m;
    public decimal BarcodeWidthRatio { get; set; } = 1.2m;
    public decimal QrSizeMm { get; set; } = 10.0m;
    public bool ShowBarcodeText { get; set; } = false;
}

public class UpdateStickerTemplateDto : CreateStickerTemplateDto
{
}
