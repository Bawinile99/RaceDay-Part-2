# 🏃 RaceDay — Programming 2B PoE (Part 2)

A full-stack event management system for the South African road running, walking, and cycling community. 🇿🇦

**Repository:** https://github.com/Bawinile99/RaceDay-Part-2

**Module:** PROG6212 — Programming 2B

**Author:** Bawinile Mahlangu

---

## 📖 Overview

**RaceDay** is a RESTful Web API built with **ASP.NET Core (.NET 10)** that powers an event management platform for South Africa's road running, walking, and cycling community. From iconic events like the Comrades Marathon and Cape Town Cycle Tour to weekly parkruns, RaceDay gives **Organisers** the tools to create and manage events, and gives **Participants** the ability to browse, enter, and track their performance across events.

This repository contains **Part 2** of the three-part PoE: a fully working, tested, and container-ready API with **session-based authentication**, **role-based access control**, and **automated CI/CD**.

---

## 👥 User Roles

The system supports two distinct user roles, enforced at the API level via a custom `[RequireRole]` authorisation attribute:

| Role | Capabilities |
|------|-------------|
| 🎯 **Organiser** | Register an account · Create, edit, and delete events · Manage age/distance categories for each event · View all enrolments for events they own · Capture finish times and finishing positions after an event concludes |
| 🏅 **Participant** | Register an account · Browse and filter upcoming events · Enter an event by selecting a category · View personal enrolment history and status · Track personal race results (finish time, position) |

**Access control:**
- Unauthenticated requests to protected endpoints return **`401 Unauthorized`**
- Role-specific endpoints (e.g. `POST /api/events`) return **`403 Forbidden`** when accessed by the wrong role

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ⚙️ **.NET 10** — ASP.NET Core Web API |
| ORM | 🗄️ **Entity Framework Core 10** (Code-First) |
| Database | 💾 **SQLite** (file-based, zero-config) |
| Auth | 🔐 **Session-based** with server-side session + HttpOnly cookie |
| Password Hashing | 🔑 **BCrypt.Net-Next** |
| API Documentation | 📘 **Swagger (Swashbuckle)** |
| Testing | 🧪 **xUnit** + **Microsoft.AspNetCore.Mvc.Testing** (in-memory SQLite) |
| CI/CD | 🚀 **GitHub Actions** |

---

## 📁 Project Structure

```
RaceDay/
├── .github/
│   └── workflows/
│       └── ci.yml                    ← GitHub Actions CI pipeline
├── docs/
│   └── ci-screenshot.png             ← green CI build evidence
├── RaceDay.API/                      ← ASP.NET Core Web API project
│   ├── Attributes/
│   │   └── RequireRoleAttribute.cs   ← custom role-authorisation filter
│   ├── Controllers/                  ← 6 controllers, 19 endpoints
│   │   ├── AuthController.cs
│   │   ├── ProfileController.cs
│   │   ├── EventsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── EnrolmentsController.cs
│   │   └── ResultsController.cs
│   ├── Data/
│   │   └── RaceDayDbContext.cs       ← EF Core DbContext + relationships
│   ├── DTOs/                         ← request/response record types
│   ├── Migrations/                   ← EF Core migrations
│   ├── Models/                       ← domain entities
│   │   ├── User.cs
│   │   ├── Event.cs
│   │   ├── Category.cs
│   │   ├── Enrolment.cs
│   │   └── Result.cs
│   ├── Services/
│   │   ├── IRaceDayPasswordHasher.cs
│   │   ├── BcryptPasswordHasher.cs
│   │   └── SessionExtensions.cs
│   ├── Program.cs                    ← app entry point + DI + middleware
│   └── appsettings.json
├── RaceDay.Tests/                    ← xUnit integration tests (10 tests)
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

## 🚀 Setup & Run Instructions

### ✅ Prerequisites

- **.NET 10 SDK** — [download here](https://dotnet.microsoft.com/download)
- **Git** — [download here](https://git-scm.com/download)
- **EF Core CLI** (optional, for migrations) — install with `dotnet tool install --global dotnet-ef`

### 📥 Clone the Repository

```bash
git clone https://github.com/Bawinile99/RaceDay-Part-2.git
cd RaceDay-Part-2
```

### 📦 Restore Dependencies

```bash
dotnet restore
```

### 🗄️ Apply Database Migrations

The SQLite database (`raceday.db`) is created automatically on the first run, but you can also create it explicitly:

```bash
dotnet ef database update --project RaceDay.API
```

### ▶️ Run the API

```bash
dotnet run --project RaceDay.API
```

The terminal will print the listening URLs:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5XXX
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 🌐 Open Swagger

Open the printed HTTP URL + `/swagger` in your browser:

```
http://localhost:5XXX/swagger
```

You'll see the Swagger UI showing **all 6 controllers and 19 endpoints**, fully testable without any external tooling.

---

## 🧪 Running the Tests

```bash
dotnet test
```

Expected output:

```
Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10
```

The tests use `WebApplicationFactory<Program>` to spin up the API in-memory against a **shared in-memory SQLite connection**, ensuring fast, isolated, and deterministic integration tests.

### Test Coverage

| Test Class | Tests | Purpose |
|------------|-------|---------|
| `AuthTests` | 4 | Registration success, duplicate email conflict, valid login, invalid login |
| `EventsTests` | 3 | Organiser can create event, Participant is forbidden, public listing works |
| `EnrolmentsTests` | 1 | Participant can enrol in an event with a valid category |
| `RoleAccessTests` | 2 | Unauthenticated requests to `/api/profile` and `/api/enrolments` return 401 |

---

## 🔌 API Endpoint Reference

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| `POST` | `/api/auth/register` | None | Register a new user (Organiser or Participant) |
| `POST` | `/api/auth/login` | None | Login and start a session |
| `POST` | `/api/auth/logout` | Any | Clear the current session |
| `GET` | `/api/profile` | Any | Get the current user's profile |
| `PUT` | `/api/profile` | Any | Update the current user's profile |
| `GET` | `/api/events` | None | List all events (public) |
| `GET` | `/api/events/{id}` | None | Get a single event (public) |
| `POST` | `/api/events` | Organiser | Create a new event |
| `PUT` | `/api/events/{id}` | Organiser | Update an event owned by the caller |
| `DELETE` | `/api/events/{id}` | Organiser | Delete an event owned by the caller |
| `GET` | `/api/events/{eventId}/categories` | None | List categories for an event (public) |
| `POST` | `/api/events/{eventId}/categories` | Organiser | Create a category for own event |
| `PUT` | `/api/events/{eventId}/categories/{id}` | Organiser | Update a category |
| `DELETE` | `/api/events/{eventId}/categories/{id}` | Organiser | Delete a category |
| `GET` | `/api/enrolments` | Any | Own (Participant) or own-events (Organiser) enrolments |
| `POST` | `/api/enrolments` | Participant | Enrol in an event by choosing a category |
| `DELETE` | `/api/enrolments/{id}` | Participant | Cancel an enrolment |
| `GET` | `/api/results` | Any | Own (Participant) or own-events (Organiser) results |
| `POST` | `/api/results` | Organiser | Capture a finish time and finishing position |

### 🔐 Authentication Flow

1. **Register** — `POST /api/auth/register` with `{ firstName, lastName, email, password, role, ... }`
2. **Login** — `POST /api/auth/login` with `{ email, password }`
3. On success, the server **stores `UserId` and `Role` in the session** (server-side, `ISession`)
4. All subsequent requests automatically include the session cookie (`.RaceDay.Session`)
5. **Logout** — `POST /api/auth/logout` clears the session

### 🎛️ Enum Serialisation

The API uses `JsonStringEnumConverter`, so enums accept **both strings and integers**:

```json
{ "role": "Organiser" }
{ "role": 0 }
```

| Enum | Values |
|------|--------|
| `Role` | `0` = Organiser, `1` = Participant |
| `EventType` | `0` = Run, `1` = Walk, `2` = Cycle |
| `EnrolmentStatus` | `0` = Confirmed, `1` = Pending, `2` = Cancelled |

---

## 🔄 CI/CD Pipeline

The repository includes a GitHub Actions workflow at `.github/workflows/ci.yml`, triggered on every push and pull request to `main`:

1. 📥 **Checkout** repository
2. ⚙️ **Setup .NET 10**
3. 📦 **Restore** NuGet packages
4. 🔨 **Build** in Release configuration
5. 🧪 **Run** all unit tests

**Green build evidence:**

![CI Green Build](docs/ci-screenshot.png)

🔗 Live status: **https://github.com/Bawinile99/RaceDay-Part-2/actions**

---

## 📸 Screenshots

### 🖥️ Build & Test Output
<img width="993" height="474" alt="Build and test output" src="https://github.com/user-attachments/assets/ca1d689a-a157-42b9-9d50-79559e808771" />

### ✅ Unit Tests Passing (10/10)
<img width="853" height="512" alt="Tests passing" src="https://github.com/user-attachments/assets/33b2aa62-2e3a-479c-ac3e-032f0c13c2c7" />

### 🧭 API Response Sample
<img width="835" height="700" alt="API response sample" src="https://github.com/user-attachments/assets/b1a9773b-d701-45da-82e6-b5c66cb0609a" />

### 📘 Swagger UI — All Endpoints
<img width="995" height="1025" alt="Swagger UI" src="https://github.com/user-attachments/assets/7db4d0fa-378a-4d8b-8319-4ef9e6b89d05" />

### 🧪 Tests Passing in CLI
<img width="1191" height="785" alt="Tests passing in CLI" src="https://github.com/user-attachments/assets/a4cb6285-5ebe-498e-ae4f-0f9a84110d8c" />

---

## 🎥 Video Presentation

📺 **Unlisted YouTube link:** **[ADD YOUR YOUTUBE LINK HERE]**

The video covers:
- 🗂️ **Solution structure** — walkthrough of `Models`, `DbContext`, `Controllers`, `Services`, `Attributes`, `DTOs`
- 🔐 **Authentication** — live demo of register, login, session handling, and logout
- 🌐 **Live API demo** — create event, add category, enrol participant, verify role enforcement (`403`), verify unauthenticated rejection (`401`)
- 🧪 **Unit tests** — running `dotnet test` live with all 10 tests passing

---

## 📝 AI Usage Disclosure

In accordance with the IIE Assessment Strategy, the following AI tools were used during this project:

- **Claude (Anthropic)** — used for code review, debugging assistance, and documentation drafting.

All code was reviewed, understood, and validated by the student. Architectural decisions, business logic, and testing strategy reflect the student's own synthesis and analysis.

---

## 👨‍💻 Author

**Bawinile**  
Student, IIE Programming 2B — **PROG6212**  
GitHub: [@Bawinile99](https://github.com/Bawinile99)

---

## 📄 License

This project is submitted as academic work for **PROG6212 — Programming 2B** at The Independent Institute of Education (IIE). Not licensed for redistribution.

---

## 🔗 Quick Links

- 🌐 **Repository:** https://github.com/Bawinile99/RaceDay-Part-2
- 📊 **CI Actions:** https://github.com/Bawinile99/RaceDay-Part-2/actions
- 📘 **Swagger UI:** `http://localhost:5XXX/swagger` (after running locally)
- 🎥 **Video:** *[to be added]*
