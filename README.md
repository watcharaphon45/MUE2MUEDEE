# MUE2MUEDEE — มือสองมือดี
> **Second-hand Business Management System** — ระบบบริหารร้านสินค้ามือสอง (เริ่มต้นออกแบบสำหรับร้านกล้องและเลนส์มือสอง)
> จัดการกระบวนการ: รับซื้อ → ประเมินราคา → ตรวจสภาพ → รับเข้าคลัง → คำนวณต้นทุนจริง → ตั้งราคาขาย → ขาย → คำนวณกำไร

---

## 🛠 Technology Stack

- **Backend:** ASP.NET Core 10 Web API, Clean Architecture (Modular Monolith)
- **Database:** PostgreSQL + Entity Framework Core 10
- **Frontend:** Angular 22 (Standalone Components, Signals, Router, HttpClient with fetch)
- **UI:** Tailwind CSS v4, Angular Material, Google Fonts (Prompt & Inter)
- **Architecture:** Clean Architecture + Multi-Tenant (`StoreId` isolation)

---

## 📁 Project Structure

```text
MUE2MUEDEE/
├── mue2muedee/
│   ├── Mue2MueDee.slnx
│   ├── src/
│   │   ├── Mue2MueDee.Domain/          # Entities, Enums, ValueObjects, Domain Rules
│   │   ├── Mue2MueDee.Application/     # Use Cases, DTOs, Commands, Queries, Interfaces
│   │   ├── Mue2MueDee.Infrastructure/  # EF Core, PostgreSQL Persistence, External Services
│   │   └── Mue2MueDee.Web/             # Controllers / Endpoints, Middlewares, DI Setup
│   │       └── spa/                    # Angular Standalone SPA (Tailwind CSS)
│   └── tests/
│       ├── Mue2MueDee.Domain.Tests/
│       └── Mue2MueDee.Application.Tests/
├── docs/                               # Project requirements & specifications
│   └── project_plan.md
└── README.md
```

---

## 🚀 Getting Started

### 1. Backend (ASP.NET Core 10)

```bash
cd mue2muedee
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Mue2MueDee.Web/Mue2MueDee.Web.csproj --urls "http://localhost:5101"
```

- API Base URL: `http://localhost:5101`
- Health Check: `http://localhost:5101/health`
- OpenAPI Documentation: `http://localhost:5101/openapi/v1.json`

### 2. Frontend (Angular Standalone SPA)

```bash
cd mue2muedee/src/Mue2MueDee.Web/spa
npm.cmd install
npm.cmd start
```

- Web UI: `http://localhost:4200`
