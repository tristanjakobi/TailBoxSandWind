using System;
using System.IO;
using System.Linq;
using Editor;
using static Sandbox.Internal.GlobalGameNamespace;
using static Sandbox.Internal.GlobalToolsNamespace;

namespace Sandbox.TailBox;

public sealed class TailBoxEditorWatcher
{
	private const string WatcherCookie = "TailBoxSandWind.WatcherEnabled";
	private const double DebounceSeconds = 0.35;

	private static TailBoxEditorWatcher instance;

	private FileSystemWatcher watcher;
	private string watchedRoot;
	private string watchedOutputPath;
	private bool pendingGenerate;
	private DateTime lastChangeUtc;
	private DateTime lastProjectProbeUtc;
	private readonly object gate = new();

	public static bool WatcherEnabled
	{
		get => EditorCookie.Get( WatcherCookie, true );
		set => EditorCookie.Set( WatcherCookie, value );
	}

	public static TailBoxEditorWatcher EnsureStarted()
	{
		if ( instance is not null )
			return instance;

		instance = new TailBoxEditorWatcher();
		EditorEvent.Register( instance );
		instance.Reconfigure();
		return instance;
	}

	public static TailBoxGenerationResult GenerateNow( string projectRoot )
	{
		var result = TailBoxEditorProject.Generate( projectRoot );
		Log.Info( $"TailBox generated {result.GeneratedClassCount} utilities, skipped {result.SkippedClassCount}, warnings {result.Warnings.Count}: {result.OutputPath}" );

		foreach ( var skipped in result.Skipped.Take( 12 ) )
		{
			Log.Warning( $"TailBox skipped '{skipped.ClassName}' ({skipped.Reason}): {skipped.Detail}" );
		}

		return result;
	}

	public void Reconfigure()
	{
		var root = Project.Current?.GetRootPath();
		if ( string.IsNullOrWhiteSpace( root ) )
		{
			DisposeWatcher();
			return;
		}

		root = Path.GetFullPath( root );
		if ( !WatcherEnabled || !IsProjectOptedIn( root ) )
		{
			DisposeWatcher();
			watchedRoot = root;
			return;
		}

		var config = TailBoxEditorProject.LoadConfig( root );
		var outputPath = TailBoxEditorProject.ResolveOutputPath( root, config );

		if ( watcher is not null
			&& string.Equals( watchedRoot, root, StringComparison.OrdinalIgnoreCase )
			&& string.Equals( watchedOutputPath, outputPath, StringComparison.OrdinalIgnoreCase ) )
		{
			return;
		}

		DisposeWatcher();

		watchedRoot = root;
		watchedOutputPath = outputPath;
		watcher = new FileSystemWatcher( root )
		{
			IncludeSubdirectories = true,
			Filter = "*.*",
			NotifyFilter = NotifyFilters.LastWrite
				| NotifyFilters.FileName
				| NotifyFilters.DirectoryName
				| NotifyFilters.Size
				| NotifyFilters.CreationTime
		};

		watcher.Changed += OnFileChanged;
		watcher.Created += OnFileChanged;
		watcher.Deleted += OnFileChanged;
		watcher.Renamed += OnFileChanged;
		watcher.EnableRaisingEvents = true;
		Log.Info( $"TailBox watcher enabled for {root}" );
	}

	[EditorEvent.Frame]
	public void OnFrame()
	{
		if ( (DateTime.UtcNow - lastProjectProbeUtc).TotalSeconds > 1.0 )
		{
			lastProjectProbeUtc = DateTime.UtcNow;
			Reconfigure();
		}

		var shouldGenerate = false;
		lock ( gate )
		{
			if ( pendingGenerate && (DateTime.UtcNow - lastChangeUtc).TotalSeconds >= DebounceSeconds )
			{
				pendingGenerate = false;
				shouldGenerate = true;
			}
		}

		if ( !shouldGenerate || string.IsNullOrWhiteSpace( watchedRoot ) || !TailBoxEditorProject.ConfigExists( watchedRoot ) )
			return;

		try
		{
			GenerateNow( watchedRoot );
		}
		catch ( Exception ex )
		{
			Log.Error( $"TailBox generation failed: {ex.Message}" );
		}
	}

	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		if ( !ShouldHandleChangedPath( watchedRoot, watchedOutputPath, e.FullPath ) )
			return;

		lock ( gate )
		{
			pendingGenerate = true;
			lastChangeUtc = DateTime.UtcNow;
		}
	}

	private void DisposeWatcher()
	{
		if ( watcher is null )
			return;

		watcher.EnableRaisingEvents = false;
		watcher.Changed -= OnFileChanged;
		watcher.Created -= OnFileChanged;
		watcher.Deleted -= OnFileChanged;
		watcher.Renamed -= OnFileChanged;
		watcher.Dispose();
		watcher = null;
	}

	public static bool IsProjectOptedIn( string projectRoot )
	{
		return !string.IsNullOrWhiteSpace( projectRoot ) && TailBoxEditorProject.ConfigExists( projectRoot );
	}

	public static bool ShouldHandleChangedPath( string projectRoot, string outputPath, string changedPath )
	{
		if ( string.IsNullOrWhiteSpace( projectRoot ) || string.IsNullOrWhiteSpace( changedPath ) )
			return false;

		var fullPath = Path.GetFullPath( changedPath );
		if ( !string.IsNullOrWhiteSpace( outputPath )
			&& string.Equals( fullPath, Path.GetFullPath( outputPath ), StringComparison.OrdinalIgnoreCase ) )
		{
			return false;
		}

		if ( TailBoxEditorProject.ShouldSkipPath( projectRoot, outputPath, fullPath ) )
			return false;

		if ( !TailBoxProjectFileSystem.TryGetRelativeProjectPath( projectRoot, fullPath, out var relative ) )
			return false;

		return string.Equals( relative, TailBoxConfig.FileName, StringComparison.OrdinalIgnoreCase )
			|| string.Equals( Path.GetExtension( fullPath ), ".razor", StringComparison.OrdinalIgnoreCase );
	}
}
