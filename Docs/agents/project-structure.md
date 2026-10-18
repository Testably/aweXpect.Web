# Architecture and Source Locations

## Directory layout

- `Source/aweXpect.Web/`: the shipped package (root namespace `aweXpect`).
- `Source/aweXpect.Web.Samples/`: a minimal API web application (`net8.0;net10.0`, not packed, not trimmable) used by
  the Samples tests and the README example.
- `Tests/`: one test project per concern (see `Docs/agents/workflows.md`); `Tests/Aot/` holds the AOT smoke tests.
- `Benchmarks/aweXpect.Web.Benchmarks/`: BenchmarkDotNet happy-case benchmarks.
- `Pipeline/`: the build pipeline (`Build.*.cs`, one partial file per area).
- `Docs/pages/`: the published documentation; `Docs/agents/`: this guide.
- `.github/workflows/`: CI, see `Docs/agents/workflows.md`.

## `aweXpect.Web`

- Expectations are extension methods on `IThat<T?>` in one `public static partial class` per subject type, in the
  `aweXpect` namespace: `ThatHttpRequestMessage`, `ThatHttpResponseMessage`, `ThatUri` and `ThatIPAddress`. The base
  file (e.g. `ThatUri.cs`) holds shared private helpers; each expectation lives in `That<Type>.<Expectation>.cs`
  and contains its negated form (e.g. `IsAbsolute` and `IsNotAbsolute`) and its constraint classes.
- `Web/Results/` (`aweXpect.Web.Results`): public result types for fluent continuations, e.g. `StatusCodeResult`
  (`.Success()`, `.DifferentTo(...)`), `HasHeaderValueResult` (`.WithValue(...)`, `.WhoseValue(...)`),
  `HasQueryParameterValueResult` and `ProblemDetailsResult`.
- `Web/ContentProcessors/` (`aweXpect.Web.ContentProcessors`): `IContentProcessor` and the JSON, string and binary
  processors that format `HttpContent` in failure messages.
- `Web/WebAwexpectCustomizationExtensions.cs` (`aweXpect.Web`): `Customize.aweXpect.Web()` with the
  `ContentProcessors` setting.
- `Helpers/` (internal): `HttpFormatter` writes the `HTTP-Request:`/`HTTP-Response:` sections appended to failures
  on requests and responses; `ThrowHelper` validates arguments; further extensions on `HttpContent`, `Uri`, strings
  and aweXpect.Core options.
- `Usings.cs` imports `aweXpect.Formatting` globally. The building blocks (`ExpectationBuilder`, `ConstraintResult`,
  `Outcome`, `PropertyResult`, `Tracing.WriteException`, `[GuaranteesNotNull]`) come from aweXpect.Core.

## Behaviour rules visible in the code

- A `null` subject fails both ways (also under negation) with `but it was <null>`.
- A relative `Uri` fails component expectations both ways (`FailIfRelative()` in `ThatUri.cs`, `but it was relative`)
  instead of throwing.
- Invalid arguments throw through `ThrowHelper` or `Tracing.WriteException(...)` with messages like
  `The 'expected' cannot be null.`; an option specified twice throws `InvalidOperationException`.

## Adding an expectation

Recent feature commits touch the same set of files:

1. `Source/aweXpect.Web/That<Type>.<Expectation>.cs` with XML docs and, where it applies, `[GuaranteesNotNull]`.
2. Tests in `Tests/aweXpect.Web.Tests/That<Type>.<Expectation>.Tests.cs` (and `That<Type>.<Negated>.Tests.cs`),
   nested as `That<Type>.<Expectation>.Tests`/`NegatedTests`, asserting the complete failure message with
   `Throws<XunitException>().WithMessage("""...""")`. Internal helpers are tested in
   `Tests/aweXpect.Web.Internal.Tests`.
3. Updated API snapshots for all three target frameworks.
4. A check in `Tests/Aot/aweXpect.Web.Aot/Checks.cs`.
5. The overview table in `README.md` and the section on the matching `Docs/pages/` page.
