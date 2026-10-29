# Workshop backend

Cloudflare Worker with D1 storage. Provides the shared pack catalog, GitHub/Firebase authentication, uploads, downloads and owner-only deletion. Desktop configuration lives in `launcher/service.json`; users do not need to configure hosting.

For a separate deployment, copy `wrangler.example.jsonc` to the ignored `wrangler.local.jsonc` and fill in your own D1 database and authentication provider settings. Apply the SQL migrations and deploy with Wrangler using `--config wrangler.local.jsonc`. Do not add private keys or OAuth client secrets to the repository.

Archives are limited to 8 MB, accounts to 64 MB, and total pack storage to 512 MB. Uploads require a verified identity; deletion requires ownership. Legacy accounts can sign in to manage existing packs. Moderation and account-deletion UI are not implemented.
