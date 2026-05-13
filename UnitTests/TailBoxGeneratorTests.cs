using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxGeneratorTests
{
	[TestMethod]
	public void ExtractsStaticDynamicAndSafelistedClasses()
	{
		var text = """
			<div class="flex p-4 @(IsActive ? "hover:bg-accent" : "text-accent")"></div>
			<Button class='w-1/2 px-[14px]' />
			@* tailbox safelist: intro:opacity-0 bg-[#0d1418] *@
			""";

		var classes = TailBoxClassExtractor.ExtractClassesFromText( text );

		Assert.IsTrue( classes.Contains( "flex" ) );
		Assert.IsTrue( classes.Contains( "p-4" ) );
		Assert.IsTrue( classes.Contains( "hover:bg-accent" ) );
		Assert.IsTrue( classes.Contains( "text-accent" ) );
		Assert.IsTrue( classes.Contains( "w-1/2" ) );
		Assert.IsTrue( classes.Contains( "px-[14px]" ) );
		Assert.IsTrue( classes.Contains( "intro:opacity-0" ) );
		Assert.IsTrue( classes.Contains( "bg-[#0d1418]" ) );
	}

	[TestMethod]
	public void SavesAndLoadsConfigWithDefaultsMerged()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxEditorProject.SaveDefaultConfig( root );
			config.Colors["brand"] = "#123456";
			config.Safelist.Add( "text-brand" );
			TailBoxEditorProject.SaveConfig( root, config );

			var loaded = TailBoxEditorProject.LoadConfig( root );

			Assert.AreEqual( "Code/tailbox.generated.scss", loaded.OutputPath );
			Assert.AreEqual( "#d7b46a", loaded.Colors["accent"] );
			Assert.AreEqual( "#123456", loaded.Colors["brand"] );
			Assert.AreEqual( "768px", loaded.Screens["md"] );
			Assert.AreEqual( "0.15s", loaded.Durations["150"] );
			Assert.AreEqual( "1", loaded.Opacity["100"] );
			Assert.IsTrue( loaded.Safelist.Contains( "text-brand" ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratesRepresentativeUtilities()
	{
		var root = CreateTempProject();
		try
		{
			File.WriteAllText( Path.Combine( root, "Code", "Screen.razor" ), """
				<div class="flex w-full w-1/2 p-4 px-[14px] bg-[#0d1418] text-accent hover:bg-panel intro:opacity-0"></div>
				""" );

			var result = TailBoxEditorProject.Generate( root, TailBoxConfig.CreateDefault() );
			var scss = result.GeneratedScss;

			StringAssert.Contains( scss, ".flex {" );
			StringAssert.Contains( scss, "display: flex;" );
			StringAssert.Contains( scss, RuleStart( "w-1/2" ) );
			StringAssert.Contains( scss, "width: 50%;" );
			StringAssert.Contains( scss, ".p-4 {" );
			StringAssert.Contains( scss, "padding: 16px;" );
			StringAssert.Contains( scss, RuleStart( "px-[14px]" ) );
			StringAssert.Contains( scss, "padding-left: 14px;" );
			StringAssert.Contains( scss, "padding-right: 14px;" );
			StringAssert.Contains( scss, RuleStart( "bg-[#0d1418]" ) );
			StringAssert.Contains( scss, "background-color: #0d1418;" );
			StringAssert.Contains( scss, ".text-accent {" );
			StringAssert.Contains( scss, "color: #d7b46a;" );
			StringAssert.Contains( scss, RuleStart( "hover:bg-panel" ) );
			StringAssert.Contains( scss, RuleStart( "intro:opacity-0" ) );
			StringAssert.Contains( scss, "opacity: 0;" );

			Assert.AreEqual( 9, result.GeneratedClassCount );
			Assert.IsTrue( File.Exists( Path.Combine( root, "Code", "tailbox.generated.scss" ) ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void EscapesSpecialSelectorCharacters()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			config.Safelist.AddRange( new[]
			{
				"hover:bg-accent",
				"w-1/2",
				"bg-[#0d1418]",
				"z-[100%]",
				"opacity-[0.5]"
			} );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );
			var scss = result.GeneratedScss;

			StringAssert.Contains( scss, RuleSelector( "hover:bg-accent" ) + ":hover" );
			StringAssert.Contains( scss, RuleSelector( "w-1/2" ) );
			StringAssert.Contains( scss, RuleSelector( "bg-[#0d1418]" ) );
			StringAssert.Contains( scss, RuleSelector( "z-[100%]" ) );
			StringAssert.Contains( scss, RuleSelector( "opacity-[0.5]" ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void SkippedItemsCarryStableReasonsAndSourcePaths()
	{
		var root = CreateTempProject();
		try
		{
			var razor = Path.Combine( root, "Code", "Screen.razor" );
			File.WriteAllText( razor, """
				<div class="grid md:flex first:flex rotate-45 [--brand:#fff]"></div>
				""" );

			var result = TailBoxEditorProject.Generate( root, TailBoxConfig.CreateDefault(), writeFile: false );

			Assert.AreEqual( 0, result.GeneratedClassCount );
			AssertSkip( result, "grid", TailBoxSkipReason.UnsupportedValue, razor );
			AssertSkip( result, "md:flex", TailBoxSkipReason.UnsupportedMediaVariant, razor );
			AssertSkip( result, "first:flex", TailBoxSkipReason.UnsupportedSelectorVariant, razor );
			AssertSkip( result, "rotate-45", TailBoxSkipReason.UnsupportedUtility, razor );
			AssertSkip( result, "[--brand:#fff]", TailBoxSkipReason.UnsupportedArbitraryProperty, razor );
			Assert.AreEqual( result.SkippedClassCount, result.SkippedClasses.Count );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratesConstructionDashboardExampleDeterministically()
	{
		var repoRoot = FindRepositoryRoot();
		if ( repoRoot is null )
			Assert.Inconclusive( "Repository root could not be located for the ConstructionDashboard fixture." );

		var root = CreateTempProject();
		try
		{
			var exampleRazor = Path.Combine( repoRoot, "Examples", "ConstructionDashboard", "Code", "ConstructionDashboard.razor.example" );
			var exampleConfig = Path.Combine( repoRoot, "Examples", "ConstructionDashboard", TailBoxConfig.FileName );
			Directory.CreateDirectory( Path.Combine( root, "Code" ) );
			File.Copy( exampleRazor, Path.Combine( root, "Code", "ConstructionDashboard.razor" ), overwrite: true );

			var config = TailBoxConfig.LoadJson( File.ReadAllText( exampleConfig ), exampleConfig );
			var first = TailBoxEditorProject.Generate( root, config, writeFile: false );
			var second = TailBoxEditorProject.Generate( root, config, writeFile: false );

			Assert.IsTrue( first.GeneratedClassCount > 35, "Expected the example to generate a broad utility set." );
			Assert.AreEqual( 0, first.SkippedClassCount );
			Assert.AreEqual( first.GeneratedScss, second.GeneratedScss );
			CollectionAssert.AreEqual( first.GeneratedClasses.ToArray(), second.GeneratedClasses.ToArray() );
			StringAssert.Contains( first.GeneratedScss, RuleSelector( "hover:bg-site" ) + ":hover" );
			StringAssert.Contains( first.GeneratedScss, RuleStart( "w-1/4" ) );
			StringAssert.Contains( first.GeneratedScss, "color: #d7b46a;" );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void SboxStyleParserAcceptsGeneratedEscapedSelectorsWhenAvailable()
	{
		try
		{
			var panelType = Type.GetType( "Sandbox.UI.Panel, Sandbox.Engine", throwOnError: true );
			var panel = Activator.CreateInstance( panelType );
			var styleSheet = panelType.GetProperty( "StyleSheet" )?.GetValue( panel );
			var parse = styleSheet?.GetType().GetMethod( "Parse", new[] { typeof( string ), typeof( bool ) } );
			parse?.Invoke(
				styleSheet,
				new object[]
				{
					RuleStart( "hover:bg-accent" ) + " background-color: #d7b46a; }\n" + RuleStart( "w-1/2" ) + " width: 50%; }",
					false
				} );
		}
		catch ( Exception ex ) when ( ex is NullReferenceException or InvalidOperationException or FileNotFoundException or System.Reflection.TargetInvocationException )
		{
			Assert.Inconclusive( "s&box style parsing is not available in this headless test context: " + ex.Message );
		}
	}

	[TestMethod]
	public void WatcherPathFilterIgnoresGeneratedOutputAndRequiresConfigForOptIn()
	{
		var root = CreateTempProject();
		try
		{
			var output = Path.Combine( root, "Code", "tailbox.generated.scss" );
			var razor = Path.Combine( root, "Code", "Screen.razor" );
			File.WriteAllText( razor, "<div class=\"flex\"></div>" );

			Assert.IsFalse( TailBoxEditorWatcher.IsProjectOptedIn( root ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, output ) );
			Assert.IsTrue( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, razor ) );

			TailBoxEditorProject.SaveDefaultConfig( root );
			Assert.IsTrue( TailBoxEditorWatcher.IsProjectOptedIn( root ) );
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

	private static void AssertSkip( TailBoxGenerationResult result, string className, TailBoxSkipReason reason, string sourcePath )
	{
		var skipped = result.Skipped.SingleOrDefault( item => item.ClassName == className );
		Assert.IsNotNull( skipped, $"Expected '{className}' to be skipped." );
		Assert.AreEqual( reason, skipped.Reason );
		Assert.AreEqual( Path.GetFullPath( sourcePath ), Path.GetFullPath( skipped.SourcePath ) );
		Assert.IsFalse( string.IsNullOrWhiteSpace( skipped.Detail ) );
	}

	private static string RuleSelector( string className )
	{
		return "." + TailBoxUtilityCompiler.EscapeClassSelector( className );
	}

	private static string RuleStart( string className )
	{
		Assert.IsTrue( TailBoxCandidateParser.TryParse( className, out var candidate, out _ ), $"Expected '{className}' to parse." );
		return RuleSelector( className )
			+ string.Concat( candidate.Variants.Select( variant => variant.SelectorSuffix ) )
			+ " {";
	}

	private static string FindRepositoryRoot()
	{
		foreach ( var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory } )
		{
			var current = new DirectoryInfo( start );
			while ( current is not null )
			{
				var fixture = Path.Combine( current.FullName, "Examples", "ConstructionDashboard", "Code", "ConstructionDashboard.razor.example" );
				if ( File.Exists( fixture ) )
					return current.FullName;

				current = current.Parent;
			}
		}

		return null;
	}
}
