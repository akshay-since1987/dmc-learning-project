# Secrets & Environment Setup Guide

> **WARNING**: Never commit actual secrets to Git. This file documents *where* secrets
> belong and *what values* to set — not the actual production secrets.

---

## 1. V2 Backend (Active Code) — `v2/backend/src/ProposalManagement.Api/`

### `appsettings.Development.json` (NOT committed — in .gitignore)

Create this file manually on each dev machine:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=dmc-v2-ProposalMgmt;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "Otp": {
    "SimulateOtp": true,
    "DefaultOtp": "123456"
  },
  "Jwt": {
    "Key": "YOUR-JWT-SECRET-KEY-MIN-32-CHARACTERS-LONG",
    "Issuer": "ProposalManagement",
    "Audience": "ProposalManagement",
    "ExpiryMinutes": 60
  }
}
```

### What each secret does:

| Key | Purpose | How to Get |
|-----|---------|------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection | Install SQL Server Express, create DB `dmc-v2-ProposalMgmt` |
| `Jwt:Key` | HMAC-SHA256 signing key for JWT tokens | Generate any random 32+ char string |
| `Jwt:Issuer` | JWT issuer claim | Use `ProposalManagement` |
| `Jwt:Audience` | JWT audience claim | Use `ProposalManagement` |
| `Jwt:ExpiryMinutes` | Access token lifetime | `60` for dev, `15` for prod |
| `Otp:SimulateOtp` | Skip real SMS, accept any OTP | `true` for dev, `false` for prod |
| `Otp:DefaultOtp` | Fixed OTP for testing | `123456` for dev, remove for prod |

---

## 2. Docker Deployment — Root `.env` file

Copy `.env.example` to `.env` and fill in values:

```bash
cp .env.example .env
```

### Required `.env` values:

| Variable | Purpose | Example |
|----------|---------|---------|
| `SQL_SA_PASSWORD` | SQL Server SA password (container) | `YourStrong!Passw0rd` |
| `SQL_PORT` | Host port for SQL Server | `1433` |
| `APP_PORT` | Host port for the API | `5111` |
| `JWT_KEY` | JWT signing key (32+ chars) | Random strong string |
| `JWT_ISSUER` | JWT issuer | `ProposalManagement.Api` |
| `JWT_AUDIENCE` | JWT audience | `ProposalManagement.Client` |
| `JWT_ACCESS_EXPIRY_MINUTES` | Token expiry | `15` |
| `JWT_REFRESH_EXPIRY_DAYS` | Refresh token expiry | `7` |
| `OTP_SIMULATE` | Simulate OTP in dev | `false` for prod |
| `SMS_API_URL` | SMS gateway endpoint | Provider-specific |
| `SMS_API_KEY` | SMS gateway API key | Provider-specific |

---

## 3. V1 Backend (___backend/) — Reference Only

The `___backend/` folder contains V1 code with hardcoded secrets in appsettings files.
These are committed for reference but should **never be used in production**:

- `___backend/src/ProposalManagement.Api/appsettings.json` — contains DB IP, user/pass
- Multiple `_buildcheck*/appsettings.json` files — contain JWT keys

**Action**: These files are included in the repo for learning/reference. The secrets
in them are for a development environment and should be rotated if ever used elsewhere.

---

## 4. Test Users (Development Only)

All test users use OTP `123456` when `SimulateOtp=true`:

| Mobile       | Role            | Purpose                    |
|-------------|-----------------|----------------------------|
| 9999999999  | Lotus           | Super admin                |
| 8888000001  | JE              | Junior Engineer (proposer) |
| 8888000002  | TS              | Technical Supervisor       |
| 8888000003  | CityEngineer    | City Engineer              |
| 8888000004  | AccountOfficer  | Account Officer            |
| 8888000005  | DyCommissioner  | Deputy Commissioner        |
| 8888000006  | Commissioner    | Commissioner               |
| 8888000007  | Auditor         | Audit viewer               |

---

## 5. Setting Up on a New Machine

```bash
# 1. Install prerequisites
#    - .NET SDK 10.0.102+
#    - SQL Server Express (instance: .\SQLEXPRESS)
#    - Git, VS Code

# 2. Clone repo
git clone https://github.com/akshay-since1987/dmc-learning-project.git
cd dmc-learning-project

# 3. Create database
sqlcmd -S .\SQLEXPRESS -E -Q "CREATE DATABASE [dmc-v2-ProposalMgmt]"

# 4. Create appsettings.Development.json (see Section 1 above)
# Place in: v2/backend/src/ProposalManagement.Api/appsettings.Development.json

# 5. Run the application (EF will apply migrations / create tables on startup)
cd v2/backend/src/ProposalManagement.Api
dotnet run --urls http://localhost:5108

# 6. Seed test data (if DbInitializer doesn't auto-seed)
# Run SQL scripts from v2/database/ against the database

# 7. Open browser → http://localhost:5108
# Login with: 9999999999 / 123456
```

---

## 6. Production Checklist

- [ ] Generate a strong random JWT key (32+ chars, no dictionary words)
- [ ] Set `Otp:SimulateOtp` = `false`
- [ ] Configure real SMS gateway (API URL + key)
- [ ] Use SQL Server with a dedicated app user (not SA/Windows Auth)
- [ ] Enable HTTPS with a valid certificate
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Review and rotate all secrets from development
- [ ] Configure backup strategy for SQL Server database
