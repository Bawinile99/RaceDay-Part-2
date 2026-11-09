
# 🏃 RaceDay — PROG6212 Programming 2B PoE

A full-stack event management platform for the South African running, walking and cycling community. 🇿🇦

**Repository:** https://github.com/Bawinile99/RaceDay-Part-2
**Module:** PROG6212 — Programming 2B
**Author:** Bawinile Mahlangu

---

## 📖 Overview

**RaceDay** is a two-tier web application that powers event management for South Africa's road running, walking, and cycling community. From iconic events like the Comrades Marathon and Cape Town Cycle Tour to weekly parkruns, RaceDay gives **Organisers** the tools to create and manage events, and gives **Participants** the ability to browse, enrol, and track their performance across events.

This repository contains the complete **Part 2 + Part 3** submission for the PoE:

- **Part 2 — RESTful Web API** built with ASP.NET Core (.NET 10), Entity Framework Core, SQLite, session-based authentication, role-based access control, Azure Blob Storage integration, and automated CI/CD.
- **Part 3 — ASP.NET Core MVC frontend** that consumes the Part 2 API, provides role-specific interfaces for Organisers and Participants, uploads images via the API to Azure Blob Storage, and is fully containerised with Docker.

---

## 🎯 Architecture at a Glance

```
┌──────────────────────┐        HTTP        ┌──────────────────────┐
│  RaceDay.MVC         │  ───────────────▶  │  RaceDay.API         │
│  (ASP.NET Core MVC)  │  session cookie    │  (ASP.NET Core Web)  │
│  Containerised       │                    │  EF Core + SQLite    │
└──────────────────────┘                    └──────────┬───────────┘
                                                       │
                                                       ▼
                                            ┌──────────────────────┐
                                            │  Azure Blob Storage  │
                                            │  (images)            │
                                            └──────────────────────┘
```

The MVC layer **never** touches the database or Azure directly. Every data operation goes through the API. This mirrors real-world sports-technology architecture.

---

## 👥 User Roles

| Role | Capabilities |
|------|-------------|
| 🎯 **Organiser** | Register · Create/edit/delete events · Manage age/distance categories · Upload event banners · View enrolments for own events · Capture finish times and positions |
| 🏅 **Participant** | Register · Browse events · Enrol in a category · View personal enrolments · View personal race results · Update profile and profile picture |

**Access control** is enforced at **two levels**:

1. **API level** — the custom `[RequireRole]` attribute returns `401 Unauthorized` when not signed in, or `403 Forbidden` when the wrong role tries a protected endpoint.
2. **MVC level** — controllers check the session role before rendering, so participants cannot reach the Organiser Dashboard and organisers cannot reach the participant Enrol/Results pages.

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| API framework | ASP.NET Core Web API — **.NET 10** |
| MVC framework | ASP.NET Core MVC — **.NET 10** |
| ORM | **Entity Framework Core 10** (Code-First) |
| Database | **SQLite** (file-based, zero-config) |
| Auth | **Session-based** with server-side session + HttpOnly cookie |
| Password hashing | **BCrypt.Net-Next** |
| Cloud storage | **Azure Blob Storage** |
| API docs | **Swagger (Swashbuckle)** |
| Frontend styling | **Bootstrap 5** |
| Containerisation | **Docker** |
| Testing | **xUnit** + `Microsoft.AspNetCore.Mvc.Testing` (in-memory SQLite) |
| CI/CD | **GitHub Actions** |

---

## 📁 Project Structure

```
RaceDay-Part-2/
├── .github/
│   └── workflows/
│       └── ci.yml                       ← CI: builds API + MVC + runs tests
├── RaceDay.API/                         ← Part 2 — RESTful Web API
│   ├── Attributes/
│   │   └── RequireRoleAttribute.cs      ← role-authorisation filter
│   ├── Controllers/                     ← 6 controllers, 19 endpoints
│   ├── Data/
│   │   └── RaceDayDbContext.cs
│   ├── DTOs/                            ← request/response record types
│   ├── Migrations/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Event.cs
│   │   ├── Category.cs
│   │   ├── Enrolment.cs
│   │   └── Result.cs
│   ├── Services/
│   │   ├── IRaceDayPasswordHasher.cs
│   │   ├── BcryptPasswordHasher.cs
│   │   ├── IAzureBlobService.cs
│   │   ├── AzureBlobService.cs
│   │   └── SessionExtensions.cs
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.MVC/                         ← Part 3 — MVC frontend
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── HomeController.cs
│   │   ├── EventsController.cs
│   │   ├── OrganiserController.cs
│   │   ├── EnrolmentsController.cs
│   │   ├── ResultsController.cs
│   │   └── ProfileController.cs
│   ├── Services/
│   │   └── RaceDayApiService.cs         ← typed HttpClient, forwards session cookie
│   ├── ViewModels/
│   ├── Views/
│   │   ├── Account/
│   │   ├── Events/
│   │   ├── Enrolments/
│   │   ├── Home/
│   │   ├── Organiser/
│   │   ├── Profile/
│   │   ├── Results/
│   │   └── Shared/
│   ├── wwwroot/
│   ├── Dockerfile                       ← multi-stage build
│   ├── .dockerignore
│   └── Program.cs
├── RaceDay.Tests/                       ← xUnit integration tests
├── .gitignore
├── README.md
└── RaceDay.slnx
```

---

## 🚀 Setup & Run

### ✅ Prerequisites

- **.NET 10 SDK** — [download](https://dotnet.microsoft.com/download)
- **Git** — [download](https://git-scm.com/download)
- **Docker Desktop** — [download](https://www.docker.com/products/docker-desktop/)
- **EF Core CLI** (optional) — `dotnet tool install --global dotnet-ef`

### 📥 Clone

```bash
git clone https://github.com/Bawinile99/RaceDay-Part-2.git
cd RaceDay-Part-2
```

### 🔐 Configure Azure Blob Storage (local dev only)

Create `RaceDay.API/appsettings.Development.json` (this file is **gitignored**):

```json
{
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "raceday-images"
  }
}
```

In Azure Portal → Storage Account → **Containers** → create a container named `raceday-images` with public access level **Blob**.

### 📦 Restore

```bash
dotnet restore
```

### ▶️ Run the API

```bash
dotnet run --project RaceDay.API
```

Terminal prints:

```
Now listening on: http://localhost:5286
```

Swagger: **`http://localhost:5286/swagger`**

### ▶️ Run the MVC app

In **a second terminal**:

```bash
dotnet run --project RaceDay.MVC --urls "http://localhost:5500"
```

Terminal prints:

```
Now listening on: http://localhost:5500
```

Open **`http://localhost:5500`**.

> ⚠️ Both terminals must run simultaneously. The MVC app calls the API on every page.

---

## 🐳 Run with Docker

### Build the image

```bash
docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .
```

### Run the container

The MVC container must reach the host API. Use `host.docker.internal`:

```bash
docker run --rm -d --name raceday-test -p 8080:8080 \
  -e "ApiBaseUrl=http://host.docker.internal:5286/" \
  --add-host=host.docker.internal:host-gateway \
  raceday-mvc
```

Open **`http://localhost:8080`**.

**Stop the container:**

```bash
docker stop raceday-test
```

---

## 🧪 Tests

```bash
dotnet test
```

Expected:

```
Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10
```

| Test class | Tests | Purpose |
|------------|-------|---------|
| `AuthTests` | 4 | Register success, duplicate email conflict, valid login, invalid login |
| `EventsTests` | 3 | Organiser creates event, Participant forbidden, public listing |
| `EnrolmentsTests` | 1 | Participant enrols with a valid category |
| `RoleAccessTests` | 2 | Unauthenticated requests to `/api/profile` and `/api/enrolments` return 401 |

---

## 🔌 API Endpoint Reference

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| `POST` | `/api/auth/register` | None | Register a new user |
| `POST` | `/api/auth/login` | None | Login and start session |
| `POST` | `/api/auth/logout` | Any | Clear session |
| `GET` | `/api/profile` | Any | Current user profile |
| `PUT` | `/api/profile` | Any | Update current user |
| `POST` | `/api/profile/picture` | Any | Upload profile picture |
| `GET` | `/api/events` | None | List events (public) |
| `GET` | `/api/events/{id}` | None | Get single event |
| `GET` | `/api/events/my-events` | Organiser | Own events |
| `POST` | `/api/events` | Organiser | Create event |
| `PUT` | `/api/events/{id}` | Organiser | Update own event |
| `DELETE` | `/api/events/{id}` | Organiser | Delete own event |
| `POST` | `/api/events/{id}/banner` | Organiser | Upload event banner |
| `GET` | `/api/events/{eventId}/categories` | None | List categories |
| `POST` | `/api/events/{eventId}/categories` | Organiser | Create category |
| `PUT` | `/api/events/{eventId}/categories/{id}` | Organiser | Update category |
| `DELETE` | `/api/events/{eventId}/categories/{id}` | Organiser | Delete category |
| `GET` | `/api/enrolments` | Any | Own / own-events enrolments |
| `POST` | `/api/enrolments` | Participant | Enrol in event |
| `DELETE` | `/api/enrolments/{id}` | Participant | Cancel enrolment |
| `GET` | `/api/results` | Any | Own / own-events results |
| `POST` | `/api/results` | Organiser | Capture result |

---

## 🔄 CI/CD Pipeline

The repository runs a GitHub Actions workflow on every push and pull request (`.github/workflows/ci.yml`):

1. **Checkout** the repository
2. **Setup .NET 10**
3. **Restore** NuGet packages
4. **Build** the full solution (`RaceDay.slnx`) in Release
5. **Run** all xUnit tests

Live status: **https://github.com/Bawinile99/RaceDay-Part-2/actions**

---

## 📸 Screenshots & Walkthrough

### 🖥️ Build & Test Output

The full solution builds with **zero warnings** and all 10 xUnit integration tests pass — evidence that the API's authentication, role-based access, and enrolment logic work as specified.

![Build and test output](https://github.com/user-attachments/assets/ca1d689a-a157-42b9-9d50-79559e808771)

---

### ✅ Unit Tests Passing (10/10)

`dotnet test` executes the in-memory SQLite integration test suite. Each test spins up the API via `WebApplicationFactory<Program>`, seeds data, exercises an endpoint, and asserts the response — covering 401, 403, and 200 paths.

![Tests passing](https://github.com/user-attachments/assets/33b2aa62-2e3a-479c-ac3e-032f0c13c2c7)

---

### 🧭 API Response Sample

A live response from `POST /api/events` returning `201 Created` with the new event ID. Session cookie authentication and `[RequireRole("Organiser")]` are both enforced on this endpoint.

![API response sample](https://github.com/user-attachments/assets/b1a9773b-d701-45da-82e6-b5c66cb0609a)

---

### 📘 Swagger UI — All Endpoints

The auto-generated Swagger UI lists every controller and route. The `banner` and `picture` endpoints accept `multipart/form-data` and stream files to Azure Blob Storage via the `IAzureBlobService`.

![Swagger UI](https://github.com/user-attachments/assets/7db4d0fa-378a-4d8b-8319-4ef9e6b89d05)

---

### 🧪 Tests Passing in CLI

Console output of `dotnet test` showing individual test results and the final `Passed! - Failed: 0, Passed: 10` summary — used as proof of test coverage for the rubric.

![Tests passing in CLI](https://github.com/user-attachments/assets/a4cb6285-5ebe-498e-ae4f-0f9a84110d8c)

---

### 🏠 Home Page — Public Event Listing

The MVC home page renders event cards for all upcoming events, each with a banner image (from Azure Blob Storage), event type badge, date, location, distance, and enrolment count. Guests see **Login / Register**; authenticated users see role-specific navigation.

![Home page](https://github.com/user-attachments/assets/d66dcad1-2f8a-4f73-9447-6e297e1d4232)

---

### 🎯 Organiser Dashboard

After login as an Organiser, the dashboard shows three live stat cards — **Total Events**, **Total Enrolments**, and **Upcoming** — aggregated from `GET /api/events/my-events`. Below the stats, event cards link to edit, categories, and enrolments for each event.

![Organiser dashboard](https://github.com/user-attachments/assets/a2ee7fbd-5879-412b-b6ad-3bd1b2093b8b)

---

### 📝 My Events — Event CRUD Table

The My Events view lists the organiser's events with a banner thumbnail, name, date, location, type, and enrolment count. Each row exposes **Edit**, **Categories**, **Enrolments**, and **Delete** actions — all guarded by session-role checks in the MVC controller and `[RequireRole("Organiser")]` on the API.

![My Events](https://github.com/user-attachments/assets/951ee1c7-4432-4bca-82fb-a24eefbebdc0)

---

### ✏️ Create / Edit Event with Banner Upload

The create-event form posts JSON to `POST /api/events`, then immediately calls `POST /api/events/{id}/banner` with the chosen file. The API streams the file to Azure Blob Storage and persists the returned URL on the event record.

![Create event form](https://github.com/user-attachments/assets/12a762cd-1765-458c-922c-723421f454b0)

---

### 🗂️ Categories Management

Organisers add age/distance categories (e.g. `Open`, `Veterans 40+`) to each event. Participants later choose one of these categories when enrolling.

![Categories management](https://github.com/user-attachments/assets/1373f545-7b58-4a5c-8f19-f83b65ddc2b5)

---

### 🏅 Participant — Event Details & Enrol

Participants browse events, click **View details**, and enrol by picking a category from the dropdown. Clicking **Enrol now** posts to `POST /api/enrolments`; the status returns as **Confirmed**.

![Event details and Enrol form](https://github.com/user-attachments/assets/130a6c5f-ae5b-418e-a29a-f4419615656d)

---

### 📋 Participant — My Enrolments

The participant's enrolment history is rendered as cards, each showing event name, category, enrolment date, and a colour-coded status badge (green **Confirmed**, amber **Pending**, grey **Cancelled**).

![My Enrolments](https://github.com/user-attachments/assets/7e668f53-ea3a-4eab-8004-de09b46a1797)

---

### 🏁 Participant — My Results

After the organiser captures results, participants see their personal race history: event, category, finish time (formatted as `hh:mm:ss`), and finishing position (e.g. `#12`).

![My Results](https://github.com/user-attachments/assets/6fcf2620-947a-4bb5-bb9a-10ad22284665)

---

### ⏱️ Organiser — Capture Results

The organiser's Results page lists all enrolments that don't yet have a result. The organiser picks an enrolment, enters hours/minutes/seconds and finishing position, and clicks **Save result**. The API enforces that the organiser owns the event.

![Results capture](https://github.com/user-attachments/assets/e7860ec9-ddf3-4f46-a762-92df7cbc3412)

---

### 👤 Profile — Edit & Picture Upload

Both roles can update their name, phone, and date of birth, and upload a profile picture. The picture is sent as `multipart/form-data` to `POST /api/profile/picture`; the API uploads to Azure Blob Storage and returns the public URL, which the page then displays as a round avatar.

![Profile page](https://github.com/user-attachments/assets/38b3ee87-0863-4e34-82f6-a50504eb8577)

---

### 🐳 Docker — Containerised MVC App

The MVC project builds and runs inside a Docker container. The container reaches the host API via `host.docker.internal` (injected through the `ApiBaseUrl` environment variable). The login flow — and session cookie round-trips — works identically to the local run.

![Docker running](https://github.com/user-attachments/assets/dc1db339-7219-4b9b-86ae-3959dc4cd16f)

---

### 🚦 CI — Green Build on GitHub Actions

Every push triggers the GitHub Actions workflow, which builds both projects and runs the test suite. A green check on the latest commit is the submission evidence for the CI/CD rubric.

![CI green build](https://github.com/user-attachments/assets/20c90328-1747-4f35-a596-e810ed753036)

---

## 🎥 Video Presentation

📺 **Unlisted YouTube link:** **[ADD YOUR YOUTUBE LINK HERE]**

The video walkthrough covers:

- 🗂️ **Architecture** — how MVC consumes the API; why the MVC layer has no DB access
- 🔐 **Authentication** — session cookie captured on login, forwarded to every API call via a shared `CookieContainer`
- 🎯 **Organiser flow** — create event with banner, add categories, view enrolments, capture a result
- 🏅 **Participant flow** — browse, enrol in a category, view personal results
- ☁️ **Azure Blob Storage** — image upload from the browser → MVC → API → Azure
- 🐳 **Docker** — building and running the container
- 🧪 **Tests** — running `dotnet test` live

---

## 📝 AI Usage Disclosure

In accordance with the IIE Assessment Strategy, the following AI tools were used during this project:

- **Claude (Anthropic)** — used for code review, debugging assistance, architecture discussion, and documentation drafting.

All code was reviewed, understood, and validated by the student. Architectural decisions, business logic, and testing strategy reflect the student's own synthesis and analysis.

---

## 👨‍💻 Author

**Bawinile Mahlangu**
Student, IIE Programming 2B — **PROG6212**
GitHub: [@Bawinile99](https://github.com/Bawinile99)

---

## 📄 License

Submitted as academic work for **PROG6212 — Programming 2B** at The Independent Institute of Education (IIE). Not licensed for redistribution.

---

## 🔗 Quick Links

- 🌐 **Repository:** https://github.com/Bawinile99/RaceDay-Part-2
- 📊 **CI Actions:** https://github.com/Bawinile99/RaceDay-Part-2/actions
- 📘 **Swagger UI:** `http://localhost:5286/swagger` (after running locally)
- 🖥️ **MVC App:** `http://localhost:5500` (local) or `http://localhost:8080` (Docker)
- 🎥 **Video:** *[to be added]*
```

---

