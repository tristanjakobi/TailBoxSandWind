using System.Collections.Generic;
using System.Linq;

namespace Sandbox.TailBox;

[Title( "tailw& Demo" )]
[Category( "tailw&" )]
[Icon( "dashboard" )]
public sealed class TailBoxDemo : Component, Component.ExecuteInEditor
{
	[Property]
	public bool CreateScreenIfMissing { get; set; } = true;

	[Property]
	public bool CreateCameraIfMissing { get; set; } = true;

	[Property]
	public bool EnableMouseUi { get; set; } = true;

	[Property]
	public bool CaptureMouse { get; set; }

	[Property]
	public bool KeepSceneReady { get; set; } = true;

	[Property]
	public bool RunInEditor { get; set; }

	private bool ensuredOnce;

	protected override void OnAwake()
	{
		EnsureDemo();
	}

	protected override void OnStart()
	{
		EnsureDemo();
	}

	protected override void OnUpdate()
	{
		if ( KeepSceneReady || !ensuredOnce )
			EnsureDemo();
	}

	private void EnsureDemo()
	{
		var scene = Scene;
		if ( scene is null )
			return;

		if ( scene.IsEditor && !RunInEditor )
		{
			DestroyDemoMenus( scene, null );
			ensuredOnce = false;
			return;
		}

		var screen = ResolveScreenPanel( scene );
		if ( screen is null )
			return;

		var camera = ResolveCamera( scene );
		if ( camera is not null )
			screen.TargetCamera = camera;

		var menu = ResolveMenu( screen );
		DestroyDemoMenus( scene, menu );
		ConfigureMouse();

		ensuredOnce = true;
	}

	private ScreenPanel ResolveScreenPanel( Scene scene )
	{
		var screens = FindSceneComponents<ScreenPanel>( scene ).ToArray();
		var existingScreen = screens
			.FirstOrDefault( screen => screen.GameObject.GetComponent<TailBoxDemoMenu>( true ) is not null )
			?? screens.FirstOrDefault( screen => screen.Enabled )
			?? screens.FirstOrDefault();

		if ( existingScreen is not null )
		{
			existingScreen.Enabled = true;
			return existingScreen;
		}

		if ( !CreateScreenIfMissing )
			return null;

		var screenPanel = GameObject.GetComponent<ScreenPanel>( true ) ?? GameObject.GetOrAddComponent<ScreenPanel>();
		screenPanel.Enabled = true;
		return screenPanel;
	}

	private CameraComponent ResolveCamera( Scene scene )
	{
		var cameras = FindSceneComponents<CameraComponent>( scene ).ToArray();
		var mainCamera = cameras.FirstOrDefault( camera => camera.IsMainCamera && camera.Enabled );
		if ( mainCamera is not null )
			return mainCamera;

		var enabledCamera = cameras.FirstOrDefault( camera => camera.Enabled );
		if ( enabledCamera is not null )
		{
			enabledCamera.IsMainCamera = true;
			return enabledCamera;
		}

		if ( !CreateCameraIfMissing )
			return cameras.FirstOrDefault();

		var camera = GameObject.GetComponent<CameraComponent>( true ) ?? GameObject.GetOrAddComponent<CameraComponent>();
		camera.Enabled = true;
		camera.IsMainCamera = true;
		camera.FieldOfView = 70f;
		return camera;
	}

	private TailBoxDemoMenu ResolveMenu( ScreenPanel screen )
	{
		var menu = screen.GameObject.GetComponent<TailBoxDemoMenu>( true ) ?? screen.GameObject.GetOrAddComponent<TailBoxDemoMenu>();
		menu.Enabled = true;
		menu.EnableMouseUi = EnableMouseUi;
		menu.CaptureMouse = CaptureMouse;
		menu.ConfigureMouseInput();
		return menu;
	}

	private static void DestroyDemoMenus( Scene scene, TailBoxDemoMenu activeMenu )
	{
		foreach ( var menu in FindSceneComponents<TailBoxDemoMenu>( scene ).ToArray() )
		{
			if ( activeMenu is not null && menu == activeMenu )
				continue;

			menu.Destroy();
		}
	}

	private static IEnumerable<T> FindSceneComponents<T>( Scene scene ) where T : Component
	{
		return scene.GetAllObjects( true )
			.SelectMany( gameObject => gameObject.Components.GetAll<T>() );
	}

	private void ConfigureMouse()
	{
		if ( !EnableMouseUi )
			return;

		Mouse.Visibility = MouseVisibility.Visible;
	}
}
