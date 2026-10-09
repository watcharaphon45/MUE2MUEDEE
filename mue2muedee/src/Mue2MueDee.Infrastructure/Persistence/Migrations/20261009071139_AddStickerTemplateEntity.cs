using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mue2MueDee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStickerTemplateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sticker_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_continuous_roll = table.Column<bool>(type: "boolean", nullable: false),
                    paper_width_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    paper_height_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    label_width_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    label_height_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    columns_count = table.Column<int>(type: "integer", nullable: false),
                    rows_count = table.Column<int>(type: "integer", nullable: true),
                    horizontal_gap_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    vertical_gap_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    margin_top_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    margin_bottom_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    margin_left_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    margin_right_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    show_store_logo = table.Column<bool>(type: "boolean", nullable: false),
                    store_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    monochrome_logo = table.Column<bool>(type: "boolean", nullable: false),
                    show_store_name = table.Column<bool>(type: "boolean", nullable: false),
                    store_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    show_product_title = table.Column<bool>(type: "boolean", nullable: false),
                    show_condition_grade = table.Column<bool>(type: "boolean", nullable: false),
                    show_serial_number = table.Column<bool>(type: "boolean", nullable: false),
                    show_sku = table.Column<bool>(type: "boolean", nullable: false),
                    show_price = table.Column<bool>(type: "boolean", nullable: false),
                    show_shutter_count = table.Column<bool>(type: "boolean", nullable: false),
                    show_footer_text = table.Column<bool>(type: "boolean", nullable: false),
                    footer_text = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    code_style = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    encode_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    barcode_height_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    barcode_width_ratio = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    qr_size_mm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    show_barcode_text = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sticker_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_sticker_templates_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sticker_templates_store_id_is_default",
                table: "sticker_templates",
                columns: new[] { "store_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_sticker_templates_store_id_name",
                table: "sticker_templates",
                columns: new[] { "store_id", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sticker_templates");
        }
    }
}
