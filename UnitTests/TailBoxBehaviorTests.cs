using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxBehaviorTests
{
	[TestMethod]
	public void ExtractorReadsComponentClassAttributesAndConditionalStringLiterals()
	{
		var text = """
			<Panel Class="flex items-center">
				<Button class=@(IsReady ? "bg-good text-white" : "bg-danger text-muted") />
				<div class='px-4 py-2'></div>
			</Panel>
			""";

		var classes = TailBoxClassExtractor.ExtractClassesFromText( text );

		AssertContainsAll( classes, "flex", "items-center", "bg-good", "text-white", "bg-danger", "text-muted", "px-4", "py-2" );
	}

	[TestMethod]
	public void ExtractorReadsSafelistCommentsInRazorAndHtml()
	{
		var text = """
			@* tailbox safelist: intro:opacity-0 bg-[#0d1418] *@
			<!-- tailbox safelist: hover:bg-accent active:bg-panel -->
			""";

		var classes = TailBoxClassExtractor.ExtractClassesFromText( text );

		AssertContainsAll( classes, "intro:opacity-0", "bg-[#0d1418]", "hover:bg-accent", "active:bg-panel" );
	}

	[TestMethod]
	public void ExtractorCleansPunctuationAndIgnoresRazorExpressions()
	{
		var text = """
			<div class="flex, p-4; @ComputedClass"></div>
			<span class="hover:bg-accent) (text-muted"></span>
			""";

		var classes = TailBoxClassExtractor.ExtractClassesFromText( text );

		AssertContainsAll( classes, "flex", "p-4", "hover:bg-accent", "text-muted" );
		Assert.IsFalse( classes.Contains( "@ComputedClass" ) );
	}

	[TestMethod]
	public void GeneratorUsesConfigSafelistAndDeduplicatesClasses()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, "Code/Screen.razor", "<div class=\"flex flex\"></div>" );
			var config = TailBoxConfig.CreateDefault();
			config.Safelist.Add( "flex p-4, hover:bg-accent" );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );

			Assert.AreEqual( 3, result.GeneratedClassCount );
			AssertContainsAll( result.GeneratedClasses, "flex", "p-4", "hover:bg-accent" );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratorWritesOnlyWhenOutputChanges()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, "Code/Screen.razor", "<div class=\"flex\"></div>" );
			var generator = new TailBoxGenerator();
			var config = TailBoxConfig.CreateDefault();

			var first = TailBoxEditorProject.Generate( root, config );
			var second = TailBoxEditorProject.Generate( root, config );
			config.Safelist.Add( "p-4" );
			var third = TailBoxEditorProject.Generate( root, config );

			Assert.IsTrue( first.WroteFile );
			Assert.IsFalse( second.WroteFile );
			Assert.IsTrue( third.WroteFile );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratorHonorsCustomContentGlobs()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, "Code/Screen.razor", "<div class=\"flex\"></div>" );
			WriteFile( root, "Ui/Nested/Screen.razor", "<div class=\"p-4\"></div>" );
			var config = TailBoxConfig.CreateDefault();
			config.Content.Clear();
			config.Content.Add( "Ui/**/*.razor" );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );

			Assert.AreEqual( 1, result.ScannedFileCount );
			AssertContainsAll( result.GeneratedClasses, "p-4" );
			Assert.IsFalse( result.GeneratedClasses.Contains( "flex" ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratorIgnoresGeneratedOutputAndBuildFoldersEvenWithBroadGlobs()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, "Code/Screen.razor", "<div class=\"flex\"></div>" );
			WriteFile( root, "Code/tailbox.generated.scss", "\"text-danger\"" );
			WriteFile( root, "Code/bin/Bogus.razor", "<div class=\"p-4\"></div>" );
			WriteFile( root, "Code/obj/Bogus.razor", "<div class=\"px-4\"></div>" );
			WriteFile( root, ".sbox/Bogus.razor", "<div class=\"py-4\"></div>" );
			var config = TailBoxConfig.CreateDefault();
			config.Content.Clear();
			config.Content.Add( "**/*" );

			var result = TailBoxEditorProject.Generate( root, config, writeFile: false );

			Assert.AreEqual( 1, result.GeneratedClassCount );
			AssertContainsAll( result.GeneratedClasses, "flex" );
			Assert.IsFalse( result.GeneratedClasses.Contains( "text-danger" ) );
			Assert.IsFalse( result.GeneratedClasses.Contains( "p-4" ) );
			Assert.IsFalse( result.GeneratedClasses.Contains( "px-4" ) );
			Assert.IsFalse( result.GeneratedClasses.Contains( "py-4" ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void GeneratorWritesCustomOutputPath()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			config.OutputPath = "Assets/Generated/tailbox.scss";
			config.Safelist.Add( "flex" );

			var result = TailBoxEditorProject.Generate( root, config );

			Assert.IsTrue( result.WroteFile );
			Assert.IsTrue( File.Exists( Path.Combine( root, "Assets", "Generated", "tailbox.scss" ) ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void SavedConfigUsesCamelCaseAndLoadsWithDefaultsMerged()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxEditorProject.SaveDefaultConfig( root );
			config.Colors["brand"] = "#123456";
			TailBoxEditorProject.SaveConfig( root, config );

			var json = File.ReadAllText( TailBoxEditorProject.GetConfigPath( root ) );
			var loaded = TailBoxEditorProject.LoadConfig( root );

			StringAssert.Contains( json, "\"outputPath\"" );
			StringAssert.Contains( json, "\"fontSizes\"" );
			Assert.AreEqual( "#123456", loaded.Colors["brand"] );
			Assert.AreEqual( "#d7b46a", loaded.Colors["accent"] );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void ConfigLoadAcceptsCamelCaseOverrides()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, TailBoxConfig.FileName, """
				{
				  "outputPath": "Code/generated/custom.scss",
				  "content": [ "Ui/**/*.razor" ],
				  "safelist": [ "text-brand" ],
				  "colors": {
				    "brand": "#123456"
				  }
				}
				""" );

			var loaded = TailBoxEditorProject.LoadConfig( root );

			Assert.AreEqual( "Code/generated/custom.scss", loaded.OutputPath );
			Assert.AreEqual( "Ui/**/*.razor", loaded.Content.Single() );
			Assert.AreEqual( "#123456", loaded.Colors["brand"] );
			Assert.AreEqual( "#d7b46a", loaded.Colors["accent"] );
			AssertContainsAll( loaded.Safelist, "text-brand" );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void ConfigResolvesRelativeAndAbsoluteOutputPaths()
	{
		var root = CreateTempProject();
		try
		{
			var config = TailBoxConfig.CreateDefault();
			Assert.AreEqual(
				Path.GetFullPath( Path.Combine( root, "Code", "tailbox.generated.scss" ) ),
				TailBoxEditorProject.ResolveOutputPath( root, config ) );

			var absolute = Path.Combine( root, "Custom", "tailbox.scss" );
			config.OutputPath = absolute;
			Assert.AreEqual( Path.GetFullPath( absolute ), TailBoxEditorProject.ResolveOutputPath( root, config ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void WatcherHandlesConfigAndRazorOnly()
	{
		var root = CreateTempProject();
		try
		{
			var output = Path.Combine( root, "Code", "tailbox.generated.scss" );

			Assert.IsTrue( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, TailBoxConfig.FileName ) ) );
			Assert.IsTrue( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, "Code", "Screen.razor" ) ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, "Code", "Screen.razor.scss" ) ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, output ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void WatcherIgnoresBuildHiddenAndExternalPaths()
	{
		var root = CreateTempProject();
		try
		{
			var output = Path.Combine( root, "Code", "tailbox.generated.scss" );
			var outside = Path.Combine( Path.GetDirectoryName( root )!, Guid.NewGuid().ToString( "N" ), "Screen.razor" );

			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, "bin", "Screen.razor" ) ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, "obj", "Screen.razor" ) ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, ".sbox", "Screen.razor" ) ) );
			Assert.IsFalse( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, outside ) );
			Assert.IsTrue( TailBoxEditorWatcher.ShouldHandleChangedPath( root, output, Path.Combine( root, "..generated", "Screen.razor" ) ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void WatcherOptInRequiresConfigFile()
	{
		var root = CreateTempProject();
		try
		{
			Assert.IsFalse( TailBoxEditorWatcher.IsProjectOptedIn( root ) );
			TailBoxEditorProject.SaveDefaultConfig( root );
			Assert.IsTrue( TailBoxEditorWatcher.IsProjectOptedIn( root ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	private static void AssertContainsAll( System.Collections.Generic.IEnumerable<string> values, params string[] expected )
	{
		var set = values.ToHashSet( StringComparer.Ordinal );
		foreach ( var item in expected )
		{
			Assert.IsTrue( set.Contains( item ), $"Expected '{item}' to be present." );
		}
	}

	private static void WriteFile( string root, string relativePath, string text )
	{
		var path = Path.Combine( root, relativePath.Replace( '/', Path.DirectorySeparatorChar ) );
		Directory.CreateDirectory( Path.GetDirectoryName( path )! );
		File.WriteAllText( path, text );
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
