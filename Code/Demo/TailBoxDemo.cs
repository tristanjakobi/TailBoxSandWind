using System.Linq;

namespace Sandbox.TailBox;

[Title( "TailBox Demo" )]
[Category( "TailBox SandWind" )]
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
	public bool RunInEditor { get; set; } = true;

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
		if ( Game.IsEditor && !RunInEditor )
			return;

		var screen = ResolveScreenPanel();
		if ( screen is null )
			return;

		ResolveCamera();
		ResolveMenu( screen );
		ConfigureMouse();

		ensuredOnce = true;
	}

	private ScreenPanel ResolveScreenPanel()
	{
		var existingScreen = Scene.GetAllComponents<ScreenPanel>()
			.FirstOrDefault( screen => screen.GameObject.GetComponent<TailBoxDemoMenu>( true ) is not null )
			?? Scene.GetAllComponents<ScreenPanel>().FirstOrDefault( screen => screen.Enabled )
			?? Scene.GetAllComponents<ScreenPanel>().FirstOrDefault();

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

	private CameraComponent ResolveCamera()
	{
		var existingCamera = Scene.GetAllComponents<CameraComponent>()
			.FirstOrDefault( camera => camera.IsMainCamera && camera.Enabled )
			?? Scene.GetAllComponents<CameraComponent>().FirstOrDefault( camera => camera.Enabled )
			?? Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

		if ( existingCamera is not null )
			return existingCamera;

		if ( !CreateCameraIfMissing )
			return null;

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

	private void ConfigureMouse()
	{
		if ( !EnableMouseUi )
			return;

		Mouse.Visibility = MouseVisibility.Visible;
	}
}
