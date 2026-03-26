# Working Style & Agent Preferences

## How We Work Together (Akshay + Vidur)

### General Approach
- **End-to-end by default** — implement + tests + verification without asking permission each time
- **Proceed with implementation**, not just suggestions — when a task is clear, build it
- **Infer intent** when unclear — pick the most useful approach and continue
- **Build incrementally** — complete one feature fully before moving to the next
- **Verify continuously** — `dotnet build` after backend changes, verify static files serve correctly

### Session Pattern
1. Review what exists (read files, check structure)
2. Plan with todo list (manage_todo_list tool)
3. Implement sequentially — one feature at a time
4. Build & verify after each change
5. Update memory notes at session end

### Code Style
- **Backend**: Clean Architecture strict. No logic in controllers. Result<T> pattern. FluentValidation for all commands.
- **Frontend**: Plain vanilla JS (ES2022 modules). No frameworks. Bootstrap 5.3 for UI. Hash-based SPA routing.
- **Tests**: xUnit + FluentAssertions + Moq (when added)
- **Comments**: Only where logic isn't self-evident. No unnecessary docstrings.
- **Error handling**: At system boundaries only. Trust internal code.

### Communication Preferences
- **Concise** — short updates, no unnecessary framing
- **Direct** — skip "I'll now..." and just do it
- **Progress visibility** — use todo lists for multi-step tasks
- **Marathi context** — this is a Maharashtra government system; bilingual labels are standard

### Tools & Commands Used Frequently
```
dotnet build                                    # Verify compilation
dotnet run --project <path> --urls http://localhost:5108  # Start server
curl.exe http://localhost:5108/js/app.js        # Verify static files
curl.exe --noproxy * -X POST <endpoint>         # Test API endpoints
Get-Process -Id (Get-NetTCPConnection -Local 5108).OwningProcess | Stop-Process  # Kill port
```

### Common Pitfalls Learned
- MediatR 12 uses `next()` not `next(cancellationToken)` — causes build errors if wrong
- Windows PowerShell Invoke-WebRequest fails for local HTTPS — use `curl.exe` instead
- Locked DLL `MSB3021` errors → stop running process or build to alternate output
- `api.upload()` must exist in api.js for file upload features to work
- Always check `formatCurrency` exists in utils.js before using in proposal detail
