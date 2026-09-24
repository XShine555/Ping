# Ping

A Discord style chat app. Servers with text and voice channels, direct messages, a friends system, presence, and voice/video calls, all wrapped in a small web client.

## What it does

- **Servers and channels**: create a server, add text or voice channels inside it, reorder them.
- **Direct messages**: one on one chats outside of any server, created on demand when you message someone for the first time.
- **Friends**: send requests, accept or reject them, remove a friend, block someone.
- **Presence**: see who's online in real time across the app.
- **Voice and video calls**: ring a friend or jump into a voice channel, with LiveKit handling the actual media.
- **Attachments**: images, audio and plain files attached to messages, uploaded straight to storage.
- **Realtime everything**: new messages, edits, deletes, friend updates and presence changes all arrive over a single SignalR connection, no polling.
- Login through OIDC, sharing the same identity provider as Musify.

## How it's built

**Backend**: .NET 10, one API host on top of a Domain / Application / Infrastructure split.
- Mediator handles CQRS style commands and queries, with `ErrorOr` for typed results instead of throwing on expected failures.
- PostgreSQL through EF Core for everything: users, servers, channels, messages, friendships, attachments.
- SignalR for chat, presence and call signaling (ringing, accepting, hanging up).
- LiveKit's server SDK issues the access tokens clients use to join a voice/video room.
- FluentValidation on the request DTOs, wired in through a minimal API filter.
- Storage goes through an S3 compatible bucket, reusing the shared SeaweedFS instance from the Infrastructure stack instead of running a second one.
- Zitadel for OIDC/OAuth2 login, also shared through Infrastructure so a single account works across every project.

**Frontend**: React 19 with TypeScript, built with Vite.
- Zustand for the small bits of client state: current view, active call, toasts, presence.
- `react-oidc-context` and `oidc-client-ts` handle the login flow and token refresh.
- `@microsoft/signalr` for the hub connection, with a small `useHubEvent` hook to subscribe to server events from any component.
- `@livekit/components-react` plus `livekit-client` for the actual call UI and media handling.
- Plain CSS, no component library.

**Infrastructure**: Docker Compose for local development. LiveKit runs in a
container here; Postgres, Zitadel and the S3 storage are shared services
provided by the separate `Infrastructure` git repository, used by other
projects too, so this stack only adds what's specific to Ping.

## Project layout

```
backend/    .NET solution: the API host plus the Domain, Application and Infrastructure projects
frontend/   the React client
backend/deploy/   Compose files and env templates for local development
```

## Running it locally

The backend expects the shared `Infrastructure` stack to already be running
(see its own README for the exact command), since it reuses its
Postgres/Zitadel/SeaweedFS instead of standing up its own copies. With that
in place:

```bash
cp backend/deploy/.env.example backend/deploy/.env
# edit backend/deploy/.env if your Infrastructure checkout isn't where
# ZITADEL_PAT_DIR assumes, or its PING_* credentials differ from the defaults

docker compose -f backend/deploy/compose.yml -f backend/deploy/compose.dev.yml up -d
```

That starts LiveKit and the one-shot setup jobs (waiting for Postgres,
migrations, the Zitadel project for Ping, the storage bucket). From there,
run the API and the frontend from your IDE or with `dotnet run` / `npm run
dev` against them. See [backend/deploy/README.md](backend/deploy/README.md)
for the full details, including how to also build and run the API and
frontend inside Docker.
