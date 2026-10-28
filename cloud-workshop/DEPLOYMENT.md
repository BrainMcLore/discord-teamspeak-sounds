# Owner test deployment

Endpoint: https://soundspace-workshop.cloud-workshop.workers.dev

Deployed 2026-10-28 on Cloudflare Workers with D1. R2 and paid subscriptions were not enabled by this workflow. Application limits remain 8 MB per uploaded pack, 64 MB per account and 512 MB for the shared test catalog.

Live checks passed: HTTPS health endpoint, account registration, authenticated pack publication, author search, download and SHA256 equality. Temporary smoke-test accounts and packs were removed. No launcher GitHub release was created.

The connected launcher preview ships `launcher/service.json` with this origin. Cloudflare account credentials are not included. Users create their own SoundSpace accounts in the launcher.
