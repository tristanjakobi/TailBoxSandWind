using Sandbox.TailBox;
using System;
using System.Linq;

[TestClass]
public sealed class TailBoxConfigTests
{
	[TestMethod]
	public void LoadJsonMergesDefaultsAndIgnoresEmptyOverrides()
	{
		var loaded = TailBoxConfig.LoadJson(
			"""
			{
				"outputPath": "",
				"content": [],
				"safelist": null,
				"spacing": {
					"card": "22px",
					"": "ignored",
					"empty": ""
				},
				"colors": {
					"accent": "#111111",
					"brand": "#234567"
				}
			}
			""",
			"C:/Project/tailwand.config.json" );

		Assert.AreEqual( "Code/tailwand.generated.scss", loaded.OutputPath );
		CollectionAssert.AreEqual( new[] { "Code/**/*.razor" }, loaded.Content.ToArray() );
		Assert.AreEqual( 0, loaded.Safelist.Count );
		Assert.AreEqual( "22px", loaded.Spacing["CARD"] );
		Assert.IsFalse( loaded.Spacing.ContainsKey( "empty" ) );
		Assert.AreEqual( "#111111", loaded.Colors["ACCENT"] );
		Assert.AreEqual( "#234567", loaded.Colors["brand"] );
		Assert.AreEqual( "C:/Project/tailwand.config.json", loaded.ConfigPath );
	}

	[TestMethod]
	public void ToJsonUsesCamelCaseAndDoesNotPersistConfigPath()
	{
		var config = TailBoxConfig.CreateDefault();
		config.OutputPath = "Assets/Generated/tailbox.scss";
		config.ConfigPath = "C:/Project/tailwand.config.json";
		config.Safelist.Add( "flex" );

		var json = config.ToJson();
		var roundTrip = TailBoxConfig.LoadJson( json, "C:/Project/loaded.config.json" );

		StringAssert.Contains( json, "\"outputPath\"" );
		StringAssert.Contains( json, "\"fontSizes\"" );
		Assert.IsFalse( json.Contains( "ConfigPath", StringComparison.Ordinal ) );
		Assert.AreEqual( "Assets/Generated/tailbox.scss", roundTrip.OutputPath );
		Assert.AreEqual( "flex", roundTrip.Safelist.Single() );
		Assert.AreEqual( "C:/Project/loaded.config.json", roundTrip.ConfigPath );
	}

	[TestMethod]
	public void GetOutputFullPathNormalizesRelativeAndRootedPaths()
	{
		var config = TailBoxConfig.CreateDefault();
		config.OutputPath = "Code\\Generated\\tailbox.scss";

		Assert.AreEqual(
			"C:/game/project/Code/Generated/tailbox.scss",
			config.GetOutputFullPath( "C:\\game\\project" ) );

		config.OutputPath = "D:\\exports\\tailbox.scss";
		Assert.AreEqual( "D:/exports/tailbox.scss", config.GetOutputFullPath( "C:/game/project" ) );
	}

	[TestMethod]
	public void InvalidJsonReportsTheConfigPath()
	{
		var exception = Assert.ThrowsException<InvalidOperationException>(
			() => TailBoxConfig.LoadJson( "{", "C:/Project/tailwand.config.json" ) );

		StringAssert.Contains( exception.Message, "C:/Project/tailwand.config.json" );
		Assert.IsNotNull( exception.InnerException );
	}

	[TestMethod]
	public void RuntimeConfigFileMethodsStayEditorOnly()
	{
		Assert.ThrowsException<NotSupportedException>( () => TailBoxConfig.Exists( "C:/Project" ) );
		Assert.ThrowsException<NotSupportedException>( () => TailBoxConfig.Load( "C:/Project" ) );
		Assert.ThrowsException<NotSupportedException>( () => TailBoxConfig.LoadFile( "C:/Project/tailwand.config.json" ) );
		Assert.ThrowsException<NotSupportedException>( () => TailBoxConfig.SaveDefault( "C:/Project" ) );
		Assert.ThrowsException<NotSupportedException>( () => TailBoxConfig.CreateDefault().Save( "C:/Project" ) );
	}

	[TestMethod]
	public void ThemeFactoryAcceptsNullConfigAndCopiesDictionaries()
	{
		var defaults = TailBoxThemeFactory.FromConfig( null );
		Assert.AreEqual( "#d7b46a", defaults.Colors["accent"] );

		var config = TailBoxConfig.CreateDefault();
		config.Spacing["card"] = "22px";
		config.Colors = null;

		var theme = TailBoxThemeFactory.FromConfig( config );
		config.Spacing["card"] = "99px";

		Assert.AreEqual( "22px", theme.Spacing["card"] );
		Assert.AreEqual( 0, theme.Colors.Count );
	}
}
