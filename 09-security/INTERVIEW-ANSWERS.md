# 09 – Security: Model Interview Answers

> Reference answers to learn from. Read them, then answer in your own words.

---

## Authentication vs authorization 🔥

**Question:** "What's the difference between authentication and authorization?"

### Short answer

> "**Authentication** answers *who are you?*: validating credentials or a token
> and building the user's identity (`ClaimsPrincipal` with claims).
> **Authorization** answers *what are you allowed to do?*: checking that identity
> against rules (roles, policies) for a resource.
> Authentication comes first, which is why `UseAuthentication()` is registered before
> `UseAuthorization()`. Failing authentication returns **401** (despite the name
> 'Unauthorized', it means *not authenticated*); being authenticated but not
> allowed returns **403 Forbidden**."

| Case | Status |
|---|---|
| No token / invalid / expired token | 401 |
| Valid token, missing role or policy | 403 |

### ASP.NET Core

`[Authorize]`, `[Authorize(Roles = "Admin")]`, `[Authorize(Policy = "...")]`, `[AllowAnonymous]`.
Minimal APIs: `.RequireAuthorization()`. A global fallback policy can require auth everywhere by default.

### Comparisons

Laravel `auth` middleware + Gates/Policies; Spring Security `@PreAuthorize`.

---

## JWT 🔥

**Question:** "How does JWT authentication work? What are its weaknesses?"

### Short answer

> "After login, the server issues a JWT: `header.payload.signature`. The payload
> holds **claims** (user id, roles, `exp`), and the signature (HMAC or RSA) proves
> the server issued it and it hasn't been changed. The client sends it as
> `Authorization: Bearer <token>`. The API validates signature, issuer, audience
> and lifetime with `AddJwtBearer`, without any database lookup, so it's
> **stateless** and scales well.
> Two key points: a JWT is **signed, not encrypted**, so anyone can read the payload;
> never put secrets in it. And it **can't easily be revoked**: a stolen token works
> until it expires. So I use **short-lived access tokens** (minutes) plus
> **refresh tokens** that are stored server-side, can be revoked, and are rotated on use."

### Extra points

- Tampering with the payload breaks the signature → validation fails → 401.
- Use HTTPS always; prefer asymmetric keys (RS256) when several services validate tokens.
- In browsers, storing tokens in `localStorage` is exposed to XSS; HttpOnly cookies are safer
  (then protect against CSRF). For SPAs, the BFF (backend-for-frontend) pattern is recommended.
- Often you don't issue tokens yourself: use an identity provider (Entra ID, Auth0,
  Keycloak, Duende IdentityServer) with OpenID Connect.

### Comparisons

Laravel Sanctum stores tokens in the DB (revocable); a JWT isn't stored.
Spring: `oauth2ResourceServer().jwt()`.

---

## Roles, policies, resource-based authorization

**Question:** "How do you implement authorization rules in ASP.NET Core?"

### Short answer

> "For simple cases, **roles**: `[Authorize(Roles = "Admin")]`. For anything more,
> I prefer **policies**: named rules registered once with `AddAuthorization`, based on
> claims, roles or custom requirements with an `IAuthorizationHandler`, then used as
> `[Authorize(Policy = "Premium")]`. A rule changes in one place.
> When the decision depends on the **data itself**, like *users can only edit their own
> order*, I use **resource-based authorization**: load the resource, then call
> `IAuthorizationService.AuthorizeAsync(User, order, "CanEdit")` and return
> `Forbid()` (403) if it fails."

### Extra points

- Custom requirement: `class MinimumAgeRequirement : IAuthorizationRequirement` + `AuthorizationHandler<MinimumAgeRequirement>`.
- A fallback policy (`o.FallbackPolicy = ...RequireAuthenticatedUser()`) makes every endpoint secure by default.
- Return **404 instead of 403** when you don't want to reveal that a resource exists.

### Comparisons

Laravel Gates & Policies (`$this->authorize('update', $order)`) ≈ resource-based handlers.
Spring `@PreAuthorize` with SpEL expressions.

---

## CORS

**Question:** "What is CORS and how do you configure it?"

### Short answer

> "Browsers enforce the **same-origin policy**: JavaScript from one origin (scheme +
> host + port) can't read responses from another origin unless the server allows it.
> CORS is how the **server** opts in, with `Access-Control-Allow-*` response
> headers. For non-simple requests (PUT/DELETE, JSON content type, custom headers),
> the browser first sends an `OPTIONS` **preflight**.
> CORS is enforced **only by browsers**. Postman, curl and server-to-server calls
> ignore it, so it's **not** an API security mechanism; that's what authentication
> and authorization are for.
> In ASP.NET Core, I use `AddCors` with a named policy listing **specific origins**,
> and `app.UseCors()` after `UseRouting` and before `UseAuthorization`."

### Extra points

- `AllowAnyOrigin()` + `AllowCredentials()` is **not allowed** (the browser rejects it).
  With cookies you must list explicit origins.
- Avoid `AllowAnyOrigin()` in production; keep the origin list in configuration per environment.
- A CORS error is fixed on the **API** side, never in the frontend.

### Comparisons

Laravel `config/cors.php`; Spring `@CrossOrigin` / `CorsConfigurationSource`.
