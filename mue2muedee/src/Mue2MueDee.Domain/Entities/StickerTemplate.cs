using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

/// <summary>
/// รูปแบบการพิมพ์สติ๊กเกอร์บาร์โค้ด/QR Code รองรับ 1, 2, 3 คอลัมน์ ทั้งแบบม้วน (Thermal Roll) และกระดาษแผ่น (A4 Sheet)
/// </summary>
public class StickerTemplate : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }

    // กระดาษ / ม้วนพิมพ์
    public bool IsContinuousRoll { get; set; } = true;
    public decimal PaperWidthMm { get; set; } = 50.0m;
    public decimal? PaperHeightMm { get; set; }

    // ขนาดดวงสติ๊กเกอร์
    public decimal LabelWidthMm { get; set; } = 50.0m;
    public decimal LabelHeightMm { get; set; } = 30.0m;

    // การจัดวางคอลัมน์และแถว
    public int ColumnsCount { get; set; } = 1;
    public int? RowsCount { get; set; }
    public decimal HorizontalGapMm { get; set; } = 0.0m;
    public decimal VerticalGapMm { get; set; } = 2.0m;

    // ระยะขอบ (Margins)
    public decimal MarginTopMm { get; set; } = 0.0m;
    public decimal MarginBottomMm { get; set; } = 0.0m;
    public decimal MarginLeftMm { get; set; } = 0.0m;
    public decimal MarginRightMm { get; set; } = 0.0m;

    // การเปิด/ปิดเนื้อหาบนป้าย
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

    // รูปแบบโค้ด (barcode_qr, barcode_only, qr_only)
    public string CodeStyle { get; set; } = "barcode_qr";
    // ค่าที่จะ encode (serial, sku)
    public string EncodeValue { get; set; } = "serial";

    // ขนาดบาร์โค้ดและ QR
    public decimal BarcodeHeightMm { get; set; } = 6.0m;
    public decimal BarcodeWidthRatio { get; set; } = 1.2m;
    public decimal QrSizeMm { get; set; } = 10.0m;
    public bool ShowBarcodeText { get; set; } = false;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public StickerTemplate()
    {
        Id = Guid.NewGuid();
    }
}
