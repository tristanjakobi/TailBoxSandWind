using System;
using System.IO;
using Editor;

namespace Sandbox.TailBox;

public static class TailBoxEditorMenu
{
	[Menu( "Editor", "TailBox SandWind/Initialize", "auto_awesome" )]
	public static void Initialize()
	{
		var root = GetProjectRoot();
		if ( root is null )
			return;

		var configPath = TailBoxEditorProject.GetConfigPath( root );
		if ( !File.Exists( configPath ) )
		{
			TailBoxEditorProject.SaveDefaultConfig( root );
		}

		var result = TailBoxEditorWatcher.GenerateNow( root );
		TailBoxEditorWatcher.EnsureStarted().Reconfigure();

		ShowDialog(
			"TailBox SandWind",
			$"Initialized TailBox SandWind.\n\nConfig: {configPath}\nGenerated: {result.GeneratedClassCount} utilities\nSkipped: {result.SkippedClassCount}\nWarnings: {result.Warnings.Count}" );
	}

	[Menu( "Editor", "TailBox SandWind/Generate Now", "refresh" )]
	public static void GenerateNow()
	{
		var root = GetProjectRoot();
		if ( root is null )
			return;

		if ( !TailBoxEditorProject.ConfigExists( root ) )
		{
			ShowDialog(
				"TailBox SandWind",
				"tailbox.config.json was not found in this project. Use TailBox SandWind/Initialize to opt in." );
			return;
		}

		var result = TailBoxEditorWatcher.GenerateNow( root );
		TailBoxEditorWatcher.EnsureStarted().Reconfigure();

		ShowDialog(
			"TailBox SandWind",
			$"Generated {result.GeneratedClassCount} utilities from {result.ScannedFileCount} Razor files.\nSkipped: {result.SkippedClassCount}\nWarnings: {result.Warnings.Count}\n\nOutput: {result.OutputPath}" );
	}

	[Menu( "Editor", "TailBox SandWind/Toggle Watcher", "sync" )]
	public static void ToggleWatcher()
	{
		var root = GetProjectRoot();
		if ( root is null )
			return;

		var enabled = TailBoxEditorWatcher.WatcherEnabled;
		TailBoxEditorWatcher.WatcherEnabled = !enabled;
		TailBoxEditorWatcher.EnsureStarted().Reconfigure();

		ShowDialog(
			"TailBox SandWind",
			$"TailBox watcher is now {(TailBoxEditorWatcher.WatcherEnabled ? "enabled" : "disabled")}." );
	}

	private static string GetProjectRoot()
	{
		var root = Project.Current?.GetRootPath();
		if ( string.IsNullOrWhiteSpace( root ) )
		{
			ShowDialog( "TailBox SandWind", "No active s&box project was found." );
			return null;
		}

		return Path.GetFullPath( root );
	}

	private static void ShowDialog( string title, string message )
	{
		EditorUtility.DisplayDialog( title, message, "OK", null, null );
	}
}

internal static class TailBoxEditorBootstrap
{
	[Event( "editor.created" )]
	private static void OnEditorCreated( EditorMainWindow _ )
	{
		TailBoxEditorWatcher.EnsureStarted();
	}
}
