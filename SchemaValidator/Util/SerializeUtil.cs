using System.Text.Json.Nodes;
using Json.Schema;
using Newtonsoft.Json;
using SchemaValidator.New;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using JsonSchema = Json.Schema.JsonSchema;

namespace SchemaValidator.Util;

public static class SerializeUtil
{
	private static readonly Serializer _serializer;

	static SerializeUtil()
	{
		var settings = new SerializerSettings
		{
			EmitAlias = false,
			EmitDefaultValues = false,
			NamingConvention = new CamelCaseNamingConvention(),
			IgnoreNulls = true,
		};

		_serializer = new Serializer(settings);
	}

	public static string Serialize(object o)
	{
		return _serializer.Serialize(o);
	}

	public static T? Deserialize<T>(string s)
	{
		return _serializer.Deserialize<T>(s);
	}

	public static object? Deserialize(string s)
	{
		return _serializer.Deserialize(s);
	}
}
