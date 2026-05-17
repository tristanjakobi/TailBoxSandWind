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
	[DataRow( "w-0", "width: 0;" )]
	[DataRow( "w-full", "width: 100%;" )]
	[DataRow( "w-screen", "width: 100vw;" )]
	[DataRow( "w-3/4", "width: 75%;" )]
	[DataRow( "size-4", "width: 16px;" )]
	[DataRow( "h-screen", "height: 100vh;" )]
	[DataRow( "max-w-[640px]", "max-width: 640px;" )]
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
	[DataRow( "gap-x-3", "gap: 0 12px;" )]
	[DataRow( "gap-x-3", "column-gap: 12px;" )]
	[DataRow( "gap-y-[18px]", "gap: 18px 0;" )]
	[DataRow( "gap-y-[18px]", "row-gap: 18px;" )]
	public void GeneratesSpacingUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[TestMethod]
	public void GeneratesCompoundGapAxisRuleWhenBothAxesArePresent()
	{
		var config = TailBoxConfig.CreateDefault();
		config.Safelist.Add( "gap-x-3 gap-y-2" );

		var result = new TailBoxGenerator().GenerateFromSources( Array.Empty<TailBoxSourceText>(), config );

		StringAssert.Contains( result.GeneratedScss, ".gap-x-3.gap-y-2 {" );
		StringAssert.Contains( result.GeneratedScss, "gap: 8px 12px;" );
		StringAssert.Contains( result.GeneratedScss, "row-gap: 8px;" );
		StringAssert.Contains( result.GeneratedScss, "column-gap: 12px;" );
	}

	[DataTestMethod]
	[DataRow( "bg-accent", "background-color: #d7b46a;" )]
	[DataRow( "bg-accent/50", "background-color: rgba( 215, 180, 106, 0.5 );" )]
	[DataRow( "bg-[#0d1418]", "background-color: #0d1418;" )]
	[DataRow( "bg-[rgba(1,_2,_3,_0.5)]", "background-color: rgba(1, 2, 3, 0.5);" )]
	[DataRow( "text-sm", "font-size: 13px;" )]
	[DataRow( "text-lg/7", "line-height: 28px;" )]
	[DataRow( "text-[18px]", "font-size: 18px;" )]
	[DataRow( "text-accent", "color: #d7b46a;" )]
	[DataRow( "text-[#fefefe]", "color: #fefefe;" )]
	[DataRow( "border", "border: 1px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-2", "border-width: 2px;" )]
	[DataRow( "border-x-2", "border-left: 2px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-t-[8px]", "border-top: 8px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-b-[8px]", "border-bottom: 8px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-accent", "border-color: #d7b46a;" )]
	[DataRow( "border-t-accent", "border-top: 1px solid #d7b46a;" )]
	[DataRow( "border-b-accent", "border-bottom: 1px solid #d7b46a;" )]
	[DataRow( "rounded", "border-radius: 6px;" )]
	[DataRow( "rounded-none", "border-radius: 0px;" )]
	[DataRow( "rounded-full", "border-radius: 9999px;" )]
	[DataRow( "rounded-[10px]", "border-radius: 10px;" )]
	[DataRow( "rounded-t", "border-radius: 6px 6px 0px 0px;" )]
	[DataRow( "rounded-r-lg", "border-radius: 0px 12px 12px 0px;" )]
	[DataRow( "rounded-b-[10px]", "border-radius: 0px 0px 10px 10px;" )]
	[DataRow( "rounded-l-md", "border-radius: 8px 0px 0px 8px;" )]
	[DataRow( "rounded-tl-lg", "border-radius: 12px 0px 0px 0px;" )]
	[DataRow( "rounded-tr-lg", "border-radius: 0px 12px 0px 0px;" )]
	[DataRow( "rounded-br-lg", "border-radius: 0px 0px 12px 0px;" )]
	[DataRow( "rounded-bl-lg", "border-radius: 0px 0px 0px 12px;" )]
	public void GeneratesColorBorderAndRadiusUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[TestMethod]
	public void GeneratesCompoundBorderSideRuleWhenSideWidthAndColorArePresent()
	{
		var config = TailBoxConfig.CreateDefault();
		config.Safelist.Add( "border-t-[8px] border-t-accent border-l-2 border-l-danger" );

		var result = new TailBoxGenerator().GenerateFromSources( Array.Empty<TailBoxSourceText>(), config );

		StringAssert.Contains( result.GeneratedScss, ".border-t-\\00005b 8px\\00005d .border-t-accent {" );
		StringAssert.Contains( result.GeneratedScss, "border-top: 8px solid #d7b46a;" );
		StringAssert.Contains( result.GeneratedScss, ".border-l-2.border-l-danger {" );
		StringAssert.Contains( result.GeneratedScss, "border-left: 2px solid #c95d5d;" );
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
	[DataRow( "leading-relaxed", "line-height: 1.625em;" )]
	[DataRow( "tracking-[2px]", "letter-spacing: 2px;" )]
	[DataRow( "underline", "text-decoration-line: underline;" )]
	[DataRow( "decoration-accent", "text-decoration-color: #d7b46a;" )]
	[DataRow( "decoration-2", "text-decoration-thickness: 2px;" )]
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
	[DataRow( "pointer-events-none", "pointer-events: none;" )]
	[DataRow( "pointer-events-all", "pointer-events: all;" )]
	[DataRow( "cursor-pointer", "cursor: pointer;" )]
	[DataRow( "cursor-text", "cursor: text;" )]
	[DataRow( "cursor-not-allowed", "cursor: not-allowed;" )]
	[DataRow( "z-10", "z-index: 10;" )]
	[DataRow( "-z-10", "z-index: -10;" )]
	[DataRow( "z-[999]", "z-index: 999;" )]
	public void GeneratesInteractionVisibilityAndLayerUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "blur-sm", "filter-blur: 4px;" )]
	[DataRow( "brightness-50", "filter-brightness: 0.5;" )]
	[DataRow( "hue-rotate-15", "filter-hue-rotate: 15deg;" )]
	public void GeneratesEffectUtilities( string className, string declaration )
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

	[DataTestMethod]
	[DataRow( "cursor-none", "cursor: none;" )]
	[DataRow( "cursor-pointer", "cursor: pointer;" )]
	[DataRow( "cursor-progress", "cursor: progress;" )]
	[DataRow( "cursor-wait", "cursor: wait;" )]
	[DataRow( "cursor-crosshair", "cursor: crosshair;" )]
	[DataRow( "cursor-text", "cursor: text;" )]
	[DataRow( "cursor-move", "cursor: move;" )]
	[DataRow( "cursor-not-allowed", "cursor: not-allowed;" )]
	[DataRow( "cursor-[crosshair]", "cursor: crosshair;" )]
	public void GeneratesDocumentedCursorUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "hover:bg-accent", ":hover", "background-color: #d7b46a;" )]
	[DataRow( "active:bg-accent", ":active", "background-color: #d7b46a;" )]
	[DataRow( "focus:border-accent", ":focus", "border-color: #d7b46a;" )]
	[DataRow( "intro:opacity-0", ":intro", "opacity: 0;" )]
	[DataRow( "outro:opacity-0", ":outro", "opacity: 0;" )]
	public void GeneratesPseudoVariantUtilities( string className, string selectorSuffix, string declaration )
	{
		var result = GenerateSafelist( className );

		StringAssert.Contains( result.GeneratedScss, RuleStart( className ) );
		StringAssert.Contains( result.GeneratedScss, selectorSuffix + " {" );
		StringAssert.Contains( result.GeneratedScss, declaration );
		Assert.AreEqual( 1, result.GeneratedClassCount );
		Assert.AreEqual( 0, result.SkippedClassCount );
	}

	[TestMethod]
	public void ReportsUnsupportedUtilityLookingClasses()
	{
		var result = GenerateSafelist(
			"grid",
			"unknown:flex",
			"-p-4",
			"hover:grid",
			"first:flex",
			"rotate-45",
			"[--brand-color:#fff]",
			"[grid-template-columns:repeat(2,_1fr)]",
			"overflow-auto",
			"select-none",
			"bg-[paint-token]",
			"break-words",
			"whitespace-pre-wrap" );

		Assert.IsFalse( result.GeneratedClasses.Contains( "grid" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "grid" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "unknown:flex" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "-p-4" ) );
		Assert.IsTrue( result.SkippedClasses.Contains( "hover:grid" ) );
		AssertSkip( result, "grid", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "unknown:flex", TailBoxSkipReason.UnsupportedVariant );
		AssertSkip( result, "-p-4", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "hover:grid", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "first:flex", TailBoxSkipReason.UnsupportedSelectorVariant );
		AssertSkip( result, "rotate-45", TailBoxSkipReason.UnsupportedUtility );
		AssertSkip( result, "[--brand-color:#fff]", TailBoxSkipReason.UnsupportedArbitraryProperty );
		AssertSkip( result, "[grid-template-columns:repeat(2,_1fr)]", TailBoxSkipReason.UnsupportedProperty );
		AssertSkip( result, "overflow-auto", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "select-none", TailBoxSkipReason.UnsupportedProperty );
		AssertSkip( result, "bg-[paint-token]", TailBoxSkipReason.UnsupportedProperty );
		AssertSkip( result, "break-words", TailBoxSkipReason.UnsupportedValue );
		AssertSkip( result, "whitespace-pre-wrap", TailBoxSkipReason.UnsupportedValue );
		Assert.IsTrue( result.Warnings.Any( warning => warning.Contains( "Unsupported TailBox variant 'unknown'" ) ) );
	}

	[TestMethod]
	public void GeneratesArbitraryPropertiesWhenSboxSupportsTheProperty()
	{
		var result = GenerateSafelist( "[background-color:#0d1418]", "[z-index:5]" );

		StringAssert.Contains( result.GeneratedScss, RuleStart( "[background-color:#0d1418]" ) );
		StringAssert.Contains( result.GeneratedScss, "background-color: #0d1418;" );
		StringAssert.Contains( result.GeneratedScss, RuleStart( "[z-index:5]" ) );
		StringAssert.Contains( result.GeneratedScss, "z-index: 5;" );
		Assert.AreEqual( 2, result.GeneratedClassCount );
		Assert.AreEqual( 0, result.SkippedClassCount );
	}

	[TestMethod]
	public void GeneratesArbitraryPropertiesForDocumentedSboxStyleCatalog()
	{
		var classes = new[]
		{
			"[align-content:center]", "[align-items:center]", "[align-self:center]",
			"[animation:fade_1s_ease]", "[animation-delay:0.1s]", "[animation-direction:alternate]",
			"[animation-duration:1s]", "[animation-fill-mode:both]", "[animation-iteration-count:infinite]",
			"[animation-name:fade]", "[animation-play-state:running]", "[animation-timing-function:ease-out]",
			"[aspect-ratio:16/9]", "[backdrop-filter:blur(10px)]", "[backdrop-filter-blur:10px]",
			"[backdrop-filter-brightness:1.2]", "[backdrop-filter-contrast:1.2]", "[backdrop-filter-hue-rotate:10deg]",
			"[backdrop-filter-invert:1]", "[backdrop-filter-saturate:1.5]", "[backdrop-filter-sepia:1]",
			"[background:linear-gradient(red,_blue)]", "[background-angle:10deg]", "[background-blend-mode:multiply]",
			"[background-color:#fff]", "[background-image:url(/ui/panel.png)]", "[background-image-tint:#ffffffaa]",
			"[background-position:10px_15px]", "[background-position-x:10px]", "[background-position-y:15px]",
			"[background-repeat:repeat-x]", "[background-size:10px_15px]", "[background-size-x:10px]",
			"[background-size-y:15px]", "[border:1px_solid_black]", "[border-bottom:1px_solid_black]",
			"[border-bottom-color:#fff]", "[border-bottom-left-radius:8px]", "[border-bottom-right-radius:8px]",
			"[border-bottom-width:1px]", "[border-color:#fff]", "[border-image:url(/ui/border.png)]",
			"[border-image-tint:#ffffffaa]", "[border-image-tint:#000]", "[border-image-width-bottom:1px]", "[border-image-width-left:1px]",
			"[border-image-width-right:1px]", "[border-image-width-top:1px]", "[border-left:1px_solid_black]",
			"[border-left-color:#fff]", "[border-left-width:1px]", "[border-radius:8px]",
			"[border-right:1px_solid_black]", "[border-right-color:#fff]", "[border-right-width:1px]",
			"[border-top:1px_solid_black]", "[border-top-color:#fff]", "[border-top-left-radius:8px]",
			"[border-top-right-radius:8px]", "[border-top-width:1px]", "[border-width:1px]",
			"[bottom:10px]", "[box-shadow:0_0_12px_black]", "[color:#fff]", "[column-gap:10px]",
			"[content:\"Loading\"]", "[cursor:crosshair]", "[display:flex]", "[filter:blur(10px)]",
			"[filter-blur:10px]", "[filter-border-color:#fff]", "[filter-border-width:1px]",
			"[filter-brightness:1.2]", "[filter-contrast:1.2]", "[filter-drop-shadow:0_0_12px_black]",
			"[filter-hue-rotate:10deg]", "[filter-invert:1]", "[filter-saturate:1.5]", "[filter-sepia:1]",
			"[filter-tint:1]", "[flex-basis:10px]", "[flex-direction:row]", "[flex-grow:1]",
			"[flex-shrink:1]", "[flex-wrap:wrap]", "[font-color:#fff]", "[font-family:Poppins]",
			"[font-size:16px]", "[font-smooth:always]", "[font-style:italic]", "[font-variant-numeric:tabular-nums]",
			"[font-weight:600]", "[gap:8px_12px]", "[height:10px]", "[image-rendering:pixelated]",
			"[justify-content:center]", "[left:10px]", "[letter-spacing:1px]", "[line-height:20px]",
			"[margin:10px]", "[margin-bottom:10px]", "[margin-left:10px]", "[margin-right:10px]",
			"[margin-top:10px]", "[mask:url(/ui/mask.png)]", "[mask-angle:10deg]", "[mask-image:url(/ui/mask.png)]",
			"[mask-mode:alpha]", "[mask-position:10px_15px]", "[mask-position-x:10px]", "[mask-position-y:15px]",
			"[mask-repeat:no-repeat]", "[mask-scope:filter]", "[mask-size:10px_15px]", "[mask-size-x:10px]",
			"[mask-size-y:15px]", "[max-height:100px]", "[max-width:100px]", "[min-height:10px]",
			"[min-width:10px]", "[mix-blend-mode:multiply]", "[opacity:0.5]", "[order:1]",
			"[overflow:hidden]", "[overflow-x:scroll]", "[overflow-y:visible]", "[padding:10px]",
			"[padding-bottom:10px]", "[padding-left:10px]", "[padding-right:10px]", "[padding-top:10px]",
			"[perspective-origin:50%_50%]", "[perspective-origin-x:50%]", "[perspective-origin-y:50%]",
			"[pointer-events:auto]", "[position:relative]", "[right:10px]", "[row-gap:10px]",
			"[sound-in:hover.wav]", "[sound-out:leave.wav]", "[text-align:center]", "[text-background-angle:10deg]",
			"[text-decoration:underline]", "[text-decoration-color:#fff]", "[text-decoration-line:underline]",
			"[text-decoration-line-through-offset:2px]", "[text-decoration-overline-offset:2px]",
			"[text-decoration-skip-ink:all]", "[text-decoration-thickness:2px]", "[text-decoration-underline-offset:2px]",
			"[text-overflow:ellipsis]", "[text-shadow:0_0_12px_black]", "[text-stroke:1px_#fff]",
			"[text-stroke-color:#fff]", "[text-stroke-width:1px]", "[text-transform:uppercase]",
			"[top:10px]", "[transform:scale(1)]", "[transform-origin:50%_50%]", "[transform-origin-x:50%]",
			"[transform-origin-y:50%]", "[transition:all_0.1s_ease]", "[transition-delay:0.1s]",
			"[transition-duration:0.1s]", "[transition-property:opacity]", "[transition-timing-function:ease-out]",
			"[white-space:pre]", "[width:10px]", "[word-break:break-all]", "[word-spacing:1px]",
			"[z-index:5]"
		};

		var result = GenerateSafelist( classes );

		Assert.AreEqual( classes.Length, result.GeneratedClassCount );
		Assert.AreEqual( 0, result.SkippedClassCount, string.Join( Environment.NewLine, result.Skipped.Select( item => $"{item.ClassName}: {item.Detail}" ) ) );
	}

	[TestMethod]
	public void ThemeBucketsCanOverrideTailwindLikeTokens()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			config.LineHeights["panel"] = "30px";
			config.FontFamilies["display"] = "Poppins";
			config.Safelist.Add( "text-lg/panel font-display" );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );

			StringAssert.Contains( result.GeneratedScss, "line-height: 30px;" );
			StringAssert.Contains( result.GeneratedScss, "font-family: Poppins;" );
			Assert.AreEqual( 2, result.GeneratedClassCount );
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
