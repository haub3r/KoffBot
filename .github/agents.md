# KoffBot - Copilot Instructions

## Project Overview

KoffBot is a Slack bot backend built on **Azure Functions v4** (dotnet-isolated) with **.NET 10.0**. It promotes Koff beer (Finnish brand by Sinebrychoff) through automated and interactive Slack messages.

## Tech Stack

- **Language:** C#
- **Framework:** .NET 10.0, Azure Functions v4 (dotnet-isolated)
- **Storage:** Azure Blob Storage (JSON blobs in dated folder structure)
- **Messaging:** Slack webhooks
- **Deployment:** Azure Bicep templates (dev + prod)
- **Secrets:** Azure Key Vault (OpenAI key, Slack signing secret, Slack webhook URL)

## Project Structure

- `KoffBot/` — Main Azure Functions project
  - `*Function.cs` — Azure Function entry points (HTTP or Timer triggered)
  - `Services/` — Shared services (authentication, blob storage, messaging, middleware)
  - `Models/` — Data models, log entities, Slack message wrappers
  - `Messages/` — Static message content (Finnish-language messages for Friday/Holiday)
  - `Deployment/` — Bicep infrastructure-as-code files
  - `Properties/` — Publish profiles and service dependencies

## Azure Functions

| Function | Trigger | Description |
|---|---|---|
| **Advertisement** | HTTP POST/GET | AI-generated Koff ads via OpenAI (currently disabled) |
| **Drunk** | HTTP POST | Activates "Drunk Mode" for 1 hour |
| **Echo** | HTTP POST | Admin-only echo command (restricted by Slack user ID) |
| **Friday** | Timer (hourly) | Sends Friday greetings with seasonal/Friday 13th variants |
| **Holiday** | Timer (daily midnight) | Fetches Finnish holidays from API, sends holiday messages |
| **Price** | HTTP POST/GET | Scrapes Alko.fi for Koff prices, tracks changes |
| **Stats** | HTTP GET | Public endpoint returning drunk/friday/toast counts |
| **Toast** | HTTP POST/GET | Sends "Koff!" to Slack (scrambled if in Drunk Mode) |
| **Untappd** | HTTP POST/GET | Sends Untappd rating promo message |

## Services

- **AuthenticationService** — HMAC-SHA256 Slack request signature verification
- **SlackAuthenticationMiddleware** — Pipeline middleware; skips auth for `KoffBotStats` and local environment
- **BlobStorageService** — CRUD for JSON blobs stored as `{Year}/{Month:D2}/{Day:D2}/{Id}.json`
- **MessagingService** — Posts typed messages to Slack webhook via `IHttpClientFactory`
- **ResponseEndpointService** — Resolves Slack webhook URL from environment variables

## Key Patterns

- All Slack message models have a single `Text` property serialized as `"text"`
- Logs use `DefaultLog` base class with `Id`, `Created`, `CreatedBy`, `Modified`, `ModifiedBy`
- `PriceLog` extends `DefaultLog` with an `Amount` string field
- Blob container names are constants in `StorageContainers` class
- Message pools are static arrays with random selection (Friday/Holiday messages are in Finnish)

## Configuration

- **Environment variables:** `SlackWebHook`, `SlackSigningSecret`, `OpenAiApiKey`, `BlobStorageConnectionString`
- **Timer schedules (prod):** Friday at `0 1 0 * * 5` (01:00 UTC Fridays), Holiday at `0 0 0 * * *` (midnight UTC daily)
- **Timezone:** FLE Standard Time (Finland)
- **Local dev:** Uses Azure Storage Emulator (`UseDevelopmentStorage=true`)

## External Integrations

- **Slack API** — Incoming webhooks and signature verification
- **OpenAI API** — Ad text generation (disabled)
- **Alko.fi** — Web scraping for beer prices
- **boffsaopendata.fi** — Finnish bank holidays API

## Coding Conventions

- Functions are single-class-per-file with constructor injection
- Services are registered as singletons in `Program.cs`
- Authentication is handled via middleware, not per-function
- All user-facing text is in Finnish
- Price is stored as string with dot decimal separator (e.g., `"1.55"`)

## Deployment

- Two Bicep files: `koffbot-dev.bicep` and `koffbot-prod.bicep`
- Resources: Storage Account, App Service Plan (Consumption Y1), Function App
- Managed identity for Azure service access
- Key Vault references for secrets
