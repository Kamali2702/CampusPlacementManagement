# Campus Placement Management System

## Company Management Module

### Overview
This module allows companies to register on the Campus Placement Management Platform. A SuperAdmin can review company registrations and approve, reject, or suspend companies.

### Technologies
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- SQL Server
- JWT Authentication
- Repository Pattern
- Dependency Injection

### Company Details
- CompanyId
- CompanyName
- CompanyEmail
- Website
- Industry
- Address
- Description
- Status
- CreatedAt

### Company Status
- Pending
- Approved
- Rejected
- Suspended
- Inactive

### API Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/companies/register` | Public |
| GET | `/api/companies` | SuperAdmin |
| GET | `/api/companies/pending` | SuperAdmin |
| GET | `/api/companies/{companyId}` | SuperAdmin |
| PUT | `/api/companies/{companyId}/approve` | SuperAdmin |
| PUT | `/api/companies/{companyId}/reject` | SuperAdmin |
| PUT | `/api/companies/{companyId}/suspend` | SuperAdmin |

### Business Rules
1. New companies are registered with Pending status.
2. Duplicate company email addresses are not allowed.
3. Only Pending companies can be approved or rejected.
4. Only Approved companies can be suspended.
5. Company management endpoints require SuperAdmin authorization, except public registration.

### Project Structure
- Models – Company entity
- Enums – CompanyStatus
- DTOs/Company – Company request and response DTOs
- Repositories – Company database operations
- Services – Company business logic
- Controllers – Company API endpoints
- Data – ApplicationDbContext

### Testing
All Company Management API tests were completed successfully using Postman.

| Test | Expected result | Actual result |
|---|---|---|
| Company registration | 201 Created | Passed |
| Get all companies | 200 OK | Passed |
| Get pending companies | 200 OK | Passed |
| Get company by ID | 200 OK | Passed |
| Approve company | 200 OK | Passed |
| Approve again | 400 Bad Request | Passed |
| Reject company | 200 OK | Passed |
| Reject again | 400 Bad Request | Passed |
| Suspend company | 200 OK | Passed |
| Suspend again | 400 Bad Request | Passed |
| Duplicate email registration | 409 Conflict | Passed |

### Final Test Data
- Company ID 1 – Tech Solutions Pvt Ltd – Suspended
- Company ID 2 – ABC Technologies – Rejected