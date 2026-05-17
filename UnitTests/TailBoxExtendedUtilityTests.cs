using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxExtendedUtilityTests
{
	[DataTestMethod]
	[DataRow( "flex-auto", "flex-grow: 1;" )]
	[DataRow( "flex-none", "flex-shrink: 0;" )]
	[DataRow( "basis-[33px]", "flex-basis: 33px;" )]
	[DataRow( "order-3", "order: 3;" )]
	[DataRow( "-order-2", "order: -2;" )]
	[DataRow( "h-full", "height: 100%;" )]
	[DataRow( "min-w-0", "min-width: 0;" )]
	[DataRow( "min-w-[220px]", "min-width: 220px;" )]
	[DataRow( "min-h-[24px]", "min-height: 24px;" )]
	[DataRow( "max-w-screen", "max-width: 100vw;" )]
	[DataRow( "aspect-square", "aspect-ratio: 1;" )]
	[DataRow( "aspect-[4/3]", "aspect-ratio: 4/3;" )]
	public void GeneratesAdditionalLayoutAndSizingUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "border-x", "border-left: 1px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-y-accent", "border-bottom: 1px solid #d7b46a;" )]
	[DataRow( "border-l-[3px]", "border-left: 3px solid rgba( 139, 154, 164, 0.32 );" )]
	[DataRow( "border-[3px]", "border-width: 3px;" )]
	[DataRow( "border-[#123456]/25", "border-color: rgba( 18, 52, 86, 0.25 );" )]
	[DataRow( "rounded-br-md", "border-radius: 0px 0px 8px 0px;" )]
	[DataRow( "rounded-s-lg", "border-radius: 12px 0px 0px 12px;" )]
	[DataRow( "rounded-ee-[9px]", "border-radius: 0px 0px 9px 0px;" )]
	public void GeneratesAdditionalBorderAndRadiusUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "text-right", "text-align: right;" )]
	[DataRow( "lowercase", "text-transform: lowercase;" )]
	[DataRow( "capitalize", "text-transform: capitalize;" )]
	[DataRow( "text-[length:22px]/7", "line-height: 28px;" )]
	[DataRow( "font-[Roboto_Slab]", "font-family: Roboto Slab;" )]
	[DataRow( "-tracking-wide", "letter-spacing: -0.025em;" )]
	[DataRow( "decoration-[3px]", "text-decoration-thickness: 3px;" )]
	[DataRow( "underline-offset-4", "text-decoration-underline-offset: 4px;" )]
	[DataRow( "-underline-offset-2", "text-decoration-underline-offset: -2px;" )]
	[DataRow( "text-[color:#123456]/50", "color: rgba( 18, 52, 86, 0.5 );" )]
	[DataRow( "text-accent/50", "color: rgba( 215, 180, 106, 0.5 );" )]
	public void GeneratesAdditionalTypographyUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "bg-contain", "background-size: contain;" )]
	[DataRow( "bg-center", "background-position: center;" )]
	[DataRow( "bg-no-repeat", "background-repeat: no-repeat;" )]
	[DataRow( "bg-[linear-gradient(red,_blue)]", "background-image: linear-gradient(red, blue);" )]
	[DataRow( "bg-[image:url(/ui/panel.png)]", "background-image: url(/ui/panel.png);" )]
	public void GeneratesAdditionalBackgroundSurfaceUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "opacity-75", "opacity: 0.75;" )]
	[DataRow( "opacity-[0.35]", "opacity: 0.35;" )]
	[DataRow( "shadow", "box-shadow: 0 12px 32px rgba( 0, 0, 0, 0.34 );" )]
	[DataRow( "shadow-none", "box-shadow: none;" )]
	[DataRow( "shadow-[0_0_12px_black]", "box-shadow: 0 0 12px black;" )]
	[DataRow( "text-shadow", "text-shadow: 0 2px 4px rgba( 0, 0, 0, 0.4 );" )]
	[DataRow( "text-shadow-[0_0_12px_black]", "text-shadow: 0 0 12px black;" )]
	[DataRow( "blur", "filter-blur: 8px;" )]
	[DataRow( "contrast-125", "filter-contrast: 1.25;" )]
	[DataRow( "saturate-[1.25]", "filter-saturate: 1.25;" )]
	[DataRow( "-hue-rotate-30", "filter-hue-rotate: -30deg;" )]
	[DataRow( "invert", "filter-invert: 1;" )]
	[DataRow( "grayscale-0", "filter-saturate: 0;" )]
	[DataRow( "drop-shadow-sm", "filter-drop-shadow: 0 2px 8px rgba( 0, 0, 0, 0.24 );" )]
	[DataRow( "backdrop-brightness-125", "backdrop-filter-brightness: 1.25;" )]
	[DataRow( "backdrop-blur-sm", "backdrop-filter-blur: 4px;" )]
	[DataRow( "mix-blend-multiply", "mix-blend-mode: multiply;" )]
	public void GeneratesAdditionalEffectsUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "transition", "transition-property: all;" )]
	[DataRow( "transition-all", "transition-property: all;" )]
	[DataRow( "transition-colors", "transition-property: color, background-color, border-color, text-decoration-color;" )]
	[DataRow( "transition-opacity", "transition-property: opacity;" )]
	[DataRow( "transition-shadow", "transition-property: box-shadow, text-shadow, filter-drop-shadow;" )]
	[DataRow( "transition-transform", "transition-property: transform;" )]
	[DataRow( "duration-[275ms]", "transition-duration: 275ms;" )]
	[DataRow( "delay-300", "transition-delay: 0.3s;" )]
	[DataRow( "ease-[cubic-bezier(0.2,_0,_0,_1)]", "transition-timing-function: cubic-bezier(0.2, 0, 0, 1);" )]
	[DataRow( "transform-none", "transform: none;" )]
	[DataRow( "transform-[translateX(4px)_scale(1.04)]", "transform: translateX(4px) scale(1.04);" )]
	[DataRow( "origin-bottom-right", "transform-origin: bottom right;" )]
	[DataRow( "animate-none", "animation: none;" )]
	[DataRow( "animate-[fade_1s_ease]", "animation: fade 1s ease;" )]
	public void GeneratesMotionUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "fixed", TailBoxSkipReason.UnsupportedValue )]
	[DataRow( "space-x-4", TailBoxSkipReason.UnsupportedSelectorVariant )]
	[DataRow( "bg-gradient-to-r", TailBoxSkipReason.UnsupportedUtility )]
	[DataRow( "bg-[paint-token]", TailBoxSkipReason.UnsupportedProperty )]
	[DataRow( "border-dashed", TailBoxSkipReason.UnsupportedProperty )]
	[DataRow( "decoration-wavy", TailBoxSkipReason.UnsupportedProperty )]
	[DataRow( "text-lg/unknown", TailBoxSkipReason.UnsupportedModifier )]
	[DataRow( "text-accent/not-real", TailBoxSkipReason.UnsupportedModifier )]
	[DataRow( "animate-spin", TailBoxSkipReason.UnsupportedUtility )]
	[DataRow( "translate-x-4", TailBoxSkipReason.UnsupportedUtility )]
	[DataRow( "touch-pan-x", TailBoxSkipReason.UnsupportedProperty )]
	public void ReportsAdditionalUnsupportedUtilitiesWithStableReasons( string className, TailBoxSkipReason reason )
	{
		var result = GenerateSafelist( className );

		Assert.AreEqual( 0, result.GeneratedClassCount );
		AssertSkip( result, className, reason );
		Assert.IsTrue( result.Warnings.Single().StartsWith( className + ":", StringComparison.Ordinal ) );
	}

	[TestMethod]
	public void ImportantArbitraryPropertyComposesInOneRule()
	{
		var result = GenerateSafelist( "![z-index:5]" );

		StringAssert.Contains( result.GeneratedScss, RuleStart( "![z-index:5]" ) );
		StringAssert.Contains( result.GeneratedScss, "z-index: 5 !important;" );
		Assert.AreEqual( 1, result.GeneratedClassCount );
	}

	private static void AssertUtility( string className, string declaration )
	{
		var result = GenerateSafelist( className );

		StringAssert.Contains( result.GeneratedScss, RuleStart( className ) );
		StringAssert.Contains( result.GeneratedScss, declaration );
		Assert.AreEqual( 1, result.GeneratedClassCount, $"Expected one generated rule for {className}." );
		Assert.AreEqual( 0, result.SkippedClassCount, $"Expected no skipped classes for {className}." );
		Assert.AreEqual( 0, result.Warnings.Count, $"Expected no warnings for {className}." );
	}

	private static void AssertSkip( TailBoxGenerationResult result, string className, TailBoxSkipReason reason )
	{
		var skipped = result.Skipped.SingleOrDefault( item => item.ClassName == className );
		Assert.IsNotNull( skipped, $"Expected '{className}' to be skipped." );
		Assert.AreEqual( reason, skipped.Reason, $"Unexpected skip reason for '{className}': {skipped.Detail}" );
		Assert.IsFalse( string.IsNullOrWhiteSpace( skipped.Detail ) );
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

	private static string RuleStart( string className )
	{
		Assert.IsTrue( TailBoxCandidateParser.TryParse( className, out var candidate, out _ ), $"Expected '{className}' to parse." );
		return "." + TailBoxUtilityCompiler.EscapeClassSelector( className )
			+ string.Concat( candidate.Variants.Select( variant => variant.SelectorSuffix ) )
			+ " {";
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
