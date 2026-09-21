# Ping deploy

Reuses the Zitadel and SeaweedFS instances already running in the separate
`Infrastructure` repository, instead of starting new ones. That stack must be
up first (see its own README for the exact command).

```bash
cp deploy/.env.example deploy/.env
# edit deploy/.env if your Infrastructure checkout isn't where
# ZITADEL_ADMIN_PAT_DIR assumes, or if its S3_ACCESS_KEY/SECRET_KEY differ
# from the defaults

docker compose -f deploy/compose.yml -f deploy/compose.dev.yml up -d
```

This starts LiveKit and the one-shot jobs (waiting for Postgres, EF Core
migrations, the Zitadel project for Ping, the `ping-storage` S3 bucket). The
usual dev loop runs the API and frontend from the IDE against these.

To also build and run the API/frontend in Docker instead:

```bash
docker compose -f deploy/compose.yml -f deploy/compose.dev.yml --profile apps up -d --build
```

By then `zitadel-init` has already written `AUTH_CLIENT_ID` / `WEB_CLIENT_ID`
into `deploy/.env`.

Ports (dev): API `5211`, web `3100`, LiveKit `7880` (signaling), `7881` (RTC
TCP) and `50000-50100/udp` (media). Postgres, Zitadel and the SeaweedFS S3 API
come from Infrastructure's own stack. See its README for that port table.
