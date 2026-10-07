using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mue2MueDee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseAndInspectionCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inspection_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_inspection_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_inspection_templates_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    po_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_estimated_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_max_buy_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_offered_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_agreed_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    needs_approval = table.Column<bool>(type: "boolean", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    purchase_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_purchase_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inspection_template_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_critical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inspection_template_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_inspection_template_items_inspection_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "inspection_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    condition_grade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shutter_count = table.Column<int>(type: "integer", nullable: true),
                    accessories = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    defects = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    expected_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estimated_repair_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estimated_cleaning_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    missing_accessories_deduction = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    max_buy_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    offered_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    agreed_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_items_condition_grades_condition_grade_id",
                        column: x => x.condition_grade_id,
                        principalTable: "condition_grades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_items_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_purchase_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_items_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_purchase_items_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    proof_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_payments_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_purchase_payments_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inspection_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inspection_template_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inspection_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_inspection_results_inspection_template_items_inspection_tem",
                        column: x => x.inspection_template_item_id,
                        principalTable: "inspection_template_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inspection_results_purchase_items_purchase_item_id",
                        column: x => x.purchase_item_id,
                        principalTable: "purchase_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inspection_results_inspection_template_item_id",
                table: "inspection_results",
                column: "inspection_template_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspection_results_purchase_item_id_inspection_template_ite",
                table: "inspection_results",
                columns: new[] { "purchase_item_id", "inspection_template_item_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inspection_template_items_template_id_sort_order",
                table: "inspection_template_items",
                columns: new[] { "template_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_inspection_templates_store_id_category_type",
                table: "inspection_templates",
                columns: new[] { "store_id", "category_type" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_condition_grade_id",
                table: "purchase_items",
                column: "condition_grade_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_inventory_item_id",
                table: "purchase_items",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_product_id",
                table: "purchase_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_purchase_order_id",
                table: "purchase_items",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_store_id_product_id_serial_number",
                table: "purchase_items",
                columns: new[] { "store_id", "product_id", "serial_number" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_store_id_purchase_order_id",
                table: "purchase_items",
                columns: new[] { "store_id", "purchase_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_customer_id",
                table: "purchase_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_store_id_customer_id",
                table: "purchase_orders",
                columns: new[] { "store_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_store_id_po_number",
                table: "purchase_orders",
                columns: new[] { "store_id", "po_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_store_id_status",
                table: "purchase_orders",
                columns: new[] { "store_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_payments_purchase_order_id",
                table: "purchase_payments",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_payments_store_id_purchase_order_id",
                table: "purchase_payments",
                columns: new[] { "store_id", "purchase_order_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inspection_results");

            migrationBuilder.DropTable(
                name: "purchase_payments");

            migrationBuilder.DropTable(
                name: "inspection_template_items");

            migrationBuilder.DropTable(
                name: "purchase_items");

            migrationBuilder.DropTable(
                name: "inspection_templates");

            migrationBuilder.DropTable(
                name: "purchase_orders");
        }
    }
}
