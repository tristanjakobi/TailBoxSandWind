using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxArchitectureTests
{
	[TestMethod]
	public void DomainLayerDoesNotDependOnApplicationOrInfrastructure()
	{
		var root = FindRepositoryRoot();
		if ( root is null )
			Assert.Inconclusive( "Repository root could not be located for architecture boundary checks." );

		var forbidden = new[]
		{
			"TailBoxConfig",
			"TailBoxGenerationPipeline",
			"TailBoxGenerationResult",
			"TailBoxGenerator",
			"TailBoxSourceText",
			"TailBoxClassExtractor",
			"TailBoxThemeFactory",
			"TailBoxSkippedClass",
			"TailBoxEditor",
			"TailBoxProjectFileSystem",
			"SourcePath",
			"ToScss",
			"System.IO",
			"File.",
			"Directory.",
			"Path.",
			"SearchOption",
			"DirectoryInfo",
			"FileInfo"
		};

		AssertNoForbiddenReferences( Path.Combine( root, "Code", "TailBox", "Domain" ), forbidden );
	}

	[TestMethod]
	public void ApplicationLayerDoesNotDependOnInfrastructure()
	{
		var root = FindRepositoryRoot();
		if ( root is null )
			Assert.Inconclusive( "Repository root could not be located for architecture boundary checks." );

		var forbidden = new[]
		{
			"TailBoxEditor",
			"TailBoxProjectFileSystem",
			"System.IO",
			"File.",
			"Directory.",
			"Path.",
			"SearchOption",
			"DirectoryInfo",
			"FileInfo"
		};

		AssertNoForbiddenReferences( Path.Combine( root, "Code", "TailBox", "Application" ), forbidden );
	}

	private static void AssertNoForbiddenReferences( string directory, string[] forbidden )
	{
		foreach ( var file in Directory.GetFiles( directory, "*.cs", SearchOption.AllDirectories ) )
		{
			var text = File.ReadAllText( file );
			foreach ( var token in forbidden )
			{
				Assert.IsFalse(
					text.Contains( token, StringComparison.Ordinal ),
					$"{Path.GetRelativePath( Directory.GetCurrentDirectory(), file )} should not reference '{token}'." );
			}
		}
	}

	private static string FindRepositoryRoot()
	{
		foreach ( var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory } )
		{
			var current = new DirectoryInfo( start );
			while ( current is not null )
			{
				var marker = Path.Combine( current.FullName, "Code", "TailBox", "Domain", "README.md" );
				if ( File.Exists( marker ) )
					return current.FullName;

				current = current.Parent;
			}
		}

		return null;
	}
}
