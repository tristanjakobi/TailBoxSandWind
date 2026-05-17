using Sandbox.TailBox;
using System.Linq;

[TestClass]
public sealed class TailBoxParserParityTests
{
	[TestMethod]
	public void SegmentsCandidatesWithNestedBracketsQuotesAndEscapes()
	{
		CollectionAssert.AreEqual( new[] { "foo" }, TailBoxText.Segment( "foo", ':' ) );
		CollectionAssert.AreEqual( new[] { "foo", "bar", "baz" }, TailBoxText.Segment( "foo:bar:baz", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "(b:c)", "d" }, TailBoxText.Segment( "a:(b:c):d", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "[b:c]", "d" }, TailBoxText.Segment( "a:[b:c]:d", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "{b:c}", "d" }, TailBoxText.Segment( "a:{b:c}:d", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "\"b:c\"", "d" }, TailBoxText.Segment( "a:\"b:c\":d", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "'b:c'", "d" }, TailBoxText.Segment( "a:'b:c':d", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "\"b:c\\\":d\"", "e" }, TailBoxText.Segment( "a:\"b:c\\\":d\":e", ':' ) );
		CollectionAssert.AreEqual( new[] { "a", "b", "c", "d" }, TailBoxText.Segment( "a\\b\\c\\d", '\\' ) );
		CollectionAssert.AreEqual( new[] { "a", "(b\\c)", "d" }, TailBoxText.Segment( "a\\(b\\c)\\d", '\\' ) );
	}

	[TestMethod]
	public void DecodesArbitraryValuesLikeTailwindReferenceFixtures()
	{
		Assert.AreEqual( "hello world", TailBoxText.DecodeArbitraryValue( "hello_world" ) );
		Assert.AreEqual( "hello_world", TailBoxText.DecodeArbitraryValue( "hello\\_world" ) );
		Assert.AreEqual( "rgba(1, 2, 3, 0.5)", TailBoxText.DecodeArbitraryValue( "rgba(1,_2,_3,_0.5)" ) );
		Assert.AreEqual( "url('/what_a_rush.png')", TailBoxText.DecodeArbitraryValue( "url('/what_a_rush.png')" ) );
		Assert.AreEqual( "var(--brand_color, red blue)", TailBoxText.DecodeArbitraryValue( "var(--brand_color,_red_blue)" ) );
		Assert.AreEqual( "theme(--spacing_unit, 1 rem)", TailBoxText.DecodeArbitraryValue( "theme(--spacing_unit,_1_rem)" ) );
	}

	[TestMethod]
	public void EscapesSelectorsLikeTailwindEscape()
	{
		Assert.AreEqual( "red-1\\/2", TailBoxText.EscapeIdentifier( "red-1/2" ) );
		Assert.AreEqual( "hover\\:bg-\\[\\#0d1418\\]", TailBoxText.EscapeIdentifier( "hover:bg-[#0d1418]" ) );
		Assert.AreEqual( "opacity-\\[0\\.5\\]", TailBoxText.EscapeIdentifier( "opacity-[0.5]" ) );
		Assert.AreEqual( "z-\\[100\\%\\]", TailBoxText.EscapeIdentifier( "z-[100%]" ) );
		Assert.AreEqual( "\\31 \\/2", TailBoxText.EscapeIdentifier( "1/2" ) );
		Assert.AreEqual( "-mt-2", TailBoxText.EscapeIdentifier( "-mt-2" ) );
	}

	[TestMethod]
	public void EscapesGeneratedSelectorsWithSboxSafeFixedHexEscapes()
	{
		Assert.AreEqual( "red-1\\00002f 2", TailBoxText.EscapeIdentifierForSboxSelector( "red-1/2" ) );
		Assert.AreEqual( "hover\\00003a bg-\\00005b \\000023 0d1418\\00005d ", TailBoxText.EscapeIdentifierForSboxSelector( "hover:bg-[#0d1418]" ) );
		Assert.AreEqual( "opacity-\\00005b 0\\00002e 5\\00005d ", TailBoxText.EscapeIdentifierForSboxSelector( "opacity-[0.5]" ) );
		Assert.AreEqual( "z-\\00005b 100\\000025 \\00005d ", TailBoxText.EscapeIdentifierForSboxSelector( "z-[100%]" ) );
		Assert.AreEqual( "\\000031 \\00002f 2", TailBoxText.EscapeIdentifierForSboxSelector( "1/2" ) );
		Assert.AreEqual( "active\\00003a bg-accent-dark", TailBoxText.EscapeIdentifierForSboxSelector( "active:bg-accent-dark" ) );
		Assert.AreEqual( "\\00002d mt-2", TailBoxText.EscapeIdentifierForSboxSelector( "-mt-2" ) );
	}

	[TestMethod]
	public void ParsesImportantNegativeModifiersFractionsAndArbitraryProperties()
	{
		Assert.IsTrue( TailBoxCandidateParser.TryParse( "hover:!text-lg/7", out var text, out _ ) );
		Assert.AreEqual( "text-lg", text.Base );
		Assert.AreEqual( "7", text.Modifier );
		Assert.IsTrue( text.Important );
		Assert.AreEqual( TailBoxVariantKind.Pseudo, text.Variants.Single().Kind );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "!-mt-2", out var negative, out _ ) );
		Assert.AreEqual( "mt-2", negative.Base );
		Assert.IsTrue( negative.Important );
		Assert.IsTrue( negative.Negative );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "w-1/2", out var fraction, out _ ) );
		Assert.AreEqual( "w-1/2", fraction.Base );
		Assert.IsNull( fraction.Modifier );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "bg-accent/50", out var color, out _ ) );
		Assert.AreEqual( "bg-accent", color.Base );
		Assert.AreEqual( "50", color.Modifier );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "focus:[background-color:#0d1418]", out var arbitraryProperty, out _ ) );
		Assert.IsTrue( arbitraryProperty.IsArbitraryProperty );
		Assert.AreEqual( "background-color", arbitraryProperty.ArbitraryProperty );
		Assert.AreEqual( "#0d1418", arbitraryProperty.ArbitraryValue );
		Assert.AreEqual( "focus", arbitraryProperty.Variants.Single().Raw );
	}

	[TestMethod]
	public void ClassifiesUnsupportedVariantsDeterministically()
	{
		Assert.IsTrue( TailBoxCandidateParser.TryParse( "unknown:flex", out var unknown, out _ ) );
		Assert.AreEqual( TailBoxVariantKind.Unsupported, unknown.Variants.Single().Kind );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "first:flex", out var selector, out _ ) );
		Assert.AreEqual( TailBoxVariantKind.Selector, selector.Variants.Single().Kind );

		Assert.IsTrue( TailBoxCandidateParser.TryParse( "mystery:flex", out var unsupported, out _ ) );
		Assert.AreEqual( TailBoxVariantKind.Unsupported, unsupported.Variants.Single().Kind );
	}
}
