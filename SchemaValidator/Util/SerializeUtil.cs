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
		settings.RegisterSerializer(typeof(Dictionary<int, List<string>>), new CustomDictionarySerializer());
		settings.RegisterSerializer(typeof(FieldType), new CustomFieldTypeSerializer());

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
	
	public static EvaluationResults? EvaluateSchema(string filePath)
	{
		var yamlText = File.ReadAllText(filePath);
		object? yamlObject;
		try
		{
			yamlObject = _serializer.Deserialize(yamlText);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return null;
		}
		if (yamlObject == null) return null;
		
		var json = JsonConvert.SerializeObject(yamlObject);
		// Console.WriteLine(json);
		// File.WriteAllText(@"C:\Users\Liam\Documents\repos\EXDSchema\SchemaValidator\test.json", json);

		var schemaText = File.ReadAllText(@"C:\Users\Liam\Documents\repos\EXDSchema\SchemaValidator\SchemaSchema.json");
		var schema = JsonSchema.FromText(schemaText);
		var node = JsonNode.Parse(json);
		return schema.Evaluate(node);
	}
}

internal class CustomDictionarySerializer : DictionarySerializer
{
	protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object?> keyValue, KeyValuePair<Type, Type> types)
	{
		objectContext.SerializerContext.WriteYaml(keyValue.Key, types.Key);
		objectContext.SerializerContext.WriteYaml(keyValue.Value, types.Value, YamlStyle.Flow);
	}
}

internal class CustomFieldTypeSerializer : ScalarSerializerBase
{
	public override object? ConvertFrom(ref ObjectContext context, Scalar fromScalar)
	{
		return Enum.Parse<FieldType>(new PascalNamingConvention().Convert(fromScalar.Value));
	}
	
	public override string ConvertTo(ref ObjectContext objectContext)
	{
		return objectContext.Settings.NamingConvention.Convert(objectContext.Instance.ToString());
	}
}