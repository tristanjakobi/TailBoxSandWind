using System;
using System.IO;

internal static class TailBoxTestPaths
{
	public static string CreateTempProject()
	{
		var baseRoot = Environment.GetEnvironmentVariable( "TAILBOX_TEST_ROOT" );
		if ( string.IsNullOrWhiteSpace( baseRoot ) )
			baseRoot = Path.Combine( Path.GetTempPath(), "tailbox-tests" );

		try
		{
			Directory.CreateDirectory( baseRoot );
		}
		catch ( UnauthorizedAccessException )
		{
			baseRoot = Path.Combine( Directory.GetCurrentDirectory(), ".tailbox-tests" );
			Directory.CreateDirectory( baseRoot );
		}

		var root = Path.Combine( baseRoot, Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( Path.Combine( root, "Code" ) );
		return root;
	}
}
