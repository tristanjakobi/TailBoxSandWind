namespace Sandbox.TailBox;

public partial class TailBoxDemoMenu : PanelComponent
{
	[Property]
	public bool EnableMouseUi { get; set; } = true;

	[Property]
	public bool CaptureMouse { get; set; }

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		ConfigureMouseInput();
	}

	protected override void OnTreeBuilt()
	{
		base.OnTreeBuilt();
		ConfigureMouseInput();
	}

	internal void ConfigureMouseInput()
	{
		if ( Panel is null )
			return;

		Panel.AcceptsFocus = EnableMouseUi;
		Panel.SetMouseCapture( EnableMouseUi && CaptureMouse );

		if ( EnableMouseUi )
		{
			Mouse.Visibility = MouseVisibility.Visible;
		}
	}
}
