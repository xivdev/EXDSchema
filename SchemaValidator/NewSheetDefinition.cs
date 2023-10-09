// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SchemaValidator.New;

public enum FieldType
{
	Scalar,
	Array,
	Icon,
	ModelId,
	Color,
	Link,
}

public class Sheet
{
	public string Name { get; set; }
	public string? DisplayField { get; set; }
	public List<Field> Fields { get; set; }
}

public class Field
{
	public string? Name { get; set; }
	public int? Count { get; set; }
	[DefaultValue(FieldType.Scalar)]
	[JsonConverter(typeof(StringEnumConverter), true)]
	public FieldType Type { get; set; }
	public string? Comment { get; set; }
	public List<Field>? Fields { get; set; }
	public Condition? Condition { get; set; }
	public List<string>? Targets { get; set; }

	public override string ToString()
	{
		var arraySuffix = Count.HasValue ? $"[{Count}]" : "";
		var name = Name != null ? $"{Name}{arraySuffix}" : "Unknown";
		return $"{name} ({Type})";
	}

	public override bool Equals(object? obj)
	{
		if (obj is not Field other)
			return false;
		var fieldsEqual = (Fields == null && other.Fields == null) || (Fields != null && other.Fields != null && Fields.SequenceEqual(other.Fields));
		var targetsEqual = (Targets == null && other.Targets == null) || (Targets != null && other.Targets != null && Targets.SequenceEqual(other.Targets));
		return Name == other.Name && Count == other.Count && Type == other.Type && Comment == other.Comment && Condition == other.Condition && fieldsEqual && targetsEqual;	
	}
}

public class Condition
{
	public string? Switch { get; set; }
	public Dictionary<int, List<string>>? Cases { get; set; } 
}