# Debugging this project with GitHub Copilot

Copilot runs inside your IDE (Visual Studio 2022 17.8+ or VS Code with the GitHub Copilot
and Copilot Chat extensions), so this guide shows how to apply it to this codebase.

## Workflow
1. Set a breakpoint (F9) in `UserService.CreateAsync` or `ApiKeyAuthMiddleware.InvokeAsync`.
2. Press F5 (a VS Code launch config is included) and send a request from `UserManagementApi.http` or Swagger.
3. When it breaks or throws, open Copilot Chat and try:

| Situation | Prompt |
|---|---|
| Unexpected 500 | Select the code, then `/fix` - or paste the stack trace and ask "Why does this throw?" |
| Failing test | Right-click the test -> Copilot -> Debug Test, or `/fixTestFailure` |
| 401 when the key looks right | "@workspace Why would ApiKeyAuthMiddleware reject a request that sends X-API-KEY?" |
| Concurrency | "Is UserService thread-safe? Review the locking around _users." |
| Missing tests | Select `UserService`, then `/tests` |
| Understand code | `/explain` on any middleware class |

## Issues considered while writing this code
- Case-sensitive duplicate emails -> emails normalized with `ToLowerInvariant()` (test: `Duplicate_email_returns_409`).
- Check-then-insert race on email uniqueness -> protected by `_writeLock`.
- Timing attacks on API-key comparison -> `CryptographicOperations.FixedTimeEquals` on SHA-256 hashes.
- Middleware ordering -> exception handler first so it wraps everything below it.
- Paging abuse -> `pageSize` clamped to 1-100.

Always review Copilot's suggestions and run `dotnet test` before accepting a fix.
