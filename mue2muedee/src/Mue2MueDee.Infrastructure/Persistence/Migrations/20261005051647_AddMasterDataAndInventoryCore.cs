using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mue2MueDee.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterDataAndInventoryCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_users_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_user_stores_stores_StoreId",
                table: "user_stores");

            migrationBuilder.DropForeignKey(
                name: "FK_user_stores_users_UserId",
                table: "user_stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_stores",
                table: "user_stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stores",
                table: "stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "users",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "users",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "users",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "users",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "users",
                newName: "deleted_by");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "users",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "users",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "users",
                newName: "avatar_url");

            migrationBuilder.RenameIndex(
                name: "IX_users_Username",
                table: "users",
                newName: "ix_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "user_stores",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_stores",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_stores",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "user_stores",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "user_stores",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "user_stores",
                newName: "store_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "user_stores",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "user_stores",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "user_stores",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_user_stores_UserId_StoreId",
                table: "user_stores",
                newName: "ix_user_stores_user_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_stores_StoreId",
                table: "user_stores",
                newName: "ix_user_stores_store_id");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "stores",
                newName: "website");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "stores",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "stores",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Line",
                table: "stores",
                newName: "line");

            migrationBuilder.RenameColumn(
                name: "Facebook",
                table: "stores",
                newName: "facebook");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "stores",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "stores",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "stores",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "stores",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "stores",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "stores",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "stores",
                newName: "tax_id");

            migrationBuilder.RenameColumn(
                name: "PrimaryColor",
                table: "stores",
                newName: "primary_color");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "stores",
                newName: "logo_url");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "stores",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "stores",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "stores",
                newName: "deleted_by");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "stores",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "stores",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "stores",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_stores_Code",
                table: "stores",
                newName: "ix_stores_code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameColumn(
                name: "RevokedByIp",
                table: "refresh_tokens",
                newName: "revoked_by_ip");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "refresh_tokens",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "ReplacedByTokenHash",
                table: "refresh_tokens",
                newName: "replaced_by_token_hash");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedByIp",
                table: "refresh_tokens",
                newName: "created_by_ip");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "refresh_tokens",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "audit_logs",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "audit_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "audit_logs",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "audit_logs",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "audit_logs",
                newName: "store_id");

            migrationBuilder.RenameColumn(
                name: "OldValues",
                table: "audit_logs",
                newName: "old_values");

            migrationBuilder.RenameColumn(
                name: "NewValues",
                table: "audit_logs",
                newName: "new_values");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "audit_logs",
                newName: "ip_address");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "audit_logs",
                newName: "entity_name");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "audit_logs",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "audit_logs",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_StoreId_CreatedAt",
                table: "audit_logs",
                newName: "ix_audit_logs_store_id_created_at");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "website",
                table: "stores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "stores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "line",
                table: "stores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "facebook",
                table: "stores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "stores",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tax_id",
                table: "stores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "primary_color",
                table: "stores",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#4f46e5",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "user_name",
                table: "audit_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ip_address",
                table: "audit_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_stores",
                table: "user_stores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stores",
                table: "stores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_logs",
                table: "audit_logs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("pk_brands", x => x.id);
                    table.ForeignKey(
                        name: "fk_brands_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "condition_grades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    grade_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_condition_grades", x => x.id);
                    table.ForeignKey(
                        name: "fk_condition_grades_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    id_card_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    line_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    facebook = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_account_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    product_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    default_selling_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    condition_grade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    purchase_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    actual_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    selling_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shutter_count = table.Column<int>(type: "integer", nullable: true),
                    accessories = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    defects = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sold_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
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
                    table.PrimaryKey("pk_inventory_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_items_condition_grades_condition_grade_id",
                        column: x => x.condition_grade_id,
                        principalTable: "condition_grades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_items_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_inventory_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inventory_items_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_movements_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_movements_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_brands_store_id_name",
                table: "brands",
                columns: new[] { "store_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_store_id_name",
                table: "categories",
                columns: new[] { "store_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_condition_grades_store_id_grade_code",
                table: "condition_grades",
                columns: new[] { "store_id", "grade_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_store_id_id_card_number",
                table: "customers",
                columns: new[] { "store_id", "id_card_number" });

            migrationBuilder.CreateIndex(
                name: "ix_customers_store_id_phone",
                table: "customers",
                columns: new[] { "store_id", "phone" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_condition_grade_id",
                table: "inventory_items",
                column: "condition_grade_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_customer_id",
                table: "inventory_items",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_product_id",
                table: "inventory_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_product_id_serial_number",
                table: "inventory_items",
                columns: new[] { "store_id", "product_id", "serial_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_serial_number",
                table: "inventory_items",
                columns: new[] { "store_id", "serial_number" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_sku",
                table: "inventory_items",
                columns: new[] { "store_id", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_items_store_id_status",
                table: "inventory_items",
                columns: new[] { "store_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_products_brand_id",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_store_id_product_code",
                table: "products",
                columns: new[] { "store_id", "product_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_inventory_item_id",
                table: "stock_movements",
                column: "inventory_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_store_id_created_at",
                table: "stock_movements",
                columns: new[] { "store_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_store_id_inventory_item_id",
                table: "stock_movements",
                columns: new[] { "store_id", "inventory_item_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_stores_stores_store_id",
                table: "user_stores",
                column: "store_id",
                principalTable: "stores",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_stores_users_user_id",
                table: "user_stores",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_user_stores_stores_store_id",
                table: "user_stores");

            migrationBuilder.DropForeignKey(
                name: "fk_user_stores_users_user_id",
                table: "user_stores");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "inventory_items");

            migrationBuilder.DropTable(
                name: "condition_grades");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_stores",
                table: "user_stores");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stores",
                table: "stores");

            migrationBuilder.DropPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_logs",
                table: "audit_logs");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "users",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "users",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "users",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "deleted_by",
                table: "users",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "users",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "users",
                newName: "AvatarUrl");

            migrationBuilder.RenameIndex(
                name: "ix_users_username",
                table: "users",
                newName: "IX_users_Username");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "user_stores",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "user_stores",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_stores",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "user_stores",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "user_stores",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "store_id",
                table: "user_stores",
                newName: "StoreId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "user_stores",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "user_stores",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "user_stores",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_user_stores_user_id_store_id",
                table: "user_stores",
                newName: "IX_user_stores_UserId_StoreId");

            migrationBuilder.RenameIndex(
                name: "ix_user_stores_store_id",
                table: "user_stores",
                newName: "IX_user_stores_StoreId");

            migrationBuilder.RenameColumn(
                name: "website",
                table: "stores",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "stores",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "stores",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "line",
                table: "stores",
                newName: "Line");

            migrationBuilder.RenameColumn(
                name: "facebook",
                table: "stores",
                newName: "Facebook");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "stores",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "stores",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "stores",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "stores",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "stores",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "stores",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tax_id",
                table: "stores",
                newName: "TaxId");

            migrationBuilder.RenameColumn(
                name: "primary_color",
                table: "stores",
                newName: "PrimaryColor");

            migrationBuilder.RenameColumn(
                name: "logo_url",
                table: "stores",
                newName: "LogoUrl");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "stores",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "stores",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "deleted_by",
                table: "stores",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "stores",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "stores",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "stores",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_stores_code",
                table: "stores",
                newName: "IX_stores_Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "refresh_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "refresh_tokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "refresh_tokens",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "revoked_by_ip",
                table: "refresh_tokens",
                newName: "RevokedByIp");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "refresh_tokens",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "replaced_by_token_hash",
                table: "refresh_tokens",
                newName: "ReplacedByTokenHash");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "refresh_tokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_by_ip",
                table: "refresh_tokens",
                newName: "CreatedByIp");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "refresh_tokens",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_TokenHash");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "audit_logs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "audit_logs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "audit_logs",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "audit_logs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "store_id",
                table: "audit_logs",
                newName: "StoreId");

            migrationBuilder.RenameColumn(
                name: "old_values",
                table: "audit_logs",
                newName: "OldValues");

            migrationBuilder.RenameColumn(
                name: "new_values",
                table: "audit_logs",
                newName: "NewValues");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "audit_logs",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "entity_name",
                table: "audit_logs",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "audit_logs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "audit_logs",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_audit_logs_store_id_created_at",
                table: "audit_logs",
                newName: "IX_audit_logs_StoreId_CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Line",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Facebook",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                table: "stores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryColor",
                table: "stores",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "#4f46e5");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "audit_logs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "audit_logs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_stores",
                table: "user_stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stores",
                table: "stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_users_UserId",
                table: "refresh_tokens",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_stores_stores_StoreId",
                table: "user_stores",
                column: "StoreId",
                principalTable: "stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_stores_users_UserId",
                table: "user_stores",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
