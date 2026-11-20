# ADR-0003 — Authentication: mobile number + password, SMS OTP enabled

- **Status:** Accepted · 2026-11-20 (token transport accepted by the user, Q-18) · SMS provider open (Q-12)
- **Context:** D-09 sign-in with mobile + password; D-13 SMS OTP enabled; D-08 mobile app later.
- **Decision:**
  - ASP.NET Core Identity; `UserName` = normalized Iranian mobile (`09xxxxxxxxx`) via a `MobileNumber` value object.
  - Roles: `Admin`, `Content` (staff), `Member`. Web keeps two separate BFF cookies: `MoveWell.Staff` (path `/admin`) and `MoveWell.Member`. One staff person in MVP (D-14).
  - OTP: Identity phone token provider; sent through an `ISmsSender` port. Adapters: `Null` / `Dev` (logs the code) / real provider (Q-12). Rate-limited per number and per IP; code TTL 2 minutes.
  - OTP verifies the mobile at **registration** (account inactive until confirmed) and authorises **password reset**.
  - Lockout after failed attempts; password policy min 8.
  - **[Decided] D-16:** the Api issues JWT access (15 min) + rotating refresh tokens (30 days, stored hashed) from day one. Web acts as a BFF: it signs in through the Api and keeps the tokens inside its own encrypted HttpOnly cookie (Data Protection keys persisted to disk). The future mobile app uses the same endpoints and tokens — no second auth system. (Differs from yildiz, where Web hosts Identity cookies directly.)
- **Consequence:** an SMS provider account and an SMS budget are needed before launch (G3).
