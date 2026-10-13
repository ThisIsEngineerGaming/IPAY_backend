# IPAY — Amazon Clone (Backend)

ASP.NET Core Web API for **IPAY** ("Incredible Project Aspnet Yes"), an Amazon-style e-commerce platform built as a team course project. It serves the React client and covers authentication, the product catalog, carts and orders, ratings, a video catalog (series, episodes, genres) and role-based administration.

## Links

| Resource | URL |
|---|---|
| Live frontend | https://ipay-client.onrender.com/ |
| Frontend repository | https://github.com/ysantonyance/ipay_client |
| API reference (Scalar registry) | https://registry.scalar.com/@default-team-wbd0w/apis/fat-api@v1#tag/admin |
| Code documentation (Doxygen) | https://ipay-doxygen-documentation.onrender.com/ |
| Requirements (SRS, in Ukrainian) | https://github.com/ysantonyance/IPAY_SRS |

## Tech stack

- **Runtime:** .NET 10, ASP.NET Core Web API (controllers)
- **Architecture:** Clean / Onion — `Domain` → `Application` → `Infrastructure` → `WebApi`
- **Database:** Google Cloud Firestore (one collection per entity)
- **Auth:** JWT bearer tokens, policy-based authorization, Firebase Authentication for Google sign-in, emailed two-factor codes
- **Validation:** FluentValidation (via a shared `ValidationFilter`)
- **Mapping:** AutoMapper
- **Images:** Cloudinary
- **Email:** SMTP
- **Movie data:** OMDb API
- **API docs:** OpenAPI / Swagger UI (development only)
- **Tests:** xUnit-style unit tests in `tests/IPAY.UnitTests`
- **Deployment:** Docker image, hosted on Render

## Project structure

```
IPAY_backend/
├── Dockerfile
├── IPAY.slnx
├── src/
│   ├── IPAY.Domain/          # Entities, enums, repository and service contracts
│   ├── IPAY.Application/     # DTOs, services, validators, mappings, options
│   ├── IPAY.Infrastructure/  # Firestore repositories, Cloudinary, SMTP, Firebase auth, caching
│   └── IPAY.WebApi/          # Controllers, filters, Program.cs, appsettings
└── tests/
    └── IPAY.UnitTests/       # Services, controllers, validators, domain, infrastructure
```

## Features

- **Authentication:** register, login, email verification, optional emailed 2FA codes, Google login, password reset, guest sessions
- **Profile:** change username, phone, email and password (email-confirmed flows)
- **Catalog:** products with paging, filtering and sorting; categories; manufacturers; image upload
- **Cart and orders:** cart items, orders with order items and statuses (`Pending`, `Paid`, `Shipped`, `Delivered`, `Cancelled`)
- **Ratings:** one star rating per user per product
- **Video catalog:** series, episodes and genres, with OMDb import support
- **Roles:** `Guest`, `Customer`, `Seller`, `Moderator`, `Admin`
  - Customers can apply to become sellers; staff approve or reject the request
  - Sellers create and edit only their own products (the seller id comes from the JWT)
  - Moderators and admins manage products, roles and bans, with restrictions on who can change whom
  - Nobody can be made an admin through the API
- **Role-change safety:** the JWT role is compared with the user's current role on every request, so a stale token is rejected with `401` after a role change

## API overview

All routes are under `/api`. The full, always-current reference is in the [Scalar registry](https://registry.scalar.com/@default-team-wbd0w/apis/fat-api@v1#tag/admin); when running locally in Development, Swagger UI is at `/swagger`.

| Area | Routes |
|---|---|
| Auth | `/auth/register`, `/auth/login`, `/auth/login/verify-code`, `/auth/login/resend-code`, `/auth/google`, `/auth/password-reset/{start,confirm}` |
| Session | `/session` (authenticated ping used by the client to detect role changes) |
| Profile | `/profile`, `/profile/{username,phone,password,email}` |
| Products | `/products`, `/products/all`, `/products/{id}`, `/products/filter`, `/products/sort`, `/products/seller`, `/products/seller/mine` |
| Categories / manufacturers | `/categories`, `/manufacturers` |
| Cart | `/cart`, `/cart/items`, `/cart/clear` |
| Orders | `/orders`, `/orders/{id}`, `/orders/{id}/status`, `/orders/{id}/items` |
| Ratings | `/ratings/{productId}`, `/ratings/mine` |
| Seller requests | `/seller-requests`, `/seller-requests/mine`, `/seller-requests/{id}/{approve,reject}` |
| Media | `/series`, `/episodes`, `/genres` |
| Images | `/images` |
| Admin | `/admin/users`, `/admin/users/{id}/{make-seller,make-moderator,remove-moderator,make-customer}`, `/admin/{id}/{ban,unban}`, `/admin/customers`, `/admin/guests` |

Protected endpoints expect `Authorization: Bearer <accessToken>`. A login response looks like `{ accessToken, refreshToken, expiresAt, tokenType, user }`. When emailed 2FA is enabled, login first returns `{ twoFactorRequired, challengeId, maskedEmail }` and the session is only issued after `/auth/login/verify-code`.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A Firebase project with Firestore enabled and a service-account key
- Optional: a Cloudinary account (image uploads), an SMTP account (2FA and verification emails), an OMDb API key (movie import)

### Configuration

Settings live in `src/IPAY.WebApi/appsettings.json`. **Keep secrets out of source control** and supply them through [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables (use `__` for nesting, e.g. `Jwt__Key`).

| Section | Keys | Purpose |
|---|---|---|
| `Firebase` | `ProjectId`, `ServiceAccountKeyPath`, `RequireEmailVerification` | Firestore and Firebase Authentication |
| `Jwt` | `Issuer`, `Audience`, `Key` | Token signing and validation |
| `TwoFactor` | `Enabled`, `CodeLifetimeMinutes`, `MaxAttempts`, `ResendCooldownSeconds` | Emailed login codes |
| `Email` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `FromName` | SMTP sender (2FA only works when this is configured) |
| `Cloudinary` | `CloudName`, `ApiKey`, `ApiSecret` | Image hosting |
| `OMDb` | `ApiKey`, `BaseUrl` | Movie metadata import (see `src/IPAY.WebApi/OMDB_SETUP.md`) |

Place your Firebase service-account key at `src/IPAY.WebApi/firebase-service-account.json`. It is git-ignored; never commit it.

### Run locally

```bash
dotnet run --project src/IPAY.WebApi --launch-profile https
```

The API listens on `https://localhost:7137` and `http://localhost:5278`, and Swagger UI opens at `/swagger`.

### Run with the frontend

1. Clone and install the client:
   ```bash
   git clone https://github.com/ysantonyance/ipay_client
   cd ipay_client
   npm install
   ```
2. Create `.env.local` in the client and point it at this project's `IPAY.WebApi` folder:
   ```
   IPAY_BACKEND_PATH=C:\path\to\IPAY_backend\src\IPAY.WebApi
   ```
3. Run `npm run dev`. Vite starts the backend for you and proxies `/api` to `https://localhost:7137`. Set `START_BACKEND=false` to start the backend yourself.

For a deployed frontend, set `VITE_API_URL` (for example `https://your-backend.example.com/api`) at build time.

### Docker

```bash
docker build -t ipay-backend .
docker run -p 10000:10000 ipay-backend
```

The container listens on port `10000`. Provide the configuration above through environment variables and mount or inject the Firebase service-account key.

### Tests

```bash
dotnet test
```

## Project documentation

- **Requirements:** the [SRS](https://github.com/ysantonyance/IPAY_SRS) lists functional and non-functional requirements, use cases, risks and success metrics
- **API:** [Scalar registry](https://registry.scalar.com/@default-team-wbd0w/apis/fat-api@v1#tag/admin)
- **Code:** [Doxygen site](https://ipay-doxygen-documentation.onrender.com/)
- **User guide:** `IPAY_User_Guide.html` in the repository root

## Team

Built by the IPAY team as a course project for ASP.NET Core.
