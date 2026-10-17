using System.Text.Json;
using System.Text.Json.Nodes;

namespace Keysharp.Tests;

[TestFixture, Category("Internal"), Category("Curated")]
public class DescriptorTests
{
	[TestCase(false), TestCase(true)]
	public void ReadContract(bool component)
	{
		var json = component
			? """{"schemaVersion":1,"contractVersion":1,"id":"compiler","version":"1.2.3","assembly":"unit.dll","type":"Example.Unit","capabilities":["Compilation"],"files":["unit.dll","data.txt"]}"""
			: """{"name":"nuget","version":"1.2.3","assembly":"unit.dll","type":"Example.Unit","files":["unit.dll","data.txt"]}""";
		var path = Path.GetTempFileName();
		object Read() => component
			? ScriptingComponentRegistry.Descriptor.Read(path)
			: Keysharp.Internals.Os.PackageProviderRegistry.Descriptor.Read(path);

		try
		{
			File.WriteAllText(path, json.Insert(1, """
				"Id":false,"Name":false,"future":{"items":[null,false]},"id":"first","name":"first",
				"""));
			var actual = JsonSerializer.SerializeToNode(Read(),
				new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			Assert.IsTrue(JsonNode.DeepEquals(JsonNode.Parse(json), actual), actual.ToJsonString());

			foreach (var invalid in new[]
			{
				"[]", "null", """{"version":false}""", """{"version":false,"version":"1"}""",
				"""{"files":{}}""", """{"files":[1]}""",
				component ? """{"schemaVersion":"1"}""" : """{"assembly":1}"""
			})
			{
				File.WriteAllText(path, invalid);
				Assert.That(() => Read(), Throws.InstanceOf<Exception>(), invalid);
			}
		}
		finally
		{
			File.Delete(path);
		}
	}
}
