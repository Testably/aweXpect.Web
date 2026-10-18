# Agent Guide

aweXpect.Web is an extension package for [aweXpect](https://github.com/Testably/aweXpect) with expectations for
`HttpRequestMessage`, `HttpResponseMessage`, `Uri` and `IPAddress`.

## Design Goals

Correctness is a must. The other goals are strong defaults and win in this order when they conflict: stable and compatible, AOT and trimming friendly, a consistent and pleasant API, a fast happy path, extensible, errors at compile time, documented. [Docs/agents/design-goals.md](Docs/agents/design-goals.md) explains each goal, the code rules, the checks before a commit and the goals of this repository.

## Project Constraints

- Use the SDK pinned in `global.json` (10.0.300, latest patch). Package versions are managed centrally in
  `Directory.Packages.props`; shared library settings live in `Source/Directory.Build.props`, test settings in
  `Tests/Directory.Build.props`.
- `aweXpect.Web` targets `netstandard2.0;net8.0;net10.0` with `TreatWarningsAsErrors`. On `netstandard2.0` nullable
  is only `annotations`, and test projects also run on `net48`: records and `init` work (polyfilled), but index and
  range operators and list patterns do not compile there, as `System.Index` and `System.Range` are missing.
- The only runtime dependency is `aweXpect.Core` (plus `System.Text.Json` on `netstandard2.0`). Its version is the
  floor for every consumer, so raise it only when a change needs a newer Core API. Avoid new dependencies.
- Keep the library trimmable and Native AOT compatible (`IsTrimmable`/`IsAotCompatible` from `net8.0`). No
  reflection over user types and no trim suppressions; verify with the AOT smoke tests.
- Public API: document every public member (a missing XML doc, CS1591, is the only warning not treated as an
  error). A new expectation that fails on a `null` subject carries
  `[GuaranteesNotNull]`, validates arguments with `ThrowHelper` and follows the failure message conventions of
  aweXpect. Any public API change must update the API snapshots (see Validation).
- Do not add `#pragma` or other suppressions for analyzer or Sonar findings without asking first.

## Validation

- Run the tests relevant to your change before pushing. The unit test projects are `Tests/aweXpect.Web.Tests`
  (public behaviour), `Tests/aweXpect.Web.Internal.Tests` (internals, via `InternalsVisibleTo`) and
  `Tests/aweXpect.Web.Samples.Tests` (end-to-end against `Source/aweXpect.Web.Samples`, `net8.0`/`net10.0` only).
  CI runs all three on Windows, Linux and macOS; `net48` only runs on Windows.
- The Samples tests pin the failure message shown in `README.md`. Change both together.
- Public API: `Tests/aweXpect.Web.Api.Tests` (NUnit) compares the API of every target framework with the
  `*.awexpect-snapshot.txt` files. Accept an intentional change with the explicit `AcceptApiChanges` test or by running
  with `AWEXPECT_ACCEPT_SNAPSHOTS=all`, and commit the updated snapshots. Never commit `*.awexpect-snapshot.pending.*`.
- New or changed expectations get a check in `Tests/Aot/aweXpect.Web.Aot/Checks.cs`; `./build.ps1 AotSmokeTests` runs
  it under the JIT and published with Native AOT.
- Always name a pipeline target: `./build.ps1` without one runs the defaults `ApiChecks`, `Benchmarks` and
  `CodeAnalysis`.
- Sonar requires all issues solved and more than 90% coverage; mutation tests (Stryker) run on every pull request.

## Conventions

- Keep the UTF-8 BOM state of each file: most C# files have one, newer ones do not. PowerShell rewrites
  (`Set-Content`, `Out-File`) change it, so prefer targeted edits and check `git diff` for a stray U+FEFF.
- Git stores LF (`* text=auto`), `.editorconfig` asks for CRLF and tabs. Never mix line endings in a file, as raw
  string literals in tests compare multi-line failure messages; avoid `sed -i`.
- No em-dashes in `Docs/`, `README.md` or XML documentation.
- A new expectation also updates the overview table in `README.md` and the matching page in `Docs/pages/`.

## Git Workflow

- Work on a feature branch and open a pull request to `main`; it is squash-merged.
- Commit subjects follow [conventional commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `refactor:`,
  `test:`, `docs:`, `ci:`, `chore:`) and become the pull request title. The body explains why, one line per
  paragraph, without co-author or attribution trailers.
- Releases are tags `v<major>.<minor>.<patch>`; the `Build` workflow pushes the package to nuget.org.

## References

- `Docs/agents/design-goals.md`: the goals and code rules for every change, the checks before a commit and the goals of this repository.
- `Docs/agents/workflows.md`: build, test, benchmark, mutation and documentation commands, pipeline targets, CI.
- `Docs/agents/project-structure.md`: architecture, source locations and how to add an expectation.
