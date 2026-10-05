# MUE2MUEDEE (มือสองมือดี) — Coding Standards & System Rules

เอกสารข้อกำหนดและกฎเกณฑ์มาตรฐานการพัฒนา (System Conventions & Coding Standards) สำหรับโปรเจกต์ MUE2MUEDEE ทั้ง Backend และ Frontend เพื่อให้ทุกการทำงานและการพัฒนาฟีเจอร์เป็นไปตามมาตรฐานเดียวกันอย่างเคร่งครัด

---

## 1. General Architecture & Mindset

1. **Clean Architecture (Backend)**:
   - **Domain**: Pure business entities, enums, interfaces, base entity contracts (`BaseEntity`, `AuditableEntity`, `ITenantEntity`, `ISoftDelete`). ห้ามมี dependency ภายนอก
   - **Application**: DTOs, Business Services/Handlers, Interfaces, Exceptions, Response Models
   - **Infrastructure**: EF Core, Database Configurations, External Services, Persistence, Migrations
   - **Web (API)**: Thin Controllers, Middlewares, Dependency Injection registration
2. **Angular Standalone + Reactive Forms (Frontend)**:
   - Angular 22+, Standalone Components 100%
   - Signal-based state management ใน Services
   - Strict Reactive Forms (`FormGroup`, `FormControl`, `ControlValueAccessor`)
   - **ห้ามเขียน inline template หรือ inline styles**: ทุก Component ต้องแยกไฟล์ 3 ไฟล์เสมอ (`.ts`, `.html`, `.css`)
   - Feature-level modularization: รวม Services และ Models ไว้ระดับ Feature (เช่น `inventory.service.ts`, `inventory.models.ts`) ไม่แยก service ย่อยต่อ component จนกระจัดกระจาย

---

## 2. Frontend Conventions & Shared Components

### 2.1 File Separation Rule
- ทุก Component ต้องระบุ:
  ```typescript
  @Component({
    selector: 'app-...',
    standalone: true,
    imports: [...],
    templateUrl: './xyz.component.html',
    styleUrl: './xyz.component.css'
  })
  ```
- **ห้าม** ใช้ `template: \`...\`` หรือ `styles: [\`...\`]` เด็ดขาด

### 2.2 Shared Form Components Naming & Usage
คอมโพเนนต์กลางอยู่ใน `src/app/shared/components/` โดยมีชื่อ selector และเงื่อนไขการทำงานดังนี้:

| Component Tag | ชื่อโฟลเดอร์ | รายละเอียด & การใช้งาน |
| :--- | :--- | :--- |
| `<app-textbox>` | `textbox/` | สำหรับข้อความทั่วไป (Text, Email, Password, Phone) รองรับ label, icon, errorMessage |
| `<app-number>` | `number/` | **Dual-Mode Number Input**:<br>1. **โหมดตัวเลขทั่วไป** (`<app-number>`): รับเฉพาะตัวเลข/จำนวนเต็ม เช่น Shutter count, จำนวนชิ้น<br>2. **โหมดการเงิน** (`<app-number currency>`): จัดรูปแบบเงินบาทไทย (`฿`), มีทศนิยม 2 ตำแหน่ง, ใส่ comma คั่นหลักพันอัตโนมัติ |
| `<app-textarea>` | `textarea/` | สำหรับข้อความยาว เช่น หมายเหตุ, อาการตำหนิ, รายละเอียดสินค้า |
| `<app-select>` | `select/` | Dropdown คัดเลือกตัวเลือก รองรับ options list, placeholder, validation |
| `<app-checkbox>` | `checkbox/` | Toggle checkbox สำหรับ boolean flag |
| `<app-status-badge>` | `status-badge/` | แสดงสถานะต่างๆ (InventoryStatus, StoreRole, ฯลฯ) พร้อมสีเฉพาะสถานะ |

### 2.3 Reactive Forms Integration
- ทุก Input Component ต้อง implement `ControlValueAccessor` อย่างสมบูรณ์ เพื่อให้ใช้กับ `formControlName` และ `[formControl]` ได้อย่างไร้รอยต่อ

---

## 3. Backend Conventions

### 3.1 Standard API Response Envelope (`ApiResponse<T>`)
ทุก Endpoint และ Controller ต้องส่งข้อมูลกลับในรูปแบบเดียวผ่าน `ApiResponse<T>`:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "ข้อความอธิบายผลลัพธ์",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-10-05T03:00:00Z"
}
```
- Controller ทุกตัวต้องสืบทอดจาก `ApiControllerBase`
- การ return สำเร็จ: `return Success(data, "ข้อความ");` หรือ `return CreatedSuccess(data, "สร้างสำเร็จ");`

### 3.2 Dynamic Error Messaging & Exception Handling
- **ห้ามฮาร์ดโค้ดข้อความ error รวมไว้ที่ตัวกลาง**:
- เมื่อเกิดข้อผิดพลาดใน business logic ให้ `throw new AppException("ข้อความเฉพาะจุดนั้นๆ", statusCode)`
- มี Helper methods:
  - `AppException.BadRequest("ข้อความ")` -> 400
  - `AppException.NotFound("ข้อความ")` -> 404
  - `AppException.Conflict("ข้อความ")` -> 409
  - `AppException.Forbidden("ข้อความ")` -> 403
  - `AppException.Unauthorized("ข้อความ")` -> 401
- `ExceptionHandlingMiddleware` จะดึงข้อความจาก Exception มาใส่ใน `ApiResponse<object>.Fail(...)` ส่งกลับไปหา Client อัตโนมัติ

---

## 4. Database Architecture & Naming Rules

### 4.1 Strict Flow: `Entity -> Configuration -> DbContext`
ห้ามเขียน Fluent API config อัดไว้ใน `ApplicationDbContext.OnModelCreating()` เด็ดขาด ให้ใช้ขั้นตอนดังนี้เสมอ:
1. **Entity**: สร้าง Entity ใน `Domain/Entities/`
   - สืบทอด `AuditableEntity<TId>` หรือ `BaseEntity<TId>`
   - หากเป็นข้อมูลระดับร้านค้า ต้อง implement `ITenantEntity` (`StoreId`)
   - หากต้องการ Soft Delete ต้อง implement `ISoftDelete` (`IsDeleted`, `DeletedAt`)
2. **Configuration**: สร้างคลาสคอนฟิกใน `Infrastructure/Persistence/Configurations/`
   - Implement `IEntityTypeConfiguration<TEntity>`
   - กำหนด Primary Key, Foreign Keys, Indexes, Column Types, MaxLength, Default Values
3. **DbContext**: ลงทะเบียน `DbSet<TEntity>` ใน `ApplicationDbContext.cs`
   - `OnModelCreating` จะเรียก `modelBuilder.ApplyConfigurationsFromAssembly(...)` อัตโนมัติ

### 4.2 Database Naming Convention: C# PascalCase ➔ DB snake_case
- **C# Code**: ใช้ **PascalCase** เสมอ (เช่น `UserProduct`, `InventoryItem`, `SerialNumber`, `ShutterCount`)
- **Database (PostgreSQL)**: ใช้ **snake_case** เสมอ (เช่น `user_product`, `inventory_item`, `serial_number`, `shutter_count`)
- ควบคุมผ่าน EF Core Naming Conventions ใน `DependencyInjection.cs`:
  ```csharp
  options.UseNpgsql(...).UseSnakeCaseNamingConvention();
  ```

---

## 5. Multi-Tenancy & Security Rules

1. **Multi-Tenant Isolation (`StoreId`)**:
   - `StoreId` จะถูกผูกไว้ใน JWT Token และดึงผ่าน `ICurrentStore`
   - **ห้ามเชื่อถือ `StoreId` ที่ส่งมาจาก Client Body** ในการทำงานทั่วไป (ระบบจะใช้ StoreId จาก Token ของ CurrentStore เพื่อความปลอดภัย)
   - `ApplicationDbContext` มี Global Query Filter กรองเฉพาะข้อมูลของ `StoreId` อัตโนมัติสำหรับ Entity ที่ implement `ITenantEntity`
   - ใน `SaveChangesAsync` ระบบจะ assign `StoreId` และ Audit fields (`CreatedAt`, `CreatedBy`) ให้อัตโนมัติ

2. **Role & Permission**:
   - สิทธิ์ (`Owner`, `Admin`, `Staff`) ผูกกับ `UserStore` (1 user อาจมีบทบาทต่างกันในแต่ละสาขา/ร้านค้า)

---

## 6. Tooling & Platform Rules

1. **Windows PowerShell Execution Policy**:
   - การรันคำสั่งเกี่ยวกับ Node / Angular CLI ใน Environment นี้ **ต้องใช้ `npm.cmd` หรือ `npx.cmd` เสมอ** เพื่อป้องกัน Restricted Execution Policy บน `.ps1`
