# MUE2MUEDEE

## มือสองมือดี — Second-hand Business Management System

> ระบบบริหารร้านสินค้ามือสอง ตั้งแต่รับซื้อ ประเมิน ตรวจสภาพ จัดเก็บสินค้า ตั้งราคา ขาย และวิเคราะห์กำไร

---

# 1. Project Overview

## 1.1 Concept

MUE2MUEDEE เป็นระบบบริหารร้านสินค้ามือสอง โดยเริ่มต้นออกแบบสำหรับ **ร้านกล้องและเลนส์มือสอง**

จุดสำคัญของระบบคือการบริหารกระบวนการ:

```text
ลูกค้านำสินค้ามาขาย
        ↓
รับซื้อ
        ↓
ประเมินราคา
        ↓
ตรวจสภาพ
        ↓
รับเข้าสินค้า
        ↓
คำนวณต้นทุนจริง
        ↓
ตั้งราคาขาย
        ↓
ขาย
        ↓
คำนวณกำไร
        ↓
ติดตามสินค้าเก่า / เงินจม
```

ระบบถูกออกแบบให้รองรับหลายร้านตั้งแต่ต้น โดยแต่ละร้านมีข้อมูลแยกจากกัน

---

# 2. Project Goals

## 2.1 เป้าหมายหลัก

1. ลดเวลาการทำงานของร้าน
2. ลดความผิดพลาดในการรับซื้อ
3. ควบคุมต้นทุนสินค้ามือสอง
4. รู้กำไรจริงของสินค้าแต่ละชิ้น
5. ลดปัญหาสินค้าค้าง Stock
6. เก็บประวัติสินค้าแต่ละชิ้น
7. รองรับ Serial Number
8. รองรับการตรวจสภาพสินค้า
9. รองรับหลายร้าน
10. สามารถพัฒนาเป็น SaaS ในอนาคต

---

# 3. System Architecture

ใช้แนวทาง:

- Modular Monolith
- Clean Architecture
- Multi-Tenant
- REST API
- SPA

```text
                    MUE2MUEDEE
                         |
                  Authentication
                         |
                        User
                         |
                     UserStore
                         |
                       Store
                         |
        +----------------+----------------+
        |                |                |
     Purchase         Inventory          Sales
        |                |                |
     Appraisal        Inspection       Payment
        |                |
     Inspection       Pricing
        |
      Cost
```

ไม่ใช้ Microservices ใน MVP

เหตุผล:

- ลดความซับซ้อน
- ลด Infrastructure Cost
- Develop ได้เร็ว
- Debug ง่าย
- เหมาะกับระบบเริ่มต้น
- สามารถแยก Service ในอนาคตได้หากจำเป็น

---

# 4. Technology Stack

## 4.1 Frontend

### Angular

Version:

```text
Angular 21+
```

ใช้:

- Standalone Components
- Angular Router
- HttpClient
- Reactive Forms
- Signals ตามความเหมาะสม

### UI

```text
Angular Material
Tailwind CSS
```

Angular Material ใช้สำหรับ:

- Table
- Dialog
- Form
- Date Picker
- Select
- Snackbar
- Pagination

Tailwind ใช้สำหรับ:

- Layout
- Spacing
- Responsive UI
- Custom Design

---

# 5. Backend

## 5.1 ASP.NET Core

```text
ASP.NET Core 10 Web API
C#
Entity Framework Core
```

แนวทาง:

```text
Controller
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
```

---

# 6. Database

ใช้:

```text
PostgreSQL
```

เหตุผล:

- Open Source
- ไม่มี License Cost
- รองรับระบบธุรกิจได้ดี
- รองรับ JSON
- รองรับ Index และ Transaction
- เหมาะกับระบบที่ต้องการขยายในอนาคต

ORM:

```text
Entity Framework Core
```

---

# 7. Authentication

ระบบต้องมี Login ตั้งแต่ MVP

เหตุผล:

- แยกผู้ใช้งาน
- แยกสิทธิ์
- แยกข้อมูลร้าน
- รองรับ Multi-Tenant
- รองรับหลาย User ต่อร้าน

Flow:

```text
Login
  ↓
Authentication
  ↓
User
  ↓
UserStore
  ↓
Store
  ↓
JWT
  ↓
Angular
```

JWT Payload ตัวอย่าง:

```json
{
  "sub": "user-id",
  "store_id": "store-id",
  "role": "OWNER"
}
```

Backend ต้องใช้ `StoreId` จาก JWT Claims

ไม่ควรเชื่อ `StoreId` ที่ Frontend ส่งมาเอง

---

# 8. Multi-Tenant

ระบบออกแบบให้รองรับหลายร้าน

```text
MUE2MUEDEE
│
├── Store A
│   ├── Users
│   ├── Products
│   ├── Inventory
│   ├── Purchases
│   └── Sales
│
├── Store B
│   ├── Users
│   ├── Products
│   ├── Inventory
│   ├── Purchases
│   └── Sales
│
└── Store C
```

ข้อมูล Business หลักต้องมี:

```text
StoreId
```

ตัวอย่าง:

```text
InventoryItem
----------------
Id
StoreId
ProductId
SerialNumber
Cost
SellingPrice
Status
```

---

# 9. Store Management

แต่ละร้านสามารถกำหนดข้อมูลของตัวเองได้

```text
Store
----------------
Id
Name
LogoUrl
Phone
Email
Address
TaxId
Facebook
Line
Website
PrimaryColor
CreatedAt
UpdatedAt
```

## ตัวอย่าง

```text
Store Name:
ABC Camera

Logo:
abc-camera.png

Phone:
08x-xxx-xxxx

Address:
Bangkok
```

ข้อมูล Store จะถูกนำไปใช้ใน:

- Dashboard
- ใบรับซื้อ
- ใบขาย
- ใบเสร็จ
- Report
- PDF
- Email
- Branding

---

# 10. User & Role

## Roles

### OWNER

สิทธิ์ทั้งหมด

- Dashboard
- Purchase
- Appraisal
- Inspection
- Inventory
- Pricing
- Sales
- Customer
- Reports
- Store Settings
- User Management

### ADMIN

- Purchase
- Appraisal
- Inspection
- Inventory
- Pricing
- Sales
- Customer
- Reports

ไม่สามารถจัดการ Owner หรือเปลี่ยน Store หลักได้

### STAFF

- รับซื้อ
- ตรวจสินค้า
- ดู Stock
- ขายสินค้า

ไม่ควรเห็น:

- ต้นทุน
- กำไร
- Financial Reports

หาก Owner ไม่อนุญาต

---

# 11. Main Modules

```text
1. Authentication
2. Store
3. Dashboard
4. Customer
5. Product
6. Purchase
7. Appraisal
8. Inspection
9. Inventory
10. Cost
11. Pricing
12. Sales
13. Payment
14. Repair
15. Warranty
16. Reports
17. Settings
18. Audit Log
```

---

# 12. Product Management

Product ใช้เก็บข้อมูล Model ของสินค้า

ตัวอย่าง:

```text
Brand:
Sony

Category:
Camera

Model:
A7 III
```

ไม่ควรเก็บสินค้าแต่ละเครื่องไว้ใน Product

เพราะสินค้ามือสองแต่ละเครื่องมี:

- Serial ต่างกัน
- Condition ต่างกัน
- Cost ต่างกัน
- Shutter Count ต่างกัน
- Defect ต่างกัน
- ราคาขายต่างกัน

ดังนั้น:

```text
Product
    ↓
InventoryItem
```

---

# 13. Inventory Item

InventoryItem คือสินค้าจริงแต่ละชิ้น

ตัวอย่าง:

```text
Product:
Sony A7 III

Inventory Item #1
Serial: A123
Cost: 24,000
Condition: B+
Selling Price: 29,900

Inventory Item #2
Serial: A456
Cost: 25,500
Condition: A
Selling Price: 32,000
```

---

# 14. Serial Number

รองรับ Serial Number เป็น Core Feature

ใช้สำหรับ:

- Camera
- Lens
- Flash
- อุปกรณ์ที่มี Serial

Flow:

```text
Purchase
   ↓
Serial Number
   ↓
Inventory
   ↓
Sale
   ↓
Customer
```

สามารถย้อนประวัติสินค้าได้

```text
Serial
 ↓
รับมาจากใคร
 ↓
ซื้อราคาเท่าไร
 ↓
ซ่อมอะไร
 ↓
ขายเมื่อไร
 ↓
ขายให้ใคร
```

---

# 15. Purchase / รับซื้อ

เป็น Core Module ของร้านมือสอง

Flow:

```text
Customer
   ↓
นำสินค้ามาขาย
   ↓
Purchase
   ↓
Appraisal
   ↓
Inspection
   ↓
Offer Price
   ↓
Customer Accept
   ↓
Payment
   ↓
Inventory
```

---

# 16. Appraisal

ระบบประเมินราคาสินค้า

ข้อมูลหลัก:

```text
Market Price
Condition
Missing Accessories
Repair Estimate
Cleaning Cost
Expected Selling Price
Target Margin
Maximum Purchase Price
```

ตัวอย่าง:

```text
Market Price       32,000
Condition          B+
Repair             500
Cleaning           300
Expected Sale      30,000

Maximum Buy Price  24,000
```

---

# 17. Inspection

ใช้ตรวจสภาพสินค้า

## Camera Inspection

```text
Exterior
Sensor
Shutter
LCD
EVF
AF
IBIS
Button
Dial
SD Slot
Battery
USB
HDMI
```

## Lens Inspection

```text
Exterior
Front Glass
Rear Glass
Dust
Fungus
Haze
Scratch
AF
Zoom Ring
Focus Ring
Mount
```

ระบบควรใช้ Inspection Template

```text
InspectionTemplate
        ↓
InspectionTemplateItem
        ↓
Inspection
        ↓
InspectionResult
```

เพื่อรองรับสินค้าแต่ละประเภท

---

# 18. Condition

ตัวอย่างระดับสภาพ:

```text
A+
A
A-
B+
B
B-
C
D
```

หรือสามารถตั้งค่าเองได้ตาม Store

รายละเอียด Condition สามารถประกอบด้วย:

- Grade
- Description
- Defect
- Photos

---

# 19. Cost Management

ต้องแยก:

```text
Purchase Cost
Repair Cost
Cleaning Cost
Shipping Cost
Other Cost
```

ตัวอย่าง:

```text
Purchase Cost      24,000
Repair Cost           500
Cleaning Cost         300
Shipping Cost         200
Other Cost              0
--------------------------
Actual Cost         25,000
```

กำไรต้องคำนวณจาก Actual Cost

ไม่ใช่ Purchase Cost เพียงอย่างเดียว

---

# 20. Pricing

ระบบช่วยแนะนำราคาขาย

Input:

```text
Actual Cost
Target Margin
Market Price
Condition
Stock Aging
```

Output:

```text
Minimum Price
Recommended Price
Maximum Price
```

ตัวอย่าง:

```text
Actual Cost       25,000
Target Margin       15%

Recommended Price 29,412
```

---

# 21. Inventory Status

ตัวอย่างสถานะ:

```text
DRAFT
PENDING_INSPECTION
AVAILABLE
RESERVED
SOLD
REPAIR
RETURNED
DAMAGED
WRITTEN_OFF
```

Flow:

```text
Purchase
   ↓
PENDING_INSPECTION
   ↓
AVAILABLE
   ↓
RESERVED
   ↓
SOLD
```

---

# 22. Stock Movement

ทุกการเปลี่ยนแปลง Stock ต้องมีประวัติ

ตัวอย่าง:

```text
RECEIVE
TRANSFER
ADJUST
RESERVE
RELEASE
SELL
RETURN
REPAIR
```

ตัวอย่าง:

```text
Inventory Movement

2026-10-01
RECEIVE
+1

2026-10-05
RESERVE
0

2026-10-06
SELL
-1
```

---

# 23. Sales

ระบบขายสินค้า

```text
Select Customer
       ↓
Select Inventory Item
       ↓
Check Stock
       ↓
Create Sale
       ↓
Payment
       ↓
Update Inventory
       ↓
Calculate Profit
```

ต้องผูกกับ InventoryItem และ Serial Number

---

# 24. Profit

ตัวอย่าง:

```text
Selling Price      29,000
Actual Cost        25,000
--------------------------
Gross Profit        4,000
```

Margin:

```text
4,000 / 29,000 × 100
= 13.79%
```

สามารถดูได้:

- กำไรต่อสินค้า
- กำไรต่อวัน
- กำไรต่อเดือน
- กำไรตาม Category
- กำไรตาม Brand
- กำไรตาม Sales Channel

---

# 25. Stock Aging

เป็น Core Feature สำหรับร้านมือสอง

ตัวอย่าง:

```text
0-30 Days       Green
31-60 Days      Green
61-90 Days      Yellow
91-120 Days     Orange
120+ Days       Red
```

Dashboard:

```text
Inventory Aging

0-30 Days       600,000
31-60 Days      300,000
61-90 Days      200,000
90+ Days        150,000
```

สามารถสร้าง Alert:

```text
สินค้าอยู่ใน Stock เกิน 120 วัน
ควรพิจารณาลดราคา
```

---

# 26. Customer

Customer สามารถเป็นได้ทั้ง:

```text
Buyer
Seller
Both
```

ข้อมูล:

```text
Customer
Name
Phone
Email
Address
TaxId
```

ประวัติ:

```text
Purchase History
Selling History
Repair History
Warranty History
```

---

# 27. Repair

รองรับการส่งซ่อม

```text
InventoryItem
    ↓
Repair
    ↓
Repair Cost
    ↓
Repair Status
    ↓
Return to Stock
```

Status:

```text
WAITING
IN_REPAIR
WAITING_PART
COMPLETED
CANCELLED
```

---

# 28. Warranty

เชื่อมกับ InventoryItem

```text
Sale
 ↓
Warranty
 ↓
Customer
```

ข้อมูล:

```text
Warranty Start
Warranty End
Warranty Type
Warranty Terms
```

---

# 29. File / Image Management

สินค้ามือสองจำเป็นต้องเก็บรูป

ประเภท:

```text
Product Image
Inspection Image
Defect Image
Serial Image
Receipt Image
Repair Image
```

ตัวอย่าง:

```text
InventoryItem
├── Front.jpg
├── Back.jpg
├── Top.jpg
├── Bottom.jpg
├── Serial.jpg
├── Sensor.jpg
└── Defect-01.jpg
```

MVP:

```text
Local File Storage
```

Future:

```text
S3-compatible Storage
```

---

# 30. Dashboard

Dashboard ของร้าน

```text
+------------------------------------------+
| ABC CAMERA                               |
+------------------------------------------+

Sales Today             ฿45,000
Sales This Month       ฿380,000

Gross Profit            ฿52,000

Inventory Value        ฿850,000

Available Items              42
Reserved Items                5
Sold This Month               18

Stock > 90 Days               4
Stock > 120 Days              2
```

---

# 31. Reports

MVP Reports:

```text
Sales Report
Purchase Report
Inventory Report
Profit Report
Stock Aging Report
Cost Report
```

Future:

```text
Brand Analysis
Category Analysis
Sales Channel Analysis
Customer Analysis
```

---

# 32. Audit Log

ระบบต้องเก็บประวัติการเปลี่ยนแปลงข้อมูลสำคัญ

ตัวอย่าง:

```text
User:
Admin

Action:
CHANGE_PRICE

Inventory:
Sony A7 III / SN A123

Old Value:
29,900

New Value:
28,900

CreatedAt:
2026-10-05 10:30
```

ควรเก็บอย่างน้อย:

```text
UserId
StoreId
Action
Entity
EntityId
OldValue
NewValue
CreatedAt
```

---

# 33. Database Structure

## Authentication

```text
users
roles
permissions
user_roles
```

## Store

```text
stores
user_stores
```

## Product

```text
brands
categories
products
inventory_items
inventory_serials
```

## Purchase

```text
purchases
purchase_items
appraisals
appraisal_items
inspections
inspection_results
```

## Cost

```text
costs
cost_types
```

## Sales

```text
sales
sale_items
payments
```

## Customer

```text
customers
```

## Repair

```text
repairs
repair_items
```

## Warranty

```text
warranties
```

## Files

```text
files
```

## Audit

```text
audit_logs
```

---

# 34. Backend Project Structure

```text
src/

Mue2MueDee.Domain/
├── Entities/
├── Enums/
├── ValueObjects/
└── Interfaces/

Mue2MueDee.Application/
├── Authentication/
├── Stores/
├── Customers/
├── Products/
├── Purchases/
├── Appraisals/
├── Inspections/
├── Inventory/
├── Pricing/
├── Sales/
├── Payments/
├── Repairs/
├── Warranties/
└── Reports/

Mue2MueDee.Infrastructure/
├── Persistence/
├── Identity/
├── Storage/
├── Services/
└── Configurations/

Mue2MueDee.Web/
├── Controllers/
├── Middleware/
├── Extensions/
└── Program.cs
```

---

# 35. Angular Project Structure

```text
src/app/

├── core/
│   ├── auth/
│   ├── guards/
│   ├── interceptors/
│   └── services/
│
├── shared/
│   ├── components/
│   ├── dialogs/
│   ├── pipes/
│   └── directives/
│
├── layout/
│   ├── main-layout/
│   ├── sidebar/
│   └── header/
│
└── features/
    ├── dashboard/
    ├── customers/
    ├── products/
    ├── purchase/
    ├── appraisal/
    ├── inspection/
    ├── inventory/
    ├── pricing/
    ├── sales/
    ├── payments/
    ├── repairs/
    ├── warranties/
    └── reports/
```

---

# 36. API Structure

```text
/api/auth
/api/stores
/api/users
/api/customers
/api/brands
/api/categories
/api/products
/api/inventory
/api/purchases
/api/appraisals
/api/inspections
/api/pricing
/api/sales
/api/payments
/api/repairs
/api/warranties
/api/reports
```

---

# 37. Security

ต้องมี:

- JWT Authentication
- Role-based Authorization
- Store-based Authorization
- Password Hashing
- HTTPS
- Input Validation
- API Authorization
- Audit Log
- Rate Limiting ในจุดที่เหมาะสม

หลักสำคัญ:

```text
User
 ↓
Store
 ↓
Data
```

User ต้องเข้าถึงเฉพาะข้อมูล Store ที่ตัวเองมีสิทธิ์

---

# 38. Deployment

## MVP

```text
Windows Server
      │
     IIS
      │
      ├── Angular SPA
      │
      └── ASP.NET Core API
               │
               ↓
           PostgreSQL
```

ไม่ใช้ Docker ใน MVP

เหตุผล:

- ลด Infrastructure Complexity
- เหมาะกับ Environment ที่มีอยู่
- Deploy ง่าย
- Debug ง่าย

---

# 39. Development Phase

## Phase 1 — Foundation

- [ ] Create Repository
- [ ] Setup Angular
- [ ] Setup ASP.NET Core
- [ ] Setup PostgreSQL
- [ ] Setup EF Core
- [ ] Setup Clean Architecture
- [ ] Setup Authentication
- [ ] Setup JWT
- [ ] Setup Store
- [ ] Setup User / Role
- [ ] Setup Multi-Tenant

---

## Phase 2 — Product & Inventory

- [ ] Brand
- [ ] Category
- [ ] Product
- [ ] Inventory Item
- [ ] Serial Number
- [ ] Stock Status
- [ ] Stock Movement
- [ ] Image Upload

---

## Phase 3 — Purchase

- [ ] Customer
- [ ] Purchase
- [ ] Purchase Item
- [ ] Appraisal
- [ ] Inspection Template
- [ ] Inspection
- [ ] Condition
- [ ] Purchase Approval
- [ ] Payment
- [ ] Receive Stock

---

## Phase 4 — Cost & Pricing

- [ ] Cost Management
- [ ] Actual Cost
- [ ] Market Price
- [ ] Target Margin
- [ ] Recommended Price
- [ ] Price History

---

## Phase 5 — Sales

- [ ] Sales
- [ ] Sale Items
- [ ] Payment
- [ ] Serial Tracking
- [ ] Update Inventory
- [ ] Profit Calculation

---

## Phase 6 — Dashboard

- [ ] Sales Dashboard
- [ ] Profit Dashboard
- [ ] Inventory Value
- [ ] Stock Aging
- [ ] Low Stock
- [ ] Dead Stock

---

## Phase 7 — Reports

- [ ] Sales Report
- [ ] Purchase Report
- [ ] Inventory Report
- [ ] Profit Report
- [ ] Aging Report
- [ ] Cost Report

---

## Phase 8 — Advanced

- [ ] Repair
- [ ] Warranty
- [ ] Online Listing
- [ ] LINE Integration
- [ ] Facebook Integration
- [ ] Marketplace Integration
- [ ] AI Price Suggestion
- [ ] AI Condition Assistance
- [ ] Demand Prediction

---

# 40. MVP Scope

MVP แรกไม่ควรทำทุก Module

ให้โฟกัส:

```text
Authentication
      ↓
Store
      ↓
Customer
      ↓
Purchase
      ↓
Appraisal
      ↓
Inspection
      ↓
Inventory
      ↓
Cost
      ↓
Pricing
      ↓
Sales
      ↓
Profit
```

เป้าหมายของ MVP คือสามารถทำ Workflow จริงได้ตั้งแต่:

> ลูกค้านำกล้องมาขาย → รับซื้อ → ตรวจ → เข้าสต๊อก → ตั้งราคา → ขาย → รู้กำไร

---

# 41. Future SaaS Model

ในอนาคตสามารถพัฒนาเป็น SaaS:

```text
MUE2MUEDEE
│
├── Tenant / Store A
├── Tenant / Store B
├── Tenant / Store C
└── Tenant / Store D
```

แต่ละร้านมี:

- Logo
- Store Name
- Users
- Products
- Customers
- Inventory
- Purchase
- Sales
- Reports

ข้อมูลแยกจากกันทั้งหมด

---

# 42. Future Feature: Online Listing

เมื่อ Inventory พร้อมขาย สามารถกด:

```text
[ Create Listing ]
```

ระบบสร้าง:

```text
Title
Description
Price
Condition
Shutter Count
Accessories
Defects
Photos
```

แล้วนำไปใช้กับ:

```text
Facebook
LINE
Website
Marketplace
Shopee
TikTok
```

โดยไม่ต้องกรอกข้อมูลใหม่ทั้งหมด

---

# 43. Future Feature: AI

AI ไม่ควรอยู่ใน MVP

แต่สามารถเพิ่มภายหลังได้ เช่น:

### AI Price Suggestion

```text
Product
+
Condition
+
Market Data
+
Aging
+
Historical Sales
        ↓
AI Suggested Price
```

### AI Listing

```text
รูปสินค้า
+
Inspection Result
        ↓
AI
        ↓
Product Description
```

### AI Condition Assistance

```text
Product Photos
        ↓
AI
        ↓
Possible Defects
```

AI ควรเป็นตัวช่วย ไม่ใช่ตัวตัดสินราคาหรือสภาพโดยอัตโนมัติทั้งหมด

---

# 44. Development Principles

1. เริ่มจาก Business Flow จริง
2. อย่าเริ่มจาก Database ก่อน
3. ทุก Transaction ต้องตรวจ StoreId
4. สินค้ามือสองต้องแยก Product กับ InventoryItem
5. Serial Number ต้อง Trace ย้อนกลับได้
6. ต้นทุนต้องเก็บเป็นรายการ
7. กำไรต้องคำนวณจาก Actual Cost
8. ทุก Stock Movement ต้องมีประวัติ
9. ข้อมูลสำคัญต้องมี Audit Log
10. MVP ต้องเล็กและใช้งานจริงได้
11. ไม่ทำ Microservices จนกว่าจะมีเหตุผล
12. ไม่เพิ่ม AI จนกว่ากระบวนการพื้นฐานจะนิ่ง

---

# 45. Recommended Technology Summary

```text
Frontend
Angular 21+

UI
Angular Material
Tailwind CSS

Backend
ASP.NET Core 10
C#

Architecture
Clean Architecture
Modular Monolith

ORM
Entity Framework Core

Database
PostgreSQL

Authentication
JWT

Authorization
Role + Store-based Authorization

File Storage
Local Storage → S3-compatible Storage

Deployment
IIS

Version Control
Git

CI/CD
GitHub Actions / GitLab CI

Container
ไม่ใช้ใน MVP
```

---

# 46. Core Business Flow

```text
                    CUSTOMER
                       │
                       │ นำสินค้ามาขาย
                       ↓
                  ┌──────────┐
                  │ PURCHASE │
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │ APPRAISAL│
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │INSPECTION│
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │   COST   │
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │ INVENTORY │
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │  PRICING │
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │   SALES  │
                  └────┬─────┘
                       ↓
                  ┌──────────┐
                  │  PROFIT  │
                  └──────────┘
```

---

# 47. Final MVP Goal

MUE2MUEDEE MVP ต้องสามารถตอบคำถามสำคัญของเจ้าของร้านได้:

### 1. สินค้านี้รับมาเท่าไร?

```text
Purchase Cost
```

### 2. สินค้านี้ต้นทุนจริงเท่าไร?

```text
Actual Cost
```

### 3. ตอนนี้ควรขายเท่าไร?

```text
Recommended Price
```

### 4. ถ้าขายแล้วกำไรเท่าไร?

```text
Gross Profit
```

### 5. สินค้านี้อยู่ในร้านมากี่วัน?

```text
Stock Aging
```

### 6. สินค้าชิ้นนี้มาจากไหน?

```text
Purchase History
```

### 7. ขายให้ใคร?

```text
Customer
```

### 8. ตอนนี้ร้านมีเงินจมอยู่กับสินค้าเท่าไร?

```text
Inventory Value
```

---

# 48. Main Principle

> **MUE2MUEDEE ไม่ได้ถูกสร้างมาเพื่อเป็นแค่ระบบขายสินค้า**
>
> แต่ถูกสร้างมาเพื่อช่วยร้านมือสองตัดสินใจว่า
>
> **"ควรรับของเท่าไร — ต้นทุนจริงเท่าไร — ควรขายเท่าไร — และกำไรจริงเท่าไร"**

นี่คือ Core Business ของระบบ
