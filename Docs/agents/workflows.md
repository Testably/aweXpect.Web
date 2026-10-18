# Development Commands

## Build and test

- Build everything from the repository root with `dotnet build aweXpect.Web.slnx`. The solution excludes
  `Pipeline/Build.csproj` from the build; building the library also creates its NuGet package
  (`GeneratePackageOnBuild`).
- All test projects use TUnit and run as executables on the Microsoft.Testing.Platform, which `global.json` selects
  for `dotnet test`, so the project is passed with `--project`, e.g. `dotnet test --project Tests/aweXpect.Web.Tests`.
  Use `-f net10.0` for a quick run and `--treenode-filter "/*/*/ThatUri+HasPort+*/*"` to select one nested test class
  (`/<assembly>/<namespace>/<class>/<test>`, nested classes are separated by `+`).
  `dotnet run --project Tests/aweXpect.Web.Tests -f net10.0 -- <options>` starts the same executable.
- Test projects and frameworks:
  - `Tests/aweXpect.Web.Tests` and `Tests/aweXpect.Web.Internal.Tests`: `net8.0;net10.0;net48`. `net48`
    needs Windows. The Internal tests are strong-name signed so that `InternalsVisibleTo` applies.
  - `Tests/aweXpect.Web.Samples.Tests`: `WebApplicationFactory` against `Source/aweXpect.Web.Samples`, shared per test
    class through `[ClassDataSource]`, `net8.0;net10.0`.
  - `Tests/aweXpect.Web.Api.Tests`: `net8.0`. It builds all target frameworks of `aweXpect.Web` after its own
    build and reads `Source/aweXpect.Web/bin/<Configuration>/<framework>/aweXpect.Web.dll`, so build and test in the
    same configuration. The target frameworks are read from `Source/Directory.Build.props`.
- Public API snapshots live next to the API tests as
  `ApiApprovalTests.aweXpect%2EWeb.<framework>.awexpect-snapshot.txt`. A mismatch writes a
  `*.awexpect-snapshot.pending.txt`. Accept intentional changes with the `[Explicit]` test
  `dotnet test --project Tests/aweXpect.Web.Api.Tests --treenode-filter "/*/*/ApiApprovalTests/AcceptApiChanges"`
  (a run without this filter skips it, because TUnit runs an explicit test only when every test that the filter
  selects is explicit), by setting
  `AWEXPECT_ACCEPT_SNAPSHOTS=all`, or by renaming the pending file. Delete leftover pending files.
- AOT smoke tests: `Tests/Aot/aweXpect.Web.Aot` is a console app (`net10.0`, `PublishAot`) whose checks report through
  the exit code. `dotnet run --project Tests/Aot/aweXpect.Web.Aot` runs them under the JIT;
  `./build.ps1 AotSmokeTests` also publishes them with Native AOT for the current runtime and runs the executable.
  Trimmer and ILC warnings fail the publish. The native link step needs the platform toolchain (on Windows the
  "Desktop development with C++" workload, found via `vswhere`).

## Build pipeline

`build.ps1`, `build.sh` and `build.cmd` build and run `Pipeline/Build.csproj` (a NUKE-style build on
[Fallout](https://www.nuget.org/packages/Fallout.Common)) with the given targets, e.g. `./build.ps1 UnitTests`. `./build.ps1 --help` lists the targets.
Without a target the defaults `ApiChecks`, `Benchmarks` and `CodeAnalysis` run. Locally the configuration is `Debug`,
on a build server `Release`; override with `--configuration Release`. The pipeline cleans `Artifacts/` and
`TestResults/` before restoring.

| Target                  | Purpose                                                                                   |
|-------------------------|-------------------------------------------------------------------------------------------|
| `Compile`               | Restore (with `nuget.config`) and build the solution with the GitVersion-derived version  |
| `UnitTests`             | Unit, Internal and Samples tests on every target framework (`net48` on Windows only), each writing `<project>_<framework>.trx` and `<project>_<framework>.cobertura.xml` to `TestResults` |
| `CodeCoverage`          | `UnitTests` plus a SonarQube coverage report in `TestResults/reports`                     |
| `ApiChecks`             | Runs `Tests/aweXpect.Web.Api.Tests` and writes `TestResults/aweXpect.Web.Api.Tests.trx`   |
| `AotSmokeTests`         | Runs the AOT smoke tests under the JIT and published with Native AOT                      |
| `Benchmarks`            | Runs `Benchmarks/aweXpect.Web.Benchmarks` in Release and logs the report                  |
| `MutationTests`         | Stryker on `aweXpect.Web` with the unit and API test projects, `net10.0`, since `main`, with Stryker's runner for the Microsoft.Testing.Platform (`--test-runner mtp`) |
| `CodeAnalysis`          | SonarCloud analysis; needs `SonarToken`                                                   |
| `Pack`                  | Rewrites the README badges for the release and moves the packages to `Artifacts/Packages` |
| `BenchmarkComment`, `MutationTestsComment`, `MutationTestsDashboard` | CI only: comment on the pull request and upload the Stryker report |

Mutation tests and benchmarks are slow; leave them to CI unless the task is about them. For a single benchmark run
`dotnet run -c Release --project Benchmarks/aweXpect.Web.Benchmarks -- --filter *Content*`.

## Documentation

- `Docs/pages/*.md` is published to [docs.testably.org](https://docs.testably.org/aweXpect/extensions/aweXpect.Web/):
  after a successful `Build` on `main`, `notify-docs-site.yml` triggers a rebuild of `Testably/Testably.Site`.
  `Docs/pages/00-index.md` embeds `README.md` at the `{README}` placeholder.
- `README.md` is also the package readme. Its overview tables link to anchors in `Docs/pages/`; keep the tables in the
  README and on each page in sync, and keep the failure message in "At a glance" in sync with
  `Tests/aweXpect.Web.Samples.Tests`.
- `Docs/agents/` holds this guide and is not published.

## CI workflows

- `ci.yml` (pull requests to `main`): unit tests with coverage on Windows, Linux and macOS, AOT smoke tests on all
  three, API tests, mutation tests, benchmarks, Sonar analysis (skipped for forks and Dependabot) and a test result
  summary.
- `ci-analysis.yml` (after `CI` completes): uploads the mutation report and writes the mutation and benchmark comments
  on the pull request.
- `build.yml` (push to `main`, tags `v*`): the same checks plus `Pack`; on a tag it pushes the packages to nuget.org via
  trusted publishing, creates the GitHub release for stable versions and comments on the released issues.
- `notify-docs-site.yml`: see Documentation.
- Dependabot updates actions and NuGet packages weekly with `chore:` commits; Mergify squash-merges them when the unit
  and API tests pass.
