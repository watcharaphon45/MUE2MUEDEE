# MUE2MUEDEE — สรุปข้อกำหนดและมาตรฐานการพัฒนา (System Conventions)

เอกสารนี้รวบรวมข้อตกลง สถาปัตยกรรม และมาตรฐานการเขียนโค้ดสำหรับโปรเจกต์ MUE2MUEDEE

---

### 1. โครงสร้างไฟล์ Frontend (Angular 22+)
- **การแยกไฟล์**: บังคับแยก 3 ไฟล์เสมอ (`.ts`, `.html`, `.css`) ห้ามใช้ inline template หรือ inline styles
- **รูปแบบฟอร์ม**: ใช้ Angular Reactive Forms (`FormGroup`, `FormControl`, `ControlValueAccessor`)
- **การจัดการ Service**: จัดระดับ Feature (`features/xxx/xxx.service.ts`, `features/xxx/xxx.models.ts`) ไม่สร้าง service ซอยย่อยแยกราย component

---

### 2. Component ส่วนกลาง (Shared Components)
ตั้งอยู่ที่ `src/app/shared/components/`:
1. `<app-textbox>` : สำหรับ input ข้อความทั่วไป (Text, Email, Password, Phone)
2. `<app-number>` : ตัวเลข 2 โหมด
   - โหมดตัวเลขทั่วไป (`<app-number>`) : รับเฉพาะตัวเลขจำนวนเต็ม (เช่น Shutter count, จำนวน)
   - โหมดการเงิน (`<app-number currency>`) : จัดรูปแบบเงินบาทไทย (`฿`), มีจุดทศนิยม 2 ตำแหน่ง, มี comma คั่นหลักพัน
3. `<app-textarea>` : สำหรับข้อความหลายบรรทัด
4. `<app-select>` : สำหรับ dropdown คัดเลือกตัวเลือก
5. `<app-checkbox>` : สำหรับ boolean checkbox
6. `<app-status-badge>` : ป้ายแสดงสถานะต่าง ๆ

---

### 3. Backend & API Conventions (ASP.NET Core 10)
- **Response Format**: ทุก endpoint ส่งกลับผ่าน `ApiResponse<T>`:
  - `{ success, statusCode, message, data, errors, timestamp }`
- **Error Handling**: 
  - ไม่ฮาร์ดโค้ด error รวมไว้ที่ตัวกลาง
  - โยน `new AppException("ข้อความ error เฉพาะจุด", statusCode)` หรือใช้ helper เช่น `AppException.BadRequest("...")`
  - `ExceptionHandlingMiddleware` จะแปลงเป็น `ApiResponse.Fail` ส่งกลับ client อัตโนมัติ
- **Controller**: สืบทอดจาก `ApiControllerBase`

---

### 4. สถาปัตยกรรมฐานข้อมูล (Database Architecture)
- **ลำดับการสร้าง**: **`Entity -> Configuration -> DbContext`**
  1. `Entity` ใน `Domain/Entities/` (สืบทอด `AuditableEntity`, `ITenantEntity`, `ISoftDelete`)
  2. `Configuration` ใน `Infrastructure/Persistence/Configurations/` (implement `IEntityTypeConfiguration<T>`)
  3. `DbContext` ลงทะเบียน `DbSet<T>` (โหลด config ผ่าน `ApplyConfigurationsFromAssembly`)
- **การตั้งชื่อ (Naming Convention)**:
  - **C# Code**: ใช้ `PascalCase` (เช่น `UserProduct`, `InventoryItem`, `SerialNumber`)
  - **Database (PostgreSQL)**: ใช้ `snake_case` (เช่น `user_product`, `inventory_item`, `serial_number`) จัดการอัตโนมัติด้วย EF Core Naming Conventions

---

### 5. Multi-Tenant & Security
- กรองข้อมูลตาม `StoreId` เสมอผ่าน JWT Claim และ `ICurrentStore`
- ไม่ใช้ `StoreId` ที่ส่งมาจาก Client Body เพื่อความปลอดภัย
- Global Query Filter และ `SaveChangesAsync` จัดการกรองและใส่ `StoreId` อัตโนมัติ

---

### 6. คำสั่ง Shell (Windows PowerShell)
- ให้ใช้ `npm.cmd` หรือ `npx.cmd` สำหรับคำสั่ง Node / Angular เสมอ
