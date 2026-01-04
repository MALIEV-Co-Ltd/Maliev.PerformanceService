# Maliev Performance Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.PerformanceService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Comprehensive employee performance management platform for goal alignment, feedback, and developmental reviews.

**Role in MALIEV Architecture**: The authoritative engine for talent development. It manages the complete performance cycle, including goal tracking, 360-degree feedback, and formal reviews, ensuring organizational alignment and employee growth within the Maliev ecosystem.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (High-frequency feedback resolution)
- **Messaging**: RabbitMQ via MassTransit
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **Performance Review Cycles**: Structured periodic assessment workflows (Annual, Probation) with self and manager review stages.
- **Dynamic Goal Tracking**: Real-time setting and progress monitoring of employee performance goals with complete history.
- **360-Degree Feedback**: Anonymized feedback engine collecting insights from peers, managers, and direct reports.
- **Performance Improvement Plans (PIP)**: Specialized tracking for supporting employee growth through structured check-ins and outcome monitoring.
- **Formal Recognition Workflow**: Transparent acknowledgement system for employees to formally review and sign off on performance outcomes.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.PerformanceService.git
cd Maliev.PerformanceService
```

2. **Spin up Infrastructure**
```bash
docker run --name performance-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name performance-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__PerformanceDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.PerformanceService.Infrastructure --startup-project Maliev.PerformanceService.Api
dotnet run --project Maliev.PerformanceService.Api
```

The service will be available at `http://localhost:5000/performance`. Access the interactive documentation at `http://localhost:5000/performance/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/performance/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/employees/{id}/reviews` | List all performance reviews for a specific employee |
| POST | `/employees/{id}/goals` | Set a new performance goal |
| POST | `/reviews/{id}/feedback` | Submit 360-degree feedback for a review |
| POST | `/employees/{id}/pips` | Initiate a Performance Improvement Plan |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /performance/liveness`
- **Readiness**: `GET /performance/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /performance/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-performance-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.