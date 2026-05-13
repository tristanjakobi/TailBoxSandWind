using System;
using System.Collections.Generic;

namespace Sandbox.TailBox;

public static class TailBoxEditorProject
{
	public static string GetConfigPath( string projectRoot )
	{
		return TailBoxProjectFileSystem.GetConfigPath( projectRoot );
	}

	public static bool ConfigExists( string projectRoot )
	{
		return TailBoxProjectFileSystem.ConfigExists( projectRoot );
	}

	public static TailBoxConfig LoadConfig( string projectRoot )
	{
		return TailBoxProjectFileSystem.LoadConfig( projectRoot );
	}

	public static TailBoxConfig SaveDefaultConfig( string projectRoot )
	{
		var config = TailBoxConfig.CreateDefault();
		SaveConfig( projectRoot, config );
		return config;
	}

	public static void SaveConfig( string projectRoot, TailBoxConfig config )
	{
		TailBoxProjectFileSystem.SaveConfig( projectRoot, config );
	}

	public static TailBoxGenerationResult Generate( string projectRoot, bool writeFile = true )
	{
		var root = TailBoxProjectFileSystem.NormalizeProjectRoot( projectRoot );
		var config = LoadConfig( root );
		return Generate( root, config, writeFile );
	}

	public static TailBoxGenerationResult Generate( string projectRoot, TailBoxConfig config, bool writeFile = true )
	{
		var root = TailBoxProjectFileSystem.NormalizeProjectRoot( projectRoot );
		config ??= TailBoxConfig.CreateDefault();
		var outputPath = ResolveOutputPath( root, config );
		var sourceFiles = TailBoxProjectFileSystem.FindContentFiles( root, config, outputPath );
		var sources = TailBoxProjectFileSystem.ReadSources( sourceFiles );

		var result = new TailBoxGenerator().GenerateFromSources( sources, config, root, outputPath );
		var wroteFile = false;
		if ( writeFile )
			wroteFile = TailBoxProjectFileSystem.WriteIfChanged( outputPath, result.GeneratedScss );

		return WithWriteState( result, wroteFile );
	}

	public static IReadOnlyCollection<string> FindContentFiles( string projectRoot, TailBoxConfig config )
	{
		var root = TailBoxProjectFileSystem.NormalizeProjectRoot( projectRoot );
		return TailBoxProjectFileSystem.FindContentFiles( root, config, ResolveOutputPath( root, config ) );
	}

	public static string ResolveOutputPath( string projectRoot, TailBoxConfig config )
	{
		return TailBoxProjectFileSystem.ResolveOutputPath( projectRoot, config );
	}

	private static TailBoxGenerationResult WithWriteState( TailBoxGenerationResult result, bool wroteFile )
	{
		return new TailBoxGenerationResult
		{
			ProjectRoot = result.ProjectRoot,
			OutputPath = result.OutputPath,
			GeneratedScss = result.GeneratedScss,
			ScannedFileCount = result.ScannedFileCount,
			DiscoveredClassCount = result.DiscoveredClassCount,
			WroteFile = wroteFile,
			GeneratedClasses = result.GeneratedClasses,
			SkippedClasses = result.SkippedClasses,
			Skipped = result.Skipped,
			Warnings = result.Warnings
		};
	}

	public static bool ShouldSkipPath( string projectRoot, string outputPath, string changedPath )
	{
		return TailBoxProjectFileSystem.ShouldSkipPath( projectRoot, outputPath, changedPath );
	}
}
