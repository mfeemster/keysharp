namespace Keysharp.Tests;

[TestFixture, Category("Internal"), Category("Curated")]
public class AppManifestTests : TestRunner
{
	[Test]
	public void EmbeddedValidation()
	{
		var malformed = BuildAssemblyWithManifest("{");
		var malformedError = Assert.Throws<InvalidDataException>(() => AppManifest.FromAssembly(malformed));
		Assert.That(malformedError.Message, Does.Contain(AppManifest.ResourceName));
		Assert.That(malformedError.Message, Does.Contain(malformed.GetName().Name));

		var invalidUtf8 = BuildAssemblyWithManifest([0xff]);
		Assert.That(Assert.Throws<InvalidDataException>(() => AppManifest.FromAssembly(invalidUtf8)).Message,
			Does.Contain("could not be read"));

		foreach (var json in new[]
		{
			"{\"trayIcon\":\"icons/library.dll\",\"trayIconNumber\":2,\"files\":[]}",
			"{\"icon\":\"assets/app.ico\",\"files\":[]}",
		})
			Assert.That(Assert.Throws<InvalidDataException>(() =>
				AppManifest.FromAssembly(BuildAssemblyWithManifest(json))).Message, Does.Contain("payload is missing"));

		foreach (var (json, resource) in new[]
		{
			("{\"icon\":\"assets/app.ico\",\"files\":[]}", AppManifest.IconResourceName),
			("{\"trayIcon\":\"assets/tray.png\",\"files\":[]}", AppManifest.TrayIconResourceName),
		})
		{
			var assembly = BuildAssemblyWithManifest(Encoding.UTF8.GetBytes(json),
				(resource, new byte[] { 0, 0, 1, 0, 1, 0 }));
			Assert.That(Assert.Throws<InvalidDataException>(() => AppManifest.FromAssembly(assembly)).Message,
				Does.Contain("structurally valid ICO"));
		}
	}

	[Test]
	public void FailureIsolation()
	{
		var previous = Script.TheScript;
		var programType = BuildAssemblyWithManifest("{").GetType("AppManifestMarker", throwOnError: true);
		_ = Assert.Throws<InvalidDataException>(() => new Script(programType));
		Assert.That(Script.TheScript, Is.SameAs(previous));
	}

	[TestCase(" classic ", "Classic")]
	[TestCase("SYSTEM", "System")]
	[TestCase("dark", "Dark")]
	public void GuiTheme(string value, string expected)
	{
		Assert.IsTrue(Script.TryNormalizeGuiTheme(value, out var actual));
		Assert.That(actual, Is.EqualTo(expected));
		Assert.That(Script.TryNormalizeGuiTheme("Neon", out _), Is.False);
	}

	[Test]
	public void ReadContract()
	{
		Assert.IsNull(AppManifest.FromAssembly(typeof(AppManifestTests).Assembly));

		foreach (var json in new[]
		{
			"not-json", "[]", "{\"files\":null}", "{\"guiTheme\":\"Neon\",\"files\":[]}",
			"{\"name\":1}", "{\"name\":false,\"name\":\"valid\"}", "{\"noTrayIcon\":1}", "{\"consoleApp\":\"true\"}",
			"{\"files\":{}}", "{\"files\":[1]}", "{\"files\":[null]}",
		})
			Assert.That(Assert.Throws<InvalidDataException>(() => AppManifest.Read(json)).Message, Does.Contain("#App manifest"));

		foreach (var version in new[] { "1", "1.2.3.4.5", "1.65535", "1.-2" })
		{
			Assert.That(AppManifest.IsValidAssemblyVersion(version), Is.False);
			Assert.That(Assert.Throws<InvalidDataException>(() =>
				AppManifest.Read($"{{\"version\":\"{version}\",\"files\":[]}}")).Message, Does.Contain("'version'"));
		}

		foreach (var version in new[] { "0.0", "0.65534.2.3" })
		{
			Assert.IsTrue(AppManifest.IsValidAssemblyVersion(version));
			Assert.That(AppManifest.Read($"{{\"version\":\"{version}\",\"files\":[]}}").Version, Is.EqualTo(version));
		}

		var invalid = new (string Json, string Message)[]
		{
			("{\"icon\":\"/assets/app.ico\",\"files\":[]}", "'icon'"),
			("{\"icon\":\"assets\\\\app.ico\",\"files\":[]}", "'icon'"),
			("{\"icon\":\"assets/app.png\",\"files\":[]}", "'icon'"),
			("{\"trayIcon\":\"./tray.ico\",\"files\":[]}", "'trayIcon'"),
			("{\"trayIcon\":\"icons.dll\",\"trayIconNumber\":0,\"files\":[]}", "'trayIconNumber'"),
			("{\"trayIcon\":\"icons.dll\",\"trayIconNumber\":2,\"trayIconResource\":\"AppIcon\",\"files\":[]}", "mutually exclusive"),
			("{\"trayIconNumber\":2,\"files\":[]}", "requires a custom"),
			("{\"trayIcon\":\"icons.dll\",\"noTrayIcon\":true,\"files\":[]}", "cannot be true"),
			("{\"trayIcon\":\"icons.dll\",\"trayIconResource\":\"   \",\"files\":[]}", "non-empty managed resource"),
			("{\"files\":[\"assets\\\\file.txt\"]}", "'files'"),
			("{\"files\":[\"assets/*.txt\"]}", "wildcards"),
		};

		foreach (var (json, message) in invalid)
			Assert.That(Assert.Throws<InvalidDataException>(() => AppManifest.Read(json)).Message, Does.Contain(message));

		var manifest = AppManifest.Read(
			"{\"icon\":\"assets/app.ICO\",\"noTrayIcon\":true,\"files\":[\"assets/file.txt\"]}");
		Assert.That(manifest.Icon, Is.EqualTo("assets/app.ICO"));
		Assert.IsTrue(manifest.TrayIconSuppressed);
		Assert.That(manifest.Files, Is.EqualTo(["assets/file.txt"]).AsCollection);

		var numbered = AppManifest.Read(
			"{\"trayIcon\":\"icons/library.dll\",\"trayIconNumber\":-12,\"files\":[]}");
		Assert.That(numbered.TrayIconNumber, Is.EqualTo(-12L));
		var named = AppManifest.Read(
			"{\"trayIcon\":\"icons/library.dll\",\"trayIconResource\":\"ApplicationIcon\",\"files\":[]}");
		Assert.That(named.TrayIconResource, Is.EqualTo("ApplicationIcon"));

		var repeated = AppManifest.Read("""
			{"Name":false,"future":{"items":[null,1]},"title":"first","title":"last",
			 "description":"first","description":null,"noTrayIcon":true,"noTrayIcon":null,
			 "consoleApp":false,"consoleApp":null,"trayIconNumber":1,"trayIconNumber":null,
			 "files":["old.txt"],"files":["new.txt"]}
			""");
		Assert.That(repeated.Write(), Is.EqualTo("""{"title":"last","files":["new.txt"]}"""));
		Assert.That(AppManifest.Read("{}").Files, Is.Empty);

		foreach (var number in new[] { long.MinValue, long.MaxValue })
			Assert.That(AppManifest.Read($"{{\"trayIcon\":\"icons.dll\",\"trayIconNumber\":{number}}}").TrayIconNumber,
				Is.EqualTo(number));

		foreach (var number in new[] { "1.0", "1e0", "\"1\"", "true", "9223372036854775808", "-9223372036854775809" })
			Assert.Throws<InvalidDataException>(() =>
				AppManifest.Read($"{{\"trayIcon\":\"icons.dll\",\"trayIconNumber\":{number}}}"), number);
	}

	[Test]
	public void ReadsEveryWrittenKey()
	{
		AppManifest[] samples =
		[
			new()
			{
				Name = "RoundTrip", Title = "Title", Description = "Description", Configuration = "Release", Company = "Company",
				Product = "Product", Copyright = "Copyright", Trademark = "Trademark", Version = "1.2.3.4", Icon = "assets/app.ico",
				TrayIcon = "icons/library.dll", NoTrayIcon = false, TrayIconNumber = -12, GuiTheme = "Dark", SingleInstance = "Force",
				ConsoleApp = true, HookMutexName = "RoundTripMutex", DesktopEntry = "keysharp-roundtrip", Files = ["assets/a.txt"]
			},
			new() { TrayIcon = "icons/library.dll", TrayIconResource = "ApplicationIcon" },
		];

		foreach (var key in typeof(AppManifest).GetProperties()
			.Where(p => p.IsDefined(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute))))
			Assert.That(samples.Any(sample => key.GetValue(sample) != null), $"No sample sets '{key.Name}'.");

		foreach (var sample in samples)
			Assert.That(AppManifest.Read(sample.Write()).Write(), Is.EqualTo(sample.Write()));
	}

	[Test]
	public void TrayDefaults()
	{
		var json = "{\"trayIcon\":\"assets/tray.png\",\"trayIconNumber\":-12,\"files\":[]}";
		var assembly = BuildAssemblyWithManifest(Encoding.UTF8.GetBytes(json),
			(AppManifest.TrayIconResourceName, MinimalIcon()));
		using var script = new Script(assembly.GetType("AppManifestMarker", throwOnError: true));
		Assert.That(Accessors.A_IconFile, Is.Empty);
		Assert.That(Accessors.A_IconNumber, Is.EqualTo(1L));
	}

	private static Assembly BuildAssemblyWithManifest(string json) =>
		BuildAssemblyWithManifest(Encoding.UTF8.GetBytes(json));

	private static Assembly BuildAssemblyWithManifest(byte[] bytes,
		params (string Name, byte[] Bytes)[] additionalResources)
	{
		var syntax = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("internal sealed class AppManifestMarker { }");
		var reference = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"AppManifestTest_" + Guid.NewGuid().ToString("N"), [syntax], [reference],
			new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
				Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
		var resources = new List<Microsoft.CodeAnalysis.ResourceDescription> { new(
			AppManifest.ResourceName, () => new MemoryStream(bytes, writable: false), isPublic: true) };

		foreach (var (name, resourceBytes) in additionalResources)
			resources.Add(new Microsoft.CodeAnalysis.ResourceDescription(name,
				() => new MemoryStream(resourceBytes, writable: false), isPublic: true));

		using var output = new MemoryStream();
		var result = compilation.Emit(output, manifestResources: resources);
		Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
		return Assembly.Load(output.ToArray());
	}

	private static byte[] MinimalIcon() =>
		[0, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 32, 0, 1, 0, 0, 0, 22, 0, 0, 0, 0];
}
