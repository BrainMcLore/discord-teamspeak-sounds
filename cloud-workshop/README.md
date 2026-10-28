# SoundSpace cloud workshop

This is a real multi-user backend, not the old static GitHub catalog. It supports registration, sign-in, authenticated pack publication, public browsing, title/author search, category filters, newest/popular sorting, pagination and verified downloads. The desktop client connects to its HTTPS origin under Settings.

## Free hosting

Cloudflare Workers Free + D1 can host a small workshop. No R2 subscription is required: pack archives are stored as small BLOB chunks in D1. The application caps a pack at 8 MB, an account at 64 MB and the whole workshop at 512 MB by default. Workers/D1 also have their own free-tier limits; reaching them stops requests rather than promising unlimited storage or traffic. See official pricing before deployment:

- https://developers.cloudflare.com/workers/platform/pricing/
- https://developers.cloudflare.com/d1/reference/faq/

The owner's live endpoint is configured in `launcher/service.json`. End users do not need a Cloudflare account. Self-hosting requires your own account and database. Nothing in this folder deploys automatically.

## Self-hosting

```text
npx wrangler login
npx wrangler d1 create soundspace-workshop
```

Copy the returned database ID into `wrangler.jsonc`, then:

```text
npx wrangler d1 migrations apply soundspace-workshop --remote
npx wrangler deploy
```

Enter the resulting `https://…workers.dev` address in the launcher's Settings. Every user must connect to the same address. Use a free Cloudflare plan; do not enable paid upgrades or R2 billing for this prototype.

## Boundaries

GitHub device authorization uses the owner's OAuth client ID and checks verified email. Email/password authentication uses Firebase with project/account/token checks and requires email confirmation. Firebase sends verification and password-reset messages. Configure `GITHUB_CLIENT_ID`, `FIREBASE_PROJECT_ID` and `FIREBASE_API_KEY` for your own deployment. These are public client settings; never embed private keys or OAuth client secrets in the launcher.

Legacy username accounts retain salted PBKDF2-SHA256 login so existing owners can access their publications. External identities are not merged with legacy accounts by matching names or emails. Only hashed, expiring SoundSpace session tokens are stored server-side; desktop sessions use Windows DPAPI. Pack deletion requires authenticated ownership. Uploads are audio-only, validated and quota-limited. Moderation/reporting and account-deletion UI are not implemented yet.
