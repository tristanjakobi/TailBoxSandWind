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
	[DataRow( "max-w-screen", "max-width: 100vw;" )]
	public void GeneratesAdditionalLayoutAndSizingUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "bg-contain", "background-size: contain;" )]
	[DataRow( "bg-center", "background-position: center;" )]
	[DataRow( "bg-no-repeat", "background-repeat: no-repeat;" )]
	[DataRow( "bg-[linear-gradient(red,_blue)]", "background-image: linear-gradient(red, blue);" )]
	[DataRow( "bg-[image:url(/ui/panel.png)]", "background-image: url(/ui/panel.png);" )]
	[DataRow( "bg-[paint-token]", "background: paint-token;" )]
	public void GeneratesAdditionalBackgroundUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "border-x", "border-left-width: 1px;" )]
	[DataRow( "border-y-accent", "border-bottom-color: #d7b46a;" )]
	[DataRow( "border-l-[3px]", "border-left-width: 3px;" )]
	[DataRow( "border-[#123456]/25", "border-color: rgba( 18, 52, 86, 0.25 );" )]
	[DataRow( "rounded-br-md", "border-bottom-right-radius: 8px;" )]
	public void GeneratesAdditionalBorderAndRadiusUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "text-right", "text-align: right;" )]
	[DataRow( "lowercase", "text-transform: lowercase;" )]
	[DataRow( "capitalize", "text-transform: capitalize;" )]
	[DataRow( "text-[color:#123456]/50", "color: rgba( 18, 52, 86, 0.5 );" )]
	[DataRow( "text-[length:22px]/7", "line-height: 28px;" )]
	[DataRow( "font-[Roboto_Slab]", "font-family: Roboto Slab;" )]
	[DataRow( "-tracking-wide", "letter-spacing: -0.025em;" )]
	[DataRow( "-underline-offset-2", "text-decoration-underline-offset: -2px;" )]
	public void GeneratesAdditionalTypographyUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "transition-transform", "transition-property: transform;" )]
	[DataRow( "duration-[275ms]", "transition-duration: 275ms;" )]
	[DataRow( "delay-300", "transition-delay: 0.3s;" )]
	[DataRow( "ease-[cubic-bezier(0.2,_0,_0,_1)]", "transition-timing-function: cubic-bezier(0.2, 0, 0, 1);" )]
	[DataRow( "blur", "filter-blur: 8px;" )]
	[DataRow( "backdrop-brightness-125", "backdrop-filter-brightness: 1.25;" )]
	[DataRow( "contrast-125", "filter-contrast: 1.25;" )]
	[DataRow( "saturate-[1.25]", "filter-saturate: 1.25;" )]
	[DataRow( "-hue-rotate-30", "filter-hue-rotate: -30deg;" )]
	[DataRow( "invert", "filter-invert: 1;" )]
	[DataRow( "grayscale-0", "filter-saturate: 0;" )]
	[DataRow( "drop-shadow-sm", "filter-drop-shadow: 0 2px 8px rgba( 0, 0, 0, 0.24 );" )]
	[DataRow( "transform-none", "transform: none;" )]
	[DataRow( "origin-bottom-right", "transform-origin: bottom right;" )]
	[DataRow( "animate-[fade_1s_ease]", "animation: fade 1s ease;" )]
	public void GeneratesAdditionalEffectsAndTransformUtilities( string className, string declaration )
	{
		AssertUtility( className, declaration );
	}

	[DataTestMethod]
	[DataRow( "fixed", TailBoxSkipReason.UnsupportedValue )]
	[DataRow( "space-x-4", TailBoxSkipReason.UnsupportedSelectorVariant )]
	[DataRow( "bg-gradient-to-r", TailBoxSkipReason.UnsupportedUtility )]
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
	public void ImportantVariantAndArbitraryPropertyComposeInOneRule()
	{
		var result = GenerateSafelist( "hover:![opacity:0.5]" );

		StringAssert.Contains( result.GeneratedScss, RuleStart( "hover:![opacity:0.5]" ) );
		StringAssert.Contains( result.GeneratedScss, "opacity: 0.5 !important;" );
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
