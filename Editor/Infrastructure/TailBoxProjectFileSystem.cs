using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandbox.TailBox;

internal static class TailBoxProjectFileSystem
{
	public static string NormalizeProjectRoot( string projectRoot )
	{
		return Path.GetFullPath( projectRoot );
	}

	public static string GetConfigPath( string projectRoot )
	{
		return Path.GetFullPath( GetActiveConfigPath( projectRoot ) );
	}

	public static bool ConfigExists( string projectRoot )
	{
		return File.Exists( GetConfigPath( projectRoot ) );
	}

	public static TailBoxConfig LoadConfig( string projectRoot )
	{
		var path = GetConfigPath( projectRoot );
		if ( !File.Exists( path ) )
		{
			var defaults = TailBoxConfig.CreateDefault();
			defaults.ConfigPath = path;
			return defaults;
		}

		return TailBoxConfig.LoadJson( File.ReadAllText( path ), path );
	}

	public static void SaveConfig( string projectRoot, TailBoxConfig config )
	{
		var path = GetConfigPath( projectRoot );
		Directory.CreateDirectory( Path.GetDirectoryName( path )! );
		File.WriteAllText( path, config.ToJson() );
		config.ConfigPath = path;
	}

	private static string GetActiveConfigPath( string projectRoot )
	{
		var modernPath = TailBoxConfig.GetConfigPath( projectRoot );
		if ( File.Exists( modernPath ) )
			return modernPath;

		var legacyPath = Path.Combine( projectRoot, TailBoxConfig.LegacyFileName );
		if ( File.Exists( legacyPath ) )
			return legacyPath;

		return modernPath;
	}

	public static IReadOnlyCollection<string> FindContentFiles( string projectRoot, TailBoxConfig config, string outputPath )
	{
		return EnumerateContentFiles( projectRoot, config, outputPath ).ToArray();
	}

	public static IReadOnlyList<TailBoxSourceText> ReadSources( IEnumerable<string> sourceFiles )
	{
		return sourceFiles
			.Select( file => new TailBoxSourceText( file, File.ReadAllText( file ) ) )
			.ToArray();
	}

	public static string ResolveOutputPath( string projectRoot, TailBoxConfig config )
	{
		var outputPath = config?.OutputPath;
		if ( string.IsNullOrWhiteSpace( outputPath ) )
			outputPath = TailBoxConfig.CreateDefault().OutputPath;

		return Path.IsPathRooted( outputPath )
			? Path.GetFullPath( outputPath )
			: Path.GetFullPath( Path.Combine( projectRoot, outputPath ) );
	}

	public static bool WriteIfChanged( string outputPath, string content )
	{
		Directory.CreateDirectory( Path.GetDirectoryName( outputPath )! );

		if ( File.Exists( outputPath ) && string.Equals( File.ReadAllText( outputPath ), content, StringComparison.Ordinal ) )
			return false;

		File.WriteAllText( outputPath, content );
		return true;
	}

	public static bool ShouldSkipPath( string projectRoot, string outputPath, string changedPath )
	{
		if ( string.IsNullOrWhiteSpace( projectRoot ) || string.IsNullOrWhiteSpace( changedPath ) )
			return true;

		var fullPath = Path.GetFullPath( changedPath );
		if ( !string.IsNullOrWhiteSpace( outputPath )
			&& string.Equals( fullPath, Path.GetFullPath( outputPath ), StringComparison.OrdinalIgnoreCase ) )
		{
			return true;
		}

		if ( !TryGetRelativeProjectPath( projectRoot, fullPath, out var relative ) )
			return true;

		var segments = relative.Split( '/', StringSplitOptions.RemoveEmptyEntries );
		return segments.Any( segment => segment is ".git" or ".sbox" or ".vs" or "bin" or "obj" );
	}

	public static bool TryGetRelativeProjectPath( string projectRoot, string path, out string relativePath )
	{
		relativePath = "";
		if ( string.IsNullOrWhiteSpace( projectRoot ) || string.IsNullOrWhiteSpace( path ) )
			return false;

		var root = Path.GetFullPath( projectRoot );
		var fullPath = Path.GetFullPath( path );
		var relative = Path.GetRelativePath( root, fullPath );
		if ( Path.IsPathRooted( relative ) )
			return false;

		relative = relative.Replace( '\\', '/' );
		if ( relative == ".." || relative.StartsWith( "../", StringComparison.Ordinal ) )
			return false;

		relativePath = relative;
		return true;
	}

	private static IEnumerable<string> EnumerateContentFiles( string projectRoot, TailBoxConfig config, string outputPath )
	{
		var globs = (config?.Content is { Count: > 0 } ? config.Content : TailBoxConfig.CreateDefault().Content)
			.Select( TailBoxGlobMatcher.FromGlob )
			.ToArray();

		foreach ( var file in Directory.EnumerateFiles( projectRoot, "*.*", SearchOption.AllDirectories ) )
		{
			var fullPath = Path.GetFullPath( file );
			if ( ShouldSkipPath( projectRoot, outputPath, fullPath ) )
				continue;

			if ( !TryGetRelativeProjectPath( projectRoot, fullPath, out var relative ) )
				continue;

			if ( globs.Any( glob => glob.IsMatch( relative ) ) )
				yield return fullPath;
		}
	}
}
