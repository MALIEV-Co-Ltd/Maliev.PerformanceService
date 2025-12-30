# Performance Service

Dedicated microservice for managing employee performance reviews, goals, 360-degree feedback, and development plans for Maliev Co. Ltd.

## Overview

The Performance Service enables structured feedback and goal alignment, including:

- **Performance Reviews** - Conducting periodic (annual, probation) reviews with self and manager assessments.
- **Goal Tracking** - Setting, updating, and monitoring progress on performance goals with timestamped updates.
- **360-Degree Feedback** - Collecting feedback from peers, managers, and direct reports with anonymity protection.
- **PIP Management** - Coordinating Performance Improvement Plans for employees requiring additional support.
- **Acknowledge Workflow** - Formal employee acknowledgement of review outcomes.

## Architecture

- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL 18 with Entity Framework Core
- **Messaging**: RabbitMQ via MassTransit
- **Caching**: Redis
- **Documentation**: OpenAPI with Scalar UI
- **Observability**: OpenTelemetry (Metrics, Tracing, Structured Logging)

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 18
- Docker (optional, for Redis and RabbitMQ)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/MALIEV-Co-Ltd/Maliev.PerformanceService.git
   ```

2. **Run database migrations**
   ```bash
   dotnet ef database update --project Maliev.PerformanceService.Infrastructure --startup-project Maliev.PerformanceService.Api
   ```

3. **Run the service**
   ```bash
   dotnet run --project Maliev.PerformanceService.Api
   ```

   The service will be available at `http://localhost:5211`.

## API Endpoints

### Performance Reviews

```
GET  /performance/v1/employees/{employeeId}/reviews - Get all reviews for an employee
POST /performance/v1/employees/{employeeId}/reviews - Create a new review cycle
GET  /performance/v1/reviews/{reviewId}            - Get review details
PUT  /performance/v1/reviews/{reviewId}            - Save review draft
POST /performance/v1/reviews/{reviewId}/submit     - Submit final manager assessment
POST /performance/v1/reviews/{reviewId}/acknowledge - Employee acknowledgment
```

### Goals

```
GET  /performance/v1/employees/{employeeId}/goals - List employee goals
POST /performance/v1/employees/{employeeId}/goals - Set a new goal
PUT  /performance/v1/goals/{goalId}               - Update goal details
PUT  /performance/v1/goals/{goalId}/progress      - Update goal progress and status
```

### Feedback (360-Degree)

```
POST /performance/v1/reviews/{reviewId}/feedback - Submit feedback for a review
GET  /performance/v1/reviews/{reviewId}/feedback - Get aggregated feedback for a review
```

### PIP Management

```
POST /performance/v1/employees/{employeeId}/pips - Initiate a PIP
GET  /performance/v1/employees/{employeeId}/pips - List employee PIPs
PUT  /performance/v1/pips/{pipId}                - Update check-in notes
POST /performance/v1/pips/{pipId}/outcome        - Record final PIP outcome
```

## License

Copyright © 2025 Maliev Co. Ltd. All rights reserved.