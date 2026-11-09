# RaceDay

**An event management platform for the South African running, walking and cycling community.**

[![CI](https://github.com/Bawinile99/RaceDay-Part-2/actions/workflows/ci.yml/badge.svg)](https://github.com/Bawinile99/RaceDay-Part-2/actions)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED)
![Tests](https://img.shields.io/badge/tests-10%20passing-brightgreen)

| | |
|---|---|
| **Module** | PROG6212 — Programming 2B (Portfolio of Evidence) |
| **Institution** | The Independent Institute of Education (IIE) |
| **Author** | Bawinile Mahlangu |
| **Repository** | [github.com/Bawinile99/RaceDay-Part-2](https://github.com/Bawinile99/RaceDay-Part-2) |

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture](#2-architecture)
3. [User Roles and Access Control](#3-user-roles-and-access-control)
4. [Technology Stack](#4-technology-stack)
5. [Project Structure](#5-project-structure)
6. [Getting Started](#6-getting-started)
7. [Running with Docker](#7-running-with-docker)
8. [Testing](#8-testing)
9. [API Reference](#9-api-reference)
10. [Continuous Integration](#10-continuous-integration)
11. [Application Walkthrough](#11-application-walkthrough)
12. [Video Presentation](#12-video-presentation)
13. [AI Usage Disclosure](#13-ai-usage-disclosure)
14. [Author and License](#14-author-and-license)

---

## 1. Overview

RaceDay is a two-tier web application that supports event management for South Africa's road running, walking and cycling community. From landmark events such as the Comrades Marathon and the Cape Town Cycle Tour to weekly parkruns, the platform gives **Organisers** the tools to create and manage events, and allows **Participants** to browse, enrol and track their performance.

This repository contains the complete **Part 2 and Part 3** submission:

- **Part 2 — RESTful Web API.** Built with ASP.NET Core (.NET 10), Entity Framework Core and SQLite, featuring session-based authentication, role-based access control, Azure Blob Storage integration and automated CI/CD.
- **Part 3 — ASP.NET Core MVC Frontend.** Consumes the Part 2 API, provides role-specific interfaces for Organisers and Participants, uploads images through the API to Azure Blob Storage, and is fully containerised with Docker.

---

## 2. Architecture

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

The MVC layer never accesses the database or Azure directly. Every data operation is routed through the API, mirroring the separation of concerns found in production sports-technology systems.

---

## 3. User Roles and Access Control

| Role | Capabilities |
|------|--------------|
| **Organiser** | Register; create, edit and delete events; manage age and distance categories; upload event banners; view enrolments for own events; capture finish times and positions. |
| **Participant** | Register; browse events; enrol in a category; view personal enrolments and race results; update profile and profile picture. |

Access control is enforced at two levels:

1. **API level** — the custom `[RequireRole]` attribute returns `401 Unauthorized` when the caller is not signed in, and `403 Forbidden` when the caller's role is not permitted.
2. **MVC level** — controllers verify the session role before rendering, so Participants cannot reach the Organiser Dashboard and Organisers cannot reach Participant enrolment or results pages.

---

## 4. Technology Stack

| Layer | Technology |
|-------|-----------|
| API framework | ASP.NET Core Web API (.NET 10) |
| MVC framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 (Code-First) |
| Database | SQLite |
| Authentication | Server-side session with HttpOnly cookie |
| Password hashing | BCrypt.Net-Next |
| Cloud storage | Azure Blob Storage |
| API documentation | Swagger (Swashbuckle) |
| Frontend styling | Bootstrap 5 |
| Containerisation | Docker |
| Testing | xUnit, `Microsoft.AspNetCore.Mvc.Testing` (in-memory SQLite) |
| CI/CD | GitHub Actions |

---

## 5. Project Structure

```
RaceDay-Part-2/
├── .github/
│   └── workflows/
│       └── ci.yml                       CI: builds API and MVC, runs tests
├── RaceDay.API/                         Part 2 — RESTful Web API
│   ├── Attributes/
│   │   └── RequireRoleAttribute.cs      Role-authorisation filter
│   ├── Controllers/                     6 controllers, 19 endpoints
│   ├── Data/
│   │   └── RaceDayDbContext.cs
│   ├── DTOs/                            Request and response record types
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
├── RaceDay.MVC/                         Part 3 — MVC frontend
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── HomeController.cs
│   │   ├── EventsController.cs
│   │   ├── OrganiserController.cs
│   │   ├── EnrolmentsController.cs
│   │   ├── ResultsController.cs
│   │   └── ProfileController.cs
│   ├── Services/
│   │   └── RaceDayApiService.cs         Typed HttpClient; forwards session cookie
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Dockerfile                       Multi-stage build
│   ├── .dockerignore
│   └── Program.cs
├── RaceDay.Tests/                       xUnit integration tests
├── .gitignore
├── README.md
└── RaceDay.slnx
```

---

## 6. Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- EF Core CLI *(optional)*: `dotnet tool install --global dotnet-ef`

### Clone the Repository

```bash
git clone https://github.com/Bawinile99/RaceDay-Part-2.git
cd RaceDay-Part-2
```

### Configure Azure Blob Storage (local development)

Create `RaceDay.API/appsettings.Development.json`. This file is excluded from version control.

```json
{
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "raceday-images"
  }
}
```

In the Azure Portal, open your Storage Account, select **Containers**, and create a container named `raceday-images` with the public access level set to **Blob**.

### Restore Dependencies

```bash
dotnet restore
```

### Run the API

```bash
dotnet run --project RaceDay.API
```

The API listens on `http://localhost:5286`. Swagger UI is available at `http://localhost:5286/swagger`.

### Run the MVC Application

In a second terminal:

```bash
dotnet run --project RaceDay.MVC --urls "http://localhost:5500"
```

Open `http://localhost:5500` in your browser.

> **Note:** Both processes must run simultaneously, as the MVC application calls the API on every page request.

---

## 7. Running with Docker

### Build the Image

```bash
docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .
```

### Run the Container

The container reaches the host-based API through `host.docker.internal`:

```bash
docker run --rm -d --name raceday-test -p 8080:8080 \
  -e "ApiBaseUrl=http://host.docker.internal:5286/" \
  --add-host=host.docker.internal:host-gateway \
  raceday-mvc
```

Open `http://localhost:8080`. To stop the container:

```bash
docker stop raceday-test
```

---

## 8. Testing

```bash
dotnet test
```

Expected result:

```
Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10
```

| Test Class | Tests | Purpose |
|------------|:-----:|---------|
| `AuthTests` | 4 | Successful registration, duplicate email conflict, valid login, invalid login |
| `EventsTests` | 3 | Organiser creates event, Participant forbidden, public listing |
| `EnrolmentsTests` | 1 | Participant enrols with a valid category |
| `RoleAccessTests` | 2 | Unauthenticated requests to `/api/profile` and `/api/enrolments` return 401 |

---

## 9. API Reference

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| `POST` | `/api/auth/register` | None | Register a new user |
| `POST` | `/api/auth/login` | None | Log in and start a session |
| `POST` | `/api/auth/logout` | Any | Clear the session |
| `GET` | `/api/profile` | Any | Retrieve the current user's profile |
| `PUT` | `/api/profile` | Any | Update the current user |
| `POST` | `/api/profile/picture` | Any | Upload a profile picture |
| `GET` | `/api/events` | None | List events (public) |
| `GET` | `/api/events/{id}` | None | Retrieve a single event |
| `GET` | `/api/events/my-events` | Organiser | List own events |
| `POST` | `/api/events` | Organiser | Create an event |
| `PUT` | `/api/events/{id}` | Organiser | Update own event |
| `DELETE` | `/api/events/{id}` | Organiser | Delete own event |
| `POST` | `/api/events/{id}/banner` | Organiser | Upload an event banner |
| `GET` | `/api/events/{eventId}/categories` | None | List categories |
| `POST` | `/api/events/{eventId}/categories` | Organiser | Create a category |
| `PUT` | `/api/events/{eventId}/categories/{id}` | Organiser | Update a category |
| `DELETE` | `/api/events/{eventId}/categories/{id}` | Organiser | Delete a category |
| `GET` | `/api/enrolments` | Any | List own or own-event enrolments |
| `POST` | `/api/enrolments` | Participant | Enrol in an event |
| `DELETE` | `/api/enrolments/{id}` | Participant | Cancel an enrolment |
| `GET` | `/api/results` | Any | List own or own-event results |
| `POST` | `/api/results` | Organiser | Capture a result |

---

## 10. Continuous Integration

A GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push and pull request:

1. Check out the repository
2. Set up .NET 10
3. Restore NuGet packages
4. Build the full solution (`RaceDay.slnx`) in Release configuration
5. Run all xUnit tests

Live status: [github.com/Bawinile99/RaceDay-Part-2/actions](https://github.com/Bawinile99/RaceDay-Part-2/actions)

---

## 11. Application Walkthrough

### Build and Test Evidence

The full solution builds with zero warnings and all 10 xUnit integration tests pass, confirming the API's authentication, role-based access and enrolment logic.

![Build and test output](https://github.com/user-attachments/assets/ca1d689a-a157-42b9-9d50-79559e808771)

Each test starts the API through `WebApplicationFactory<Program>`, seeds data, exercises an endpoint and asserts the response, covering the 200, 401 and 403 paths.

![Tests passing](https://github.com/user-attachments/assets/33b2aa62-2e3a-479c-ac3e-032f0c13c2c7)

![Tests passing in CLI](https://github.com/user-attachments/assets/a4cb6285-5ebe-498e-ae4f-0f9a84110d8c)

### API Layer

A live response from `POST /api/events` returning `201 Created`. Session-cookie authentication and `[RequireRole("Organiser")]` are both enforced on this endpoint.

![API response sample](https://github.com/user-attachments/assets/b1a9773b-d701-45da-82e6-b5c66cb0609a)

The auto-generated Swagger UI documents every controller and route. The `banner` and `picture` endpoints accept `multipart/form-data` and stream files to Azure Blob Storage through `IAzureBlobService`.

![Swagger UI](https://github.com/user-attachments/assets/7db4d0fa-378a-4d8b-8319-4ef9e6b89d05)

### Public Home Page

Event cards display a banner image (served from Azure Blob Storage), event type, date, location, distance and enrolment count. Guests see Login and Register options; authenticated users see role-specific navigation.

![Home page](https://github.com/user-attachments/assets/d66dcad1-2f8a-4f73-9447-6e297e1d4232)

### Organiser Experience

**Dashboard.** Three live statistics (Total Events, Total Enrolments and Upcoming) are aggregated from `GET /api/events/my-events`, with event cards linking to edit, category and enrolment views.

![Organiser dashboard](https://github.com/user-attachments/assets/a2ee7fbd-5879-412b-b6ad-3bd1b2093b8b)

**My Events.** A tabular view of the organiser's events with banner thumbnail, name, date, location, type and enrolment count. Each row exposes Edit, Categories, Enrolments and Delete actions, guarded by session-role checks in the MVC controller and `[RequireRole("Organiser")]` on the API.

![My Events](https://github.com/user-attachments/assets/951ee1c7-4432-4bca-82fb-a24eefbebdc0)

**Create and Edit Event.** The form posts JSON to `POST /api/events`, then calls `POST /api/events/{id}/banner` with the selected file. The API streams the file to Azure Blob Storage and stores the returned URL on the event record.

![Create event form](https://github.com/user-attachments/assets/12a762cd-1765-458c-922c-723421f454b0)

**Category Management.** Organisers define age and distance categories (for example, `Open` or `Veterans 40+`) which Participants select when enrolling.

![Categories management](https://github.com/user-attachments/assets/1373f545-7b58-4a5c-8f19-f83b65ddc2b5)

**Result Capture.** The Results page lists enrolments without a result. The organiser selects an enrolment, enters hours, minutes, seconds and finishing position, and saves. The API verifies that the organiser owns the event.

![Results capture](https://github.com/user-attachments/assets/e7860ec9-ddf3-4f46-a762-92df7cbc3412)

### Participant Experience

**Event Details and Enrolment.** Participants browse events, open the details page and enrol by selecting a category. Submitting posts to `POST /api/enrolments`, and the status is returned as Confirmed.

![Event details and enrol form](https://github.com/user-attachments/assets/130a6c5f-ae5b-418e-a29a-f4419615656d)

**My Enrolments.** Enrolment history is displayed as cards showing event name, category, enrolment date and a colour-coded status badge (green for Confirmed, amber for Pending, grey for Cancelled).

![My Enrolments](https://github.com/user-attachments/assets/7e668f53-ea3a-4eab-8004-de09b46a1797)

**My Results.** Once results are captured, Participants see their race history: event, category, finish time (formatted `hh:mm:ss`) and finishing position.

![My Results](https://github.com/user-attachments/assets/6fcf2620-947a-4bb5-bb9a-10ad22284665)

### Profile Management

Both roles can update their name, phone number and date of birth, and upload a profile picture. The image is sent as `multipart/form-data` to `POST /api/profile/picture`; the API uploads it to Azure Blob Storage and returns the public URL, displayed as a circular avatar.

![Profile page](https://github.com/user-attachments/assets/38b3ee87-0863-4e34-82f6-a50504eb8577)

### Containerisation

The MVC project builds and runs inside a Docker container. The container reaches the host API through `host.docker.internal`, supplied via the `ApiBaseUrl` environment variable. Login and session-cookie round-trips behave identically to a local run.

![Docker running](https://github.com/user-attachments/assets/dc1db339-7219-4b9b-86ae-3959dc4cd16f)

### Continuous Integration

Every push triggers the GitHub Actions workflow, which builds both projects and runs the test suite. A green check on the latest commit serves as submission evidence for the CI/CD criteria.

![CI green build](https://github.com/user-attachments/assets/20c90328-1747-4f35-a596-e810ed753036)

---

## 12. Video Presentation

**Unlisted YouTube link:** **[ADD YOUR YOUTUBE LINK HERE]**

The walkthrough covers:

- **Architecture** — how the MVC application consumes the API, and why the MVC layer has no database access
- **Authentication** — session cookie captured on login and forwarded to every API call via a shared `CookieContainer`
- **Organiser flow** — creating an event with a banner, adding categories, viewing enrolments and capturing a result
- **Participant flow** — browsing, enrolling in a category and viewing personal results
- **Azure Blob Storage** — image upload from browser to MVC to API to Azure
- **Docker** — building and running the container
- **Testing** — running `dotnet test` live

---

## 13. AI Usage Disclosure

In accordance with the IIE Assessment Strategy, the following AI tool was used during this project:

- **Claude (Anthropic)** — code review, debugging assistance, architecture discussion and documentation drafting.

All code was reviewed, understood and validated by the student. Architectural decisions, business logic and testing strategy reflect the student's own synthesis and analysis.

---

## 14. Author and License

**Bawinile Mahlangu**
Student, IIE Programming 2B (PROG6212)
GitHub: [@Bawinile99](https://github.com/Bawinile99)

This project was submitted as academic work for PROG6212 — Programming 2B at The Independent Institute of Education (IIE) and is not licensed for redistribution.

---

### Quick Links

| Resource | Location |
|----------|----------|
| Repository | https://github.com/Bawinile99/RaceDay-Part-2 |
| CI Actions | https://github.com/Bawinile99/RaceDay-Part-2/actions |
| Swagger UI | `http://localhost:5286/swagger` (when running locally) |
| MVC Application | `http://localhost:5500` (local) or `http://localhost:8080` (Docker) |
| Video | *To be added* |
