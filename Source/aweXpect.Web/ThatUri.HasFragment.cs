using System;
using aweXpect.Core;
using aweXpect.Helpers;
using aweXpect.Results;

namespace aweXpect;

#nullable enable
public static partial class ThatUri
{
	/// <summary>
	///     Verifies that the fragment of the <see cref="Uri" /> subject, without the leading <c>#</c>…
	/// </summary>
	/// <remarks>
	///     <seealso cref="Uri.Fragment" />
	/// </remarks>
	[GuaranteesNotNull]
	public static PropertyResult.String<Uri?, Uri, IThat<Uri?>> HasFragment(this IThat<Uri?> source)
		=> new(source.FailIfRelative(), u => u is null ? null : TrimFragmentDelimiter(u.Fragment), "fragment");

	private static string TrimFragmentDelimiter(string fragment)
		=> fragment.Length > 0 ? fragment.Substring(1) : fragment;
}
