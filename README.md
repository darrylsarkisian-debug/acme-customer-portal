# Acme Customer Portal

A small ASP.NET Core MVC (.NET 10) customer portal used to demonstrate **Black Duck Polaris** (SAST + SCA) in a GitHub and GitHub Actions workflow.

> **Demo application only.** Authentication is deliberately simplified (email only), and the demo branches introduce intentionally insecure code. Do not deploy this anywhere.

## What it does

Customers sign in, view their orders, and open order details. The demo adds order search, invoice download, and JSON export through pull requests so Polaris has realistic changes to scan.

## Layout

```
src/AcmePortal.Web/      ASP.NET Core MVC app (SQLite, seeded on first run)
tests/AcmePortal.Tests/  xUnit tests for the data layer
.github/workflows/       ci.yml (build and test), polaris.yml (Polaris SAST + SCA)
docs/demo-script.md      Setup runbook, 30 minute run of show, rehearsal checklist
```

## Run locally

```
dotnet test tests/AcmePortal.Tests
dotnet run --project src/AcmePortal.Web
```

Sign in with `dana@acme.example` or `marcus@acme.example`.

## CI

Two required checks protect `main`: **Build and test** and **Polaris SAST + SCA**. The Polaris workflow needs the repository secrets `POLARIS_SERVER_URL` and `POLARIS_ACCESS_TOKEN`.
