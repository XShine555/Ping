# Ping — deploy

Reuses the Zitadel and SeaweedFS containers already running for Musify instead of starting new
ones. Musify's stack must be up first.

```bash
cp deploy/.env.example deploy/.env
# edit deploy/.env:
#   - ZITADEL_ADMIN_PAT_DIR must point at Musify's deploy/zitadel/.output folder
#   - S3_ACCESS_KEY / S3_SECRET_KEY must match the REAL values in Musify's own deploy/.env

docker compose -f deploy/compose.yml -f deploy/compose.dev.yml up -d
```

This starts Postgres, LiveKit, and the one-shot jobs (EF Core migrations, the Zitadel
project/apps for Ping, the `ping-storage` S3 bucket) — the usual dev loop runs the API and
frontend from the IDE against these.

To also build and run the API/frontend in Docker instead:

```bash
docker compose -f deploy/compose.yml -f deploy/compose.dev.yml --profile apps up -d --build
```

By then `zitadel-init` has already written `AUTH_CLIENT_ID` / `WEB_CLIENT_ID` into `deploy/.env`.

Ports (dev): API `5211`, web `3100`, Postgres `59010`, LiveKit `7880` (signaling) / `7881` (RTC
TCP) / `50000-50100/udp` (media). Zitadel (`8080`) and the SeaweedFS S3 API (`8333`) come from
Musify's own stack.
