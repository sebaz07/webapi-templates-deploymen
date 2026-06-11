# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 10 minimal Web API that aggregates several public APIs (PokeAPI, Rick and Morty, ip-api, icanhazdadjoke, adviceslip, numbersapi). It serves as the consumer/demo app for a DevSecOps Azure Pipelines template repository. User-facing strings (endpoint summaries, error messages) are in Spanish.

## Commands

```bash
dotnet build                 # build
dotnet run                   # run (http://localhost:5119, https://localhost:7254)
dotnet run --launch-profile http
```

There is no test project. In Development, OpenAPI is served at `/openapi/v1.json` and interactive docs (Scalar) at `/scalar/v1`. `webapi.http` contains sample requests for the REST Client extension.

## Architecture

- `Program.cs` — composition root. Registers one named `HttpClient` per upstream API (all with a 10s timeout) via `AddHttpClient("name", ...)`, then maps endpoint groups.
- `Endpoints/*.cs` — one static class per domain, each exposing a `Map<X>Endpoints(this WebApplication app)` extension method that creates a `MapGroup` with a tag. Adding a new endpoint area means: new static class in `webapi.Endpoints` namespace + register its named HttpClient in `Program.cs` (if it calls an upstream) + call its `Map...` method in `Program.cs`.
- Endpoint handlers take `IHttpClientFactory`, call `CreateClient("<named-client>")`, parse responses with `JsonDocument`, and return anonymous-object DTOs. Upstream 404s are translated to `Results.NotFound`.

## CI/CD (devsecops/)

- `devsecops/azure-pipelines.yaml` is a thin consumer of an **external** template repo (`All-Projects/devsecops-templates` in Azure DevOps org `Sebastian-Martinez-07`); the actual stages live there, not here. It currently extends the trunk-based template (`azure-pipelines-trunkbased.yaml@templates`); switching to legacy GitFlow means changing that single `- template:` line to `azure-pipelines.yaml@templates`.
- All pipeline configuration lives **only** in `devsecops/vars/vars-file-{dev,qa,prod}.yaml` (Azure App Service, ACR, SonarQube, Veracode settings). The files differ per environment in app/registry names, `envprefix`, `ASPNETCORE_ENVIRONMENT`, and prod additionally has `notification-webhook-url`.
- Pipeline paths assume this project sits at `webapi/` inside its repo (e.g. `projectPath: 'webapi/webapi.csproj'`, vars paths are repo-root-relative). The pipeline references `webapi/Dockerfile`, which does not exist in this directory yet.
- The vars key is `veracode-sandbox-name` (renamed from a `veracode-saandbox-name` typo); the templates expect the new key.
