using System;
using System.IO;
using Sandbox.TailBox;

var root = Directory.GetCurrentDirectory();
var razorPath = Path.Combine(root, "Code", "Demo", "TailBoxDemoMenu.razor");
var generatedPath = Path.Combine(root, "Code", "Demo", "TailBoxDemoMenu.generated.scss");
var bundledPath = Path.Combine(root, "Code", "Demo", "TailBoxDemoMenu.cs.scss");
var config = TailBoxConfig.CreateDefault();
var result = new TailBoxGenerator().GenerateFromSources(new[] { new TailBoxSourceText(razorPath, File.ReadAllText(razorPath)) }, config, root, generatedPath);
File.WriteAllText(generatedPath, result.GeneratedScss);
File.WriteAllText(bundledPath, """
root {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	pointer-events: all;
	background-color: #0d1418;
	color: #f1f5f7;
	font-family: Inter;
}

.tailwand-logo {
	width: 184px;
	height: 184px;
	object-fit: contain;
}

.tb-demo-sidebar {
	width: 250px;
}

.tb-demo-nav-button {
	height: 88px;
}

.tb-demo-nav-index {
	width: 64px;
}

.tb-demo-tile {
	background-color: rgba( 34, 39, 44, 0.94 );
}

.tb-demo-light-surface {
	background-color: #f1f5f7;
}

.tb-demo-dark-label {
	color: #0d1418;
}

.tb-demo-label-light {
	color: #f1f5f7;
}

.tb-demo-label-accent {
	color: #d7b46a;
}

""" + result.GeneratedScss);
Console.WriteLine($"generated={result.GeneratedClassCount} skipped={result.SkippedClassCount} warnings={result.Warnings.Count}");
foreach (var skipped in result.Skipped) Console.WriteLine($"skipped {skipped.ClassName}: {skipped.Reason} {skipped.Detail}");
