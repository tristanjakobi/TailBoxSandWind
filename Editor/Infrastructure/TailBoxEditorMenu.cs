using System;
using System.IO;
using Editor;

namespace Sandbox.TailBox;

public static class TailBoxEditorMenu
{
	[Menu( "Editor", "tailw&/Initialize", "auto_awesome" )]
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
			"tailw&",
			$"Initialized tailw&.\n\nConfig: {configPath}\nGenerated: {result.GeneratedClassCount} utilities\nSkipped: {result.SkippedClassCount}\nWarnings: {result.Warnings.Count}" );
	}

	[Menu( "Editor", "tailw&/Generate Now", "refresh" )]
	public static void GenerateNow()
	{
		var root = GetProjectRoot();
		if ( root is null )
			return;

		if ( !TailBoxEditorProject.ConfigExists( root ) )
		{
			ShowDialog(
				"tailw&",
				"tailwand.config.json was not found in this project. Use tailw&/Initialize to opt in. Existing tailbox.config.json files are still supported." );
			return;
		}

		var result = TailBoxEditorWatcher.GenerateNow( root );
		TailBoxEditorWatcher.EnsureStarted().Reconfigure();

		ShowDialog(
			"tailw&",
			$"Generated {result.GeneratedClassCount} utilities from {result.ScannedFileCount} Razor files.\nSkipped: {result.SkippedClassCount}\nWarnings: {result.Warnings.Count}\n\nOutput: {result.OutputPath}" );
	}

	[Menu( "Editor", "tailw&/Toggle Watcher", "sync" )]
	public static void ToggleWatcher()
	{
		var root = GetProjectRoot();
		if ( root is null )
			return;

		var enabled = TailBoxEditorWatcher.WatcherEnabled;
		TailBoxEditorWatcher.WatcherEnabled = !enabled;
		TailBoxEditorWatcher.EnsureStarted().Reconfigure();

		ShowDialog(
			"tailw&",
			$"tailw& watcher is now {(TailBoxEditorWatcher.WatcherEnabled ? "enabled" : "disabled")}." );
	}

	private static string GetProjectRoot()
	{
		var root = Project.Current?.GetRootPath();
		if ( string.IsNullOrWhiteSpace( root ) )
		{
			ShowDialog( "tailw&", "No active s&box project was found." );
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
