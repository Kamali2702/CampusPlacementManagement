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


## Company HR Management

### Overview
Company HR representatives can register under an approved company. Their accounts remain Pending until approved or rejected by the SuperAdmin.

### Features
- Register Company HR under an approved company.
- Prevent duplicate email registration.
- View all Company HR accounts.
- View pending Company HR accounts.
- View Company HR details by ID.
- Allow an approved Company HR to view their own profile.
- SuperAdmin can approve or reject Company HR accounts.
- Assign the CompanyHR role after approval.
- Prevent rejected or pending accounts from logging in.

### API Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | /api/company-hrs/register | Public |
| GET | /api/company-hrs | SuperAdmin |
| GET | /api/company-hrs/pending | SuperAdmin |
| GET | /api/company-hrs/{id} | SuperAdmin |
| GET | /api/company-hrs/me | CompanyHR |
| PUT | /api/company-hrs/{id}/approve | SuperAdmin |
| PUT | /api/company-hrs/{id}/reject | SuperAdmin |

### Account Workflow

1. Company HR submits registration details.
2. The selected company must have Approved status.
3. A new user account is created with Pending status.
4. SuperAdmin reviews the registration.
5. On approval, the account becomes Approved and receives the CompanyHR role.
6. On rejection, the account becomes Rejected and cannot log in.

### Database
- CompanyHRId
- UserId (FK to ApplicationUser)
- CompanyId (FK to Company)
- Designation
- ApprovedBy
- ApprovedAt

Each Company HR belongs to one company. A company can have multiple Company HR representatives.

### Postman Testing
- Company HR registration – 201 Created
- Get pending Company HRs – 200 OK
- Approve Company HR – 200 OK
- Company HR login – 200 OK
- Get own profile – 200 OK
- Get all Company HRs – 200 OK
- Get Company HR by ID – 200 OK
- Duplicate approval – 400 Bad Request
- Registration under rejected company – 409 Conflict
- Duplicate email – 409 Conflict
- Reject Company HR – 200 OK
- Rejected account login – Blocked
- Pending list excludes approved and rejected accounts