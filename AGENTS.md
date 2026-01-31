# AGENTS

## Conventions
- Keep clean-architecture dependency rules intact (Domain/Shared have no project references).
- Target `net8.0` for all projects; nullable and implicit usings stay enabled.
- Keep the API entry point minimal and free of business logic.
- Add new packages only when required for compilation or a clear feature need.
## Execution Plan
When asked to implement tasks, follow `.agent/PLANS.md` milestones in order.
For each milestone:
- Implement exactly what the milestone asks (no extra modules).
- Run: dotnet test
- Update the Progress checklist in `.agent/PLANS.md`
- Commit with message: "milestone X.Y - <short title>"

## Definition of Done
- `dotnet restore` completes.
- `dotnet build` succeeds with warnings treated as errors.
- `dotnet test` succeeds.
