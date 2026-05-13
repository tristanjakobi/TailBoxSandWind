using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxDemoTests
{
	[TestMethod]
	public void DemoMenuGeneratesBroadUtilitySetWithoutStructuredSkips()
	{
		var repoRoot = FindRepositoryRoot();
		if ( repoRoot is null )
			Assert.Inconclusive( "Repository root could not be located for the TailBox demo fixture." );

		var demoPath = Path.Combine( repoRoot, "Code", "Demo", "TailBoxDemoMenu.razor" );
		var result = new TailBoxGenerator().GenerateFromSources(
			new[] { new TailBoxSourceText( demoPath, File.ReadAllText( demoPath ) ) },
			TailBoxConfig.CreateDefault(),
			repoRoot,
			"Code/tailbox.generated.scss" );

		Assert.IsTrue( result.GeneratedClassCount > 130, "Expected the demo to exercise a broad utility set." );
		Assert.AreEqual( 0, result.SkippedClassCount, string.Join( Environment.NewLine, result.Skipped.Select( item => $"{item.ClassName}: {item.Detail}" ) ) );
		StringAssert.Contains( result.GeneratedScss, RuleSelector( "hover:bg-accent" ) + ":hover" );
		StringAssert.Contains( result.GeneratedScss, RuleStart( "transform-[rotate(2deg)_scale(0.98)]" ) );
		StringAssert.Contains( result.GeneratedScss, "transform: rotate(2deg) scale(0.98);" );
		StringAssert.Contains( result.GeneratedScss, RuleStart( "[text-shadow:0_2px_8px_rgba(0,0,0,0.45)]" ) );
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
				var fixture = Path.Combine( current.FullName, "Code", "Demo", "TailBoxDemoMenu.razor" );
				if ( File.Exists( fixture ) )
					return current.FullName;

				current = current.Parent;
			}
		}

		return null;
	}
}
