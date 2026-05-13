using Sandbox.TailBox;
using System;
using System.IO;
using System.Linq;

[TestClass]
public sealed class TailBoxFileSystemTests
{
	[TestMethod]
	public void GlobMatcherHandlesDoubleStarQuestionAndBackslashes()
	{
		var razorGlob = TailBoxGlobMatcher.FromGlob( "Code/**/*.razor" );
		Assert.IsTrue( razorGlob.IsMatch( "Code/Screen.razor" ) );
		Assert.IsTrue( razorGlob.IsMatch( "Code/Ui/Nested/Screen.razor" ) );
		Assert.IsFalse( razorGlob.IsMatch( "Code/Screen.razor.scss" ) );

		var questionGlob = TailBoxGlobMatcher.FromGlob( "Ui/Panel?.razor" );
		Assert.IsTrue( questionGlob.IsMatch( "Ui/Panel1.razor" ) );
		Assert.IsFalse( questionGlob.IsMatch( "Ui/Panel10.razor" ) );

		var slashGlob = TailBoxGlobMatcher.FromGlob( "\\Code\\*.razor" );
		Assert.IsTrue( slashGlob.IsMatch( "Code\\Panel.razor" ) );
	}

	[TestMethod]
	public void TryGetRelativeProjectPathNormalizesInsidePathsAndRejectsOutsidePaths()
	{
		var root = CreateTempProject();
		try
		{
			var inside = Path.Combine( root, "Code", "Screen.razor" );
			var outside = Path.Combine( Path.GetDirectoryName( root )!, Guid.NewGuid().ToString( "N" ), "Screen.razor" );

			Assert.IsTrue( TailBoxProjectFileSystem.TryGetRelativeProjectPath( root, inside, out var relative ) );
			Assert.AreEqual( "Code/Screen.razor", relative );
			Assert.IsFalse( TailBoxProjectFileSystem.TryGetRelativeProjectPath( root, outside, out _ ) );
			Assert.IsFalse( TailBoxProjectFileSystem.TryGetRelativeProjectPath( "", inside, out _ ) );
			Assert.IsFalse( TailBoxProjectFileSystem.TryGetRelativeProjectPath( root, "", out _ ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void FindContentFilesHonorsGlobsAndSkipsGeneratedHiddenAndBuildFolders()
	{
		var root = CreateTempProject();
		try
		{
			WriteFile( root, "Code/Screen.razor", "<div />" );
			WriteFile( root, "Ui/Nested/Panel.razor", "<div />" );
			WriteFile( root, "Code/Screen.razor.scss", ".ignored {}" );
			WriteFile( root, "Code/tailbox.generated.scss", ".ignored {}" );
			WriteFile( root, "Code/bin/Skip.razor", "<div />" );
			WriteFile( root, "obj/Skip.razor", "<div />" );
			WriteFile( root, ".vs/Skip.razor", "<div />" );
			WriteFile( root, ".sbox/Skip.razor", "<div />" );
			WriteFile( root, ".git/Skip.razor", "<div />" );

			var config = TailBoxConfig.CreateDefault();
			config.Content.Clear();
			config.Content.Add( "Code/**/*.razor" );
			config.Content.Add( "Ui/**/*.razor" );
			var output = Path.Combine( root, "Code", "tailbox.generated.scss" );

			var files = TailBoxProjectFileSystem.FindContentFiles( root, config, output )
				.Select( file => Path.GetRelativePath( root, file ).Replace( '\\', '/' ) )
				.ToHashSet( StringComparer.Ordinal );

			CollectionAssert.AreEquivalent( new[] { "Code/Screen.razor", "Ui/Nested/Panel.razor" }, files.ToArray() );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void WriteIfChangedCreatesDirectoriesAndReportsChangeState()
	{
		var root = CreateTempProject();
		try
		{
			var output = Path.Combine( root, "Assets", "Generated", "tailbox.scss" );

			Assert.IsTrue( TailBoxProjectFileSystem.WriteIfChanged( output, "first" ) );
			Assert.IsFalse( TailBoxProjectFileSystem.WriteIfChanged( output, "first" ) );
			Assert.IsTrue( TailBoxProjectFileSystem.WriteIfChanged( output, "second" ) );
			Assert.AreEqual( "second", File.ReadAllText( output ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void ResolveOutputPathUsesDefaultsForNullOrBlankConfig()
	{
		var root = CreateTempProject();
		try
		{
			var expected = Path.GetFullPath( Path.Combine( root, "Code", "tailbox.generated.scss" ) );
			Assert.AreEqual( expected, TailBoxProjectFileSystem.ResolveOutputPath( root, null ) );

			var config = TailBoxConfig.CreateDefault();
			config.OutputPath = " ";
			Assert.AreEqual( expected, TailBoxProjectFileSystem.ResolveOutputPath( root, config ) );
		}
		finally
		{
			DeleteTempProject( root );
		}
	}

	[TestMethod]
	public void ShouldSkipPathRejectsOutputHiddenBuildAndOutsidePaths()
	{
		var root = CreateTempProject();
		try
		{
			var output = Path.Combine( root, "Code", "tailbox.generated.scss" );
			var ordinary = Path.Combine( root, "Code", "Screen.razor" );
			var outside = Path.Combine( Path.GetDirectoryName( root )!, Guid.NewGuid().ToString( "N" ), "Screen.razor" );

			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( root, output, output ) );
			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( root, output, Path.Combine( root, ".git", "Screen.razor" ) ) );
			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( root, output, Path.Combine( root, ".sbox", "Screen.razor" ) ) );
			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( root, output, Path.Combine( root, "bin", "Screen.razor" ) ) );
			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( root, output, outside ) );
			Assert.IsTrue( TailBoxProjectFileSystem.ShouldSkipPath( "", output, ordinary ) );
			Assert.IsFalse( TailBoxProjectFileSystem.ShouldSkipPath( root, output, ordinary ) );
		}
		finally
		{
			DeleteTempProject( root );
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
