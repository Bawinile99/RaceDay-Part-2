# RaceDay — Programming 2B PoE (Part 2)

A full-stack event management system for the South African road running, walking, and cycling community.

**Repository:** https://github.com/Bawinile99/RaceDay

**Module:** PROG6212 — Programming 2B

---

## Roles

The system supports two distinct user roles:

| Role | Capabilities |
|------|-------------|
| **Organiser** | Create, edit, and delete events; manage event categories; capture participant results; view all enrolments for their own events |
| **Participant** | Register an account; browse upcoming events; enter an event by selecting a category; view their own enrolments; track their personal race history and results |

Role-based access is enforced at the API level via a custom `[RequireRole]` attribute. Organiser-only endpoints return `403 Forbidden` for Participants, and vice versa.

---

## Tech Stack (Part 2)

- **.NET 10** — ASP.NET Core Web API
- **Entity Framework Core 10** — Code-First approach with **SQLite**
- **Session-based authentication** — server-side session + HttpOnly cookie
- **BCrypt.Net-Next** — password hashing
- **Swagger (Swashbuckle)** — interactive API documentation and testing
- **xUnit** + **Microsoft.AspNetCore.Mvc.Testing** — integration tests against an in-memory SQLite database
- **GitHub Actions** — CI pipeline (build + test on every push)

---

## Project Structure

```
RaceDay/
├── .github/
│   └── workflows/
│       └── ci.yml
├── docs/
│   └── ci-screenshot.png
├── RaceDay.API/
│   ├── Attributes/
│   │   └── RequireRoleAttribute.cs
│   ├── Controllers/
│   ├── Data/
│   │   └── RaceDayDbContext.cs
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   │   ├── IRaceDayPasswordHasher.cs
│   │   ├── BcryptPasswordHasher.cs
│   │   └── SessionExtensions.cs
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.Tests/
│   ├── CustomWebApplicationFactory.cs
│   ├── TestHelpers.cs
│   ├── AuthTests.cs
│   ├── EventsTests.cs
│   ├── EnrolmentsTests.cs
│   └── RoleAccessTests.cs
├── .gitignore
├── README.md
└── RaceDay.slnx
```

---

## Setup & Run Instructions

### Prerequisites

- **.NET 10 SDK** — [download here](https://dotnet.microsoft.com/download)
- **Git**

### Clone & Restore

```bash
git clone https://github.com/Bawinile99/RaceDay.git
cd RaceDay
dotnet restore
```

### Apply Database Migrations

The API uses SQLite. The migration creates `RaceDay.API/raceday.db` automatically on first run, but you can create it explicitly:

```bash
dotnet ef database update --project RaceDay.API
```

If `dotnet ef` isn't installed:

```bash
dotnet tool install --global dotnet-ef
```

### Run the API

```bash
dotnet run --project RaceDay.API
```

The terminal prints something like:

```
Now listening on: http://localhost:5XXX
Now listening on: https://localhost:7XXX
```

Open the printed URL + `/swagger` in your browser:

```
http://localhost:5XXX/swagger
```

You'll see the Swagger UI with all 6 controllers and 19 endpoints, fully testable without any external tooling.

---

## Running Tests

```bash
dotnet test
```

Expected output:

```
Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10
```

The tests spin up the API in-memory using `WebApplicationFactory` against a shared in-memory SQLite connection. They cover:

- User registration (success + duplicate email conflict)
- User login (valid + invalid credentials)
- Role-based access (Organiser vs Participant vs Unauthenticated)
- Event creation (Organiser succeeds, Participant forbidden)
- Event listing (public access)
- Participant enrolment (happy path)

---

## API Endpoints

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| POST | `/api/auth/register` | None | Register a new user (Organiser or Participant) |
| POST | `/api/auth/login` | None | Login and start a session |
| POST | `/api/auth/logout` | Any | Clear session |
| GET | `/api/profile` | Any | Get own profile |
| PUT | `/api/profile` | Any | Update own profile |
| GET | `/api/events` | None | List all events (public) |
| GET | `/api/events/{id}` | None | Get one event (public) |
| POST | `/api/events` | Organiser | Create a new event |
| PUT | `/api/events/{id}` | Organiser | Update own event |
| DELETE | `/api/events/{id}` | Organiser | Delete own event |
| GET | `/api/events/{eventId}/categories` | None | List categories for an event (public) |
| POST | `/api/events/{eventId}/categories` | Organiser | Create category for own event |
| PUT | `/api/events/{eventId}/categories/{id}` | Organiser | Update category |
| DELETE | `/api/events/{eventId}/categories/{id}` | Organiser | Delete category |
| GET | `/api/enrolments` | Any | Own (Participant) or own-events (Organiser) enrolments |
| POST | `/api/enrolments` | Participant | Enrol in an event by choosing a category |
| DELETE | `/api/enrolments/{id}` | Participant | Cancel own enrolment |
| GET | `/api/results` | Any | Own (Participant) or own-events (Organiser) results |
| POST | `/api/results` | Organiser | Capture a finish time and finishing position |

### Authentication & Session Flow

1. **POST `/api/auth/register`** — creates the user with a BCrypt-hashed password
2. **POST `/api/auth/login`** — validates credentials and stores `UserId` and `Role` in the server-side session
3. All protected endpoints read `UserId` and `Role` from the session via `HttpContext.Session`
4. **POST `/api/auth/logout`** — clears the session

### Enum Handling

The API accepts enums as either **strings** or **integers** (thanks to `JsonStringEnumConverter`). Both of these work for registration:

```json
{ "role": "Organiser" }
{ "role": 0 }
```

Enum values:
- **Role:** `0` = Organiser, `1` = Participant
- **EventType:** `0` = Run, `1` = Walk, `2` = Cycle
- **EnrolmentStatus:** `0` = Confirmed, `1` = Pending, `2` = Cancelled

---

## CI/CD

The repository includes a GitHub Actions workflow at `.github/workflows/ci.yml` that runs on every push to `main`:

1. Checkout
2. Setup .NET 10
3. Restore
4. Build (Release)
5. Run all unit tests

**Green build evidence:**

![CI Green Build](docs/ci-screenshot.png)

Live status: https://github.com/Bawinile99/RaceDay/actions

---

## Video Presentation

Unlisted YouTube link: **[ADD YOUR YOUTUBE LINK HERE]**

The video covers:
- Solution structure and code walkthrough (`Models`, `DbContext`, `Controllers`, `Services`, `Attributes`, `DTOs`)
- Live demo of the API in Swagger — register, login, create event, enrol, verify role enforcement (`403`), verify unauthenticated rejection (`401`)
- Running the unit tests live with `dotnet test`

---

## Author

**Bawinile** — Student, IIE Programming 2B (PROG6212)
GitHub: [@Bawinile99](https://github.com/Bawinile99)
