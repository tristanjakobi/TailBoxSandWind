using Sandbox.TailBox;
using System;
using System.Linq;

[TestClass]
public sealed class TailBoxPipelineTests
{
	[TestMethod]
	public void GenerateFromSourcesHandlesNullSourcesAndConfig()
	{
		var result = new TailBoxGenerator().GenerateFromSources( null, null, "C:/Project", "C:/Project/Code/out.scss" );

		Assert.AreEqual( "C:/Project", result.ProjectRoot );
		Assert.AreEqual( "C:/Project/Code/out.scss", result.OutputPath );
		Assert.AreEqual( 0, result.ScannedFileCount );
		Assert.AreEqual( 0, result.DiscoveredClassCount );
		Assert.AreEqual( 0, result.GeneratedClassCount );
		Assert.AreEqual( 0, result.SkippedClassCount );
		Assert.IsFalse( result.WroteFile );
		StringAssert.Contains( result.GeneratedScss, "Source files scanned: 0" );
		StringAssert.Contains( result.GeneratedScss, "Config: tailwand.config.json" );
	}

	[TestMethod]
	public void GenerateFromSourcesDeduplicatesSortsAndKeepsFirstSourceForSkips()
	{
		var firstSource = "C:/Project/Code/First.razor";
		var secondSource = "C:/Project/Code/Second.razor";
		var sources = new[]
		{
			new TailBoxSourceText( firstSource, "<div class=\"p-4 grid flex\"></div>" ),
			new TailBoxSourceText( secondSource, "<div class=\"grid text-accent\"></div>" )
		};
		var config = TailBoxConfig.CreateDefault();
		config.Safelist.Add( "text-accent; flex p-4" );

		var result = new TailBoxGenerator().GenerateFromSources( sources, config, "C:/Project", "C:/Project/Code/out.scss" );

		CollectionAssert.AreEqual( new[] { "flex", "p-4", "text-accent" }, result.GeneratedClasses.ToArray() );
		CollectionAssert.AreEqual( new[] { "grid" }, result.SkippedClasses.ToArray() );
		Assert.AreEqual( 4, result.DiscoveredClassCount );
		Assert.AreEqual( firstSource, result.Skipped.Single().SourcePath );
		Assert.IsTrue( result.Warnings.Single().StartsWith( "grid:", StringComparison.Ordinal ) );
	}

	[TestMethod]
	public void GenerateFromSourcesRendersConfigFileNameAndScannedFileCount()
	{
		var config = TailBoxConfig.CreateDefault();
		config.ConfigPath = "C:/Project/Config/tailwand.custom.json";
		var sources = new[]
		{
			new TailBoxSourceText( "C:/Project/Code/Screen.razor", "<div class=\"flex\"></div>" ),
			new TailBoxSourceText( "C:/Project/Code/Empty.razor", "" )
		};

		var result = new TailBoxGenerator().GenerateFromSources( sources, config, "C:/Project" );

		Assert.AreEqual( 2, result.ScannedFileCount );
		StringAssert.Contains( result.GeneratedScss, "Source files scanned: 2" );
		StringAssert.Contains( result.GeneratedScss, "Config: tailwand.custom.json" );
		StringAssert.Contains( result.GeneratedScss, ".flex {" );
	}

	[TestMethod]
	public void ScanClassesFromSourcesSortsAndDeduplicates()
	{
		var sources = new[]
		{
			new TailBoxSourceText( "A.razor", "<div class=\"p-4 flex\"></div>" ),
			new TailBoxSourceText( "B.razor", "<div class=\"flex text-accent\"></div>" )
		};

		var classes = new TailBoxGenerator().ScanClassesFromSources( sources );

		CollectionAssert.AreEqual( new[] { "flex", "p-4", "text-accent" }, classes.ToArray() );
	}

	[TestMethod]
	public void SafelistSkipsHaveWarningsButNoSourcePath()
	{
		var config = TailBoxConfig.CreateDefault();
		config.Safelist.Add( "grid unknown:flex" );

		var result = new TailBoxGenerator().GenerateFromSources( Array.Empty<TailBoxSourceText>(), config );

		CollectionAssert.AreEqual( new[] { "grid", "unknown:flex" }, result.SkippedClasses.ToArray() );
		Assert.IsTrue( result.Skipped.All( item => item.SourcePath is null ) );
		Assert.IsTrue( result.Warnings.Any( warning => warning.StartsWith( "grid:", StringComparison.Ordinal ) ) );
		Assert.IsTrue( result.Warnings.Any( warning => warning.StartsWith( "unknown:flex:", StringComparison.Ordinal ) ) );
	}

	[TestMethod]
	public void ProjectPathGeneratorMethodsStayEditorOnly()
	{
		var generator = new TailBoxGenerator();
		var config = TailBoxConfig.CreateDefault();

		Assert.ThrowsException<NotSupportedException>( () => generator.Generate( "C:/Project" ) );
		Assert.ThrowsException<NotSupportedException>( () => generator.Generate( "C:/Project", config ) );
		Assert.ThrowsException<NotSupportedException>( () => generator.ScanClasses( "C:/Project", config ) );
		Assert.ThrowsException<NotSupportedException>( () => generator.FindContentFiles( "C:/Project", config ) );
	}
}
