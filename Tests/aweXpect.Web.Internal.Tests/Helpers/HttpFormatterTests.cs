using System.Net.Http;
using System.Threading;
using aweXpect.Helpers;

namespace aweXpect.Web.Internal.Tests;

public sealed class HttpFormatterTests
{
	[Fact]
	public async Task Format_HttpRequestMessage_WhenNull_ShouldReturnNullString()
	{
		HttpRequestMessage? sut = null;

		string result = await HttpFormatter.Format(sut, "  ", CancellationToken.None);

		await That(result).IsEqualTo("<null>");
	}

	[Fact]
	public async Task Format_HttpRequestMessage_WhenFormattedTwice_ShouldReturnSameText()
	{
		HttpRequestMessage sut = new(HttpMethod.Post, "https://aweXpect.com")
		{
			Content = new StringContent("foo"),
		};

		string first = await HttpFormatter.Format(sut, "  ", CancellationToken.None);
		string second = await HttpFormatter.Format(sut, "  ", CancellationToken.None);

		await That(second).IsEqualTo(first)
			.Because("the failure message shows equal contexts only once");
	}

	[Fact]
	public async Task Format_HttpResponseMessage_WhenFormattedTwice_ShouldReturnSameText()
	{
		HttpResponseMessage sut = new()
		{
			Content = new StringContent("foo"),
		};

		string first = await HttpFormatter.Format(sut, "  ", CancellationToken.None);
		string second = await HttpFormatter.Format(sut, "  ", CancellationToken.None);

		await That(second).IsEqualTo(first)
			.Because("the failure message shows equal contexts only once");
	}

	[Fact]
	public async Task Format_HttpResponseMessage_WhenNull_ShouldReturnNullString()
	{
		HttpResponseMessage? sut = null;

		string result = await HttpFormatter.Format(sut, "  ", CancellationToken.None);

		await That(result).IsEqualTo("<null>");
	}
}
