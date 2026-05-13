using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxUtilityMatrixTests
{
	[DataTestMethod]
	[DataRow( "flex", "display: flex;" )]
	[DataRow( "hidden", "display: none;" )]
	[DataRow( "flex-col", "flex-direction: column;" )]
	[DataRow( "flex-row-reverse", "flex-direction: row-reverse;" )]
	[DataRow( "flex-wrap", "flex-wrap: wrap;" )]
	[DataRow( "grow", "flex-grow: 1;" )]
	[DataRow( "shrink-0", "flex-shrink: 0;" )]
	[DataRow( "flex-1", "flex-basis: 0%;" )]
	[DataRow( "order-first", "order: -9999;" )]
	[DataRow( "items-center", "align-items: center;" )]
	[DataRow( "self-end", "align-self: flex-end;" )]
	[DataRow( "content-between", "align-content: space-between;" )]
	[DataRow( "justify-around", "justify-content: space-around;" )]
	[DataRow( "basis-1/2", "flex-basis: 50%;" )]
	public void GeneratesFlexAndLayoutUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "static", "position: static;" )]
	[DataRow( "relative", "position: relative;" )]
	[DataRow( "absolute", "position: absolute;" )]
	[DataRow( "inset-0", "top: 0;" )]
	[DataRow( "inset-x-4", "left: 16px;" )]
	[DataRow( "inset-y-[12px]", "top: 12px;" )]
	[DataRow( "top-full", "top: 100%;" )]
	[DataRow( "-left-2", "left: -8px;" )]
	[DataRow( "bottom-auto", "bottom: auto;" )]
	public void GeneratesPositionUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "w-full", "width: 100%;" )]
	[DataRow( "w-screen", "width: 100vw;" )]
	[DataRow( "w-3/4", "width: 75%;" )]
	[DataRow( "size-4", "width: 16px;" )]
	[DataRow( "h-screen", "height: 100vh;" )]
	[DataRow( "min-w-0", "min-width: 0;" )]
	[DataRow( "max-w-[640px]", "max-width: 640px;" )]
	[DataRow( "min-h-[24px]", "min-height: 24px;" )]
	[DataRow( "max-h-full", "max-height: 100%;" )]
	public void GeneratesSizingUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "p-0", "padding: 0;" )]
	[DataRow( "p-4", "padding: 16px;" )]
	[DataRow( "px-[14px]", "padding-left: 14px;" )]
	[DataRow( "py-2", "padding-top: 8px;" )]
	[DataRow( "pt-1", "padding-top: 4px;" )]
	[DataRow( "pr-2", "padding-right: 8px;" )]
	[DataRow( "pb-3", "padding-bottom: 12px;" )]
	[DataRow( "pl-4", "padding-left: 16px;" )]
	[DataRow( "m-auto", "margin: auto;" )]
	[DataRow( "-mt-2", "margin-top: -8px;" )]
	[DataRow( "mx-3", "margin-left: 12px;" )]
	[DataRow( "gap-4", "gap: 16px;" )]
	[DataRow( "gap-x-3", "column-gap: 12px;" )]
	[DataRow( "gap-y-[18px]", "row-gap: 18px;" )]
	public void GeneratesSpacingUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "bg-accent", "background-color: #d7b46a;" )]
	[DataRow( "bg-accent/50", "background-color: rgba( 215, 180, 106, 0.5 );" )]
	[DataRow( "bg-[#0d1418]", "background-color: #0d1418;" )]
	[DataRow( "bg-[rgba(1,_2,_3,_0.5)]", "background-color: rgba(1, 2, 3, 0.5);" )]
	[DataRow( "bg-[url(/ui/site_plan.png)]", "background-image: url(/ui/site_plan.png);" )]
	[DataRow( "bg-cover", "background-size: cover;" )]
	[DataRow( "text-sm", "font-size: 13px;" )]
	[DataRow( "text-lg/7", "line-height: 28px;" )]
	[DataRow( "text-[18px]", "font-size: 18px;" )]
	[DataRow( "text-accent", "color: #d7b46a;" )]
	[DataRow( "text-accent/50", "color: rgba( 215, 180, 106, 0.5 );" )]
	[DataRow( "text-[#fefefe]", "color: #fefefe;" )]
	[DataRow( "border", "border: 1px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-2", "border-width: 2px;" )]
	[DataRow( "border-x-2", "border-left-width: 2px;" )]
	[DataRow( "border-accent", "border-color: #d7b46a;" )]
	[DataRow( "border-t-accent", "border-top-color: #d7b46a;" )]
	[DataRow( "rounded", "border-radius: 6px;" )]
	[DataRow( "rounded-t-lg", "border-top-left-radius: 12px;" )]
	[DataRow( "rounded-full", "border-radius: 9999px;" )]
	[DataRow( "rounded-[10px]", "border-radius: 10px;" )]
	public void GeneratesColorBorderAndRadiusUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "text-left", "text-align: left;" )]
	[DataRow( "text-center", "text-align: center;" )]
	[DataRow( "uppercase", "text-transform: uppercase;" )]
	[DataRow( "normal-case", "text-transform: none;" )]
	[DataRow( "italic", "font-style: italic;" )]
	[DataRow( "not-italic", "font-style: normal;" )]
	[DataRow( "font-bold", "font-weight: 700;" )]
	[DataRow( "font-mono", "font-family: Roboto Mono;" )]
	[DataRow( "font-[650]", "font-weight: 650;" )]
	[DataRow( "leading-4", "line-height: 16px;" )]
	[DataRow( "tracking-[2px]", "letter-spacing: 2px;" )]
	[DataRow( "underline", "text-decoration-line: underline;" )]
	[DataRow( "decoration-accent", "text-decoration-color: #d7b46a;" )]
	[DataRow( "decoration-2", "text-decoration-thickness: 2px;" )]
	[DataRow( "underline-offset-4", "text-decoration-underline-offset: 4px;" )]
	[DataRow( "truncate", "text-overflow: ellipsis;" )]
	[DataRow( "whitespace-nowrap", "white-space: nowrap;" )]
	[DataRow( "break-all", "word-break: break-all;" )]
	public void GeneratesTypographyUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "overflow-hidden", "overflow: hidden;" )]
	[DataRow( "overflow-scroll", "overflow: scroll;" )]
	[DataRow( "overflow-x-scroll", "overflow-x: scroll;" )]
	[DataRow( "overflow-y-hidden", "overflow-y: hidden;" )]
	[DataRow( "opacity-75", "opacity: 0.75;" )]
	[DataRow( "opacity-[0.35]", "opacity: 0.35;" )]
	[DataRow( "pointer-events-none", "pointer-events: none;" )]
	[DataRow( "pointer-events-all", "pointer-events: all;" )]
	[DataRow( "cursor-pointer", "cursor: pointer;" )]
	[DataRow( "cursor-not-allowed", "cursor: not-allowed;" )]
	[DataRow( "z-10", "z-index: 10;" )]
	[DataRow( "-z-10", "z-index: -10;" )]
	[DataRow( "z-[999]", "z-index: 999;" )]
	public void GeneratesInteractionVisibilityAndLayerUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "transition", "transition: all 0.15s ease;" )]
	[DataRow( "transition-colors", "transition-property: background-color, border-color, color, text-decoration-color;" )]
	[DataRow( "transition-opacity", "transition-property: opacity;" )]
	[DataRow( "duration-150", "transition-duration: 0.15s;" )]
	[DataRow( "delay-75", "transition-delay: 0.075s;" )]
	[DataRow( "ease-in-out", "transition-timing-function: ease-in-out;" )]
	[DataRow( "shadow", "box-shadow: 0 12px 32px rgba( 0, 0, 0, 0.34 );" )]
	[DataRow( "shadow-none", "box-shadow: none;" )]
	[DataRow( "shadow-[0_0_12px_rgba(0,_0,_0,_0.5)]", "box-shadow: 0 0 12px rgba(0, 0, 0, 0.5);" )]
	[DataRow( "text-shadow-lg", "text-shadow: 0 4px 12px rgba( 0, 0, 0, 0.5 );" )]
	[DataRow( "blur-sm", "filter-blur: 4px;" )]
	[DataRow( "backdrop-blur-lg", "backdrop-filter-blur: 16px;" )]
	[DataRow( "brightness-50", "filter-brightness: 0.5;" )]
	[DataRow( "hue-rotate-15", "filter-hue-rotate: 15deg;" )]
	[DataRow( "mix-blend-multiply", "mix-blend-mode: multiply;" )]
	[DataRow( "animate-none", "animation: none;" )]
	[DataRow( "transform-[translateX(4px)_scale(1.1)]", "transform: translateX(4px) scale(1.1);" )]
	[DataRow( "origin-top-left", "transform-origin: top left;" )]
	[DataRow( "aspect-square", "aspect-ratio: 1;" )]
	[DataRow( "aspect-video", "aspect-ratio: 16/9;" )]
	[DataRow( "aspect-[4/3]", "aspect-ratio: 4/3;" )]
	public void GeneratesTransitionEffectAndAspectUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "hover:bg-accent", "background-color: #d7b46a;" )]
	[DataRow( "active:bg-accent", "background-color: #d7b46a;" )]
	[DataRow( "focus:border-accent", "border-color: #d7b46a;" )]
	[DataRow( "intro:opacity-0", "opacity: 0;" )]
	[DataRow( "outro:opacity-0", "opacity: 0;" )]
	[DataRow( "hover:focus:bg-panel", "background-color: rgba( 34, 39, 44, 0.94 );" )]
	public void GeneratesPseudoVariantUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[TestMethod]
	public void GeneratesImportantUtilities()
	{
		var result = GenerateSafelist( "!p-4" );

		StringAssert.Contains( result.GeneratedScss, ".\\!p-4 {" );
		StringAssert.Contains( result.GeneratedScss, "padding: 16px !important;" );
		Assert.AreEqual( 1, result.GeneratedClassCount );
	}

	[TestMethod]
	public void ReportsUnsupportedUtilityLookingClasses()
	{
		var result = GenerateSafelist(
			"grid",
			"md:flex",
			"-p-4",
			"hover:grid",
			"first:flex",
			"rotate-45",
			"[--brand-color:#fff]",
			"[grid-template-columns:repeat(2,_1fr)]",
			"overflow-auto",
			"select-none" );

		Assert.IsFalse( result.GeneratedClasses.Contains( "grid" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "grid" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "md:flex" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "-p-4" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "hover:grid" ) );
		AssertSkip( result, "grid", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "md:flex", TailBoxSkipReason.UnsupportedMediaVariant );
		AssertSkip( result, "-p-4", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "first:flex", TailBoxSkipReason.UnsupportedSelectorVariant );
		AssertSkip( result, "rotate-45", TailBoxSkipReason.UnsupportedUtility );
		AssertSkip( result, "[--brand-color:#fff]", TailBoxSkipReason.UnsupportedArbitraryProperty );
		AssertSkip( result, "[grid-template-columns:repeat(2,_1fr)]", TailBoxSkipReason.UnsupportedProperty );
		AssertSkip( result, "overflow-auto", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "select-none", TailBoxSkipReason.UnsupportedProperty );
		Assert.IsTrue( result.Warnings.Any( warning => warning.Contains( "Responsive/media variant 'md'" ) ) );
		Assert.IsTrue( result.Warnings.Any( warning => warning.Contains( "Unsupported TailBox variant 'hover'" ) ) == false );
	}

	[TestMethod]
	public void GeneratesArbitraryPropertiesWhenSboxSupportsTheProperty()
	{
		var result = GenerateSafelist( "[background-color:#0d1418]", "hover:[opacity:0.5]" );

		StringAssert.Contains( result.GeneratedScss, RuleStart( "[background-color:#0d1418]" ) );
		StringAssert.Contains( result.GeneratedScss, "background-color: #0d1418;" );
		StringAssert.Contains( result.GeneratedScss, RuleStart( "hover:[opacity:0.5]" ) );
		StringAssert.Contains( result.GeneratedScss, "opacity: 0.5;" );
		Assert.AreEqual( 2, result.GeneratedClassCount );
		Assert.AreEqual( 0, result.SkippedClassCount );
	}

	[TestMethod]
	public void ThemeBucketsCanOverrideTailwindLikeTokens()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			config.LineHeights["panel"] = "30px";
			config.Durations["fastish"] = "0.12s";
			config.Easings["snap"] = "cubic-bezier(.2,0,0,1)";
			config.FontFamilies["display"] = "Poppins";
			config.TextShadows["panel"] = "0 3px 6px rgba( 0, 0, 0, 0.5 )";
			config.Safelist.Add( "text-lg/panel duration-fastish ease-snap font-display text-shadow-panel" );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );

			StringAssert.Contains( result.GeneratedScss, "line-height: 30px;" );
			StringAssert.Contains( result.GeneratedScss, "transition-duration: 0.12s;" );
			StringAssert.Contains( result.GeneratedScss, "transition-timing-function: cubic-bezier(.2,0,0,1);" );
			StringAssert.Contains( result.GeneratedScss, "font-family: Poppins;" );
			StringAssert.Contains( result.GeneratedScss, "text-shadow: 0 3px 6px rgba( 0, 0, 0, 0.5 );" );
			Assert.AreEqual( 5, result.GeneratedClassCount );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	private static void AssertUtility( string className, string declaration )
	{
		var result = GenerateSafelist( className );

		StringAssert.Contains( result.GeneratedScss, RuleStart( className ) );
		StringAssert.Contains( result.GeneratedScss, declaration );
		Assert.AreEqual( 1, result.GeneratedClassCount, $"Expected one generated rule for {className}." );
		Assert.AreEqual( 0, result.SkippedClasses.Count, $"Expected no skipped classes for {className}." );
		Assert.AreEqual( 0, result.Warnings.Count, $"Expected no warnings for {className}." );
	}

	private static string RuleStart( string className )
	{
		Assert.IsTrue( TailBoxCandidateParser.TryParse( className, out var candidate, out _ ), $"Expected '{className}' to parse." );
		return "." + TailBoxUtilityCompiler.EscapeClassSelector( className )
			+ string.Concat( candidate.Variants.Select( variant => variant.SelectorSuffix ) )
			+ " {";
	}

	private static void AssertSkip( TailBoxGenerationResult result, string className, TailBoxSkipReason reason )
	{
		var skipped = result.Skipped.SingleOrDefault( item => item.ClassName == className );
		Assert.IsNotNull( skipped, $"Expected '{className}' to be skipped." );
		Assert.AreEqual( reason, skipped.Reason, $"Unexpected skip reason for '{className}': {skipped.Detail}" );
	}

	private static TailBoxGenerationResult GenerateSafelist( params string[] classes )
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			config.Safelist.AddRange( classes );
			return TailBoxEditorProject.Generate( root, config, writeFile: false );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	private static string CreateTempProject()
	{
		return TailBoxTestPaths.CreateTempProject();
	}

	private static void DeleteTempProject( string root )
	{
		if ( Directory.Exists( root ) )
		{
			Directory.Delete( root, true );
		}
	}
}
