# PRN232 - Lab 2: Advanced REST API & Security (LMS)

This project extends the Learning Management System (LMS) RESTful API developed in Lab 1 into a production-ready ASP.NET Core 9 service adhering to all technical requirements and design standards of **PRN232 Lab 2**.

---

## 1. Project Architecture (3-Layer Architecture)

```
PRN232.LMS/
├── PRN232.LMS.API/             # API Layer (Controllers, Middlewares, Versioning, Swagger, Docker setup)
├── PRN232.LMS.Services/        # Service Layer (Business Logic, AuthService, Validation, Data Shaping)
├── PRN232.LMS.Repositories/    # Repository Layer (Data Access, EF Core DbContext, Entities, User Seeds)
├── Dockerfile                  # Multi-stage Dockerfile for API container
└── docker-compose.yml          # Docker Compose orchestration (SQL Server + API with JWT env vars)
```

### Separation of Concerns:
- **API Layer (Controllers):** Handles HTTP requests/responses, status codes, input binding, and routing. No business logic.
- **Service Layer (Business Logic):** Handles queries, data shaping, expansion, business validation (FluentValidation + Custom validation), JWT generation, and BCrypt verification.
- **Repository Layer (Data Access):** Uses Generic Repository Pattern `IRepository<T>` for EF Core data access.

---

## 2. Lab 2 Feature Checklist

- [x] **3-Layer Architecture & Model Separation:** Entity, Business, Request, and Response DTOs.
- [x] **Database & Authentication Tables:** `User` (BCrypt password hashing) and `RefreshToken` (Token rotation).
- [x] **Content Negotiation:** Supports `application/json` and `application/xml`. Returns **HTTP 406 Not Acceptable** for unsupported formats.
- [x] **Model Binding:** Implemented `[FromRoute]`, `[FromQuery]`, `[FromBody]`, and `[FromHeader(Name = "X-Request-Id")]`.
- [x] **Data Validation:** 
  - Standard Data Annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[RegularExpression]`).
  - FluentValidation (`CreateStudentRequestValidator`).
  - Custom Validation (`[FptuRollNumber]` verifying FPT roll numbers like `SE19886`, `CE18793`).
- [x] **Advanced Routing & API Versioning:**
  - Route Constraints (`{id:int}`, `{courseId:int}`).
  - Nested Resources (`GET /api/v1/courses/{courseId}/students`).
  - Named Routes (`Name = "GetStudentById"` with `CreatedAtRoute`).
  - Versioning: `/api/v1/...` and `/api/v2/...` (`Asp.Versioning.Mvc`).
- [x] **Custom Middleware:**
  - `GlobalExceptionMiddleware`: Global unhandled exception catching, uniform 500 error envelope, masking stack traces.
  - `RequestLoggingMiddleware`: Logs HTTP method, path, response status code, and execution time (ms).
- [x] **JWT Security & Authorization:**
  - `POST /api/v1/auth/register`: Register new user with password hashed via BCrypt, automatically assigning JWT tokens.
  - `POST /api/v1/auth/login`: Authenticate and return `accessToken` and `refreshToken`.
  - `POST /api/v1/auth/refresh-token`: Refresh expired access token with token rotation.
  - Role-based Authorization: `[Authorize(Roles = "Admin")]` for administrative actions (e.g. `DELETE` operations).
- [x] **Swagger / OpenAPI:** Interactive Swagger UI with version selection and JWT Bearer **Authorize** button.
- [x] **Docker Deployment:** Multi-stage `Dockerfile` and `docker-compose.yml` configuring JWT secrets via environment variables.

---

## 3. Default Seed Credentials

| Role | Username | Password | Notes |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `123456` | Full administrative privileges (delete students, manage courses/subjects/semesters) |
| **Student** | `student` | `123456` | Student role |

---

## 4. How to Run

### Option 1: .NET CLI (Local Development)
```bash
cd PRN232.LMS.API
dotnet run
```
Access Swagger UI at: `http://localhost:5204/` or `http://localhost:5204/swagger`

### Option 2: Docker Compose
```bash
docker-compose up --build
```
Access Swagger UI at: `http://localhost:5000/` or `http://localhost:5000/swagger`
