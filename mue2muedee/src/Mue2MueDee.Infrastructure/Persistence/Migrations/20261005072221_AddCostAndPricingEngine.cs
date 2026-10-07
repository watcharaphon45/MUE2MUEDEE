using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mue2MueDee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostAndPricingEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "default_target_margin",
                table: "stores",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "target_margin",
                table: "categories",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inventory_costs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cost_type = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    incurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_costs", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_costs_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventory_costs_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    new_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_histories_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_price_histories_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_costs_inventory_item_id",
                table: "inventory_costs",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_costs_store_id_cost_type",
                table: "inventory_costs",
                columns: new[] { "store_id", "cost_type" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_costs_store_id_inventory_item_id",
                table: "inventory_costs",
                columns: new[] { "store_id", "inventory_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_price_histories_changed_at",
                table: "price_histories",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "ix_price_histories_inventory_item_id",
                table: "price_histories",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_histories_store_id_inventory_item_id",
                table: "price_histories",
                columns: new[] { "store_id", "inventory_item_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_costs");

            migrationBuilder.DropTable(
                name: "price_histories");

            migrationBuilder.DropColumn(
                name: "default_target_margin",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "target_margin",
                table: "categories");
        }
    }
}
