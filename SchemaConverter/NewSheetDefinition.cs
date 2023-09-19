// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System.ComponentModel;
using SharpYaml;
using SharpYaml.Serialization;

namespace SchemaConverter.New;

public enum FieldType
{
	Scalar,
	Array,
	Icon,
	ModelId,
	Color,
}

public class Sheet
{
	[YamlMember(0)]
	public string Name { get; set; }
	
	[YamlMember(1)]
	public string? DisplayField { get; set; }
	
	[YamlMember(2)]
	public List<Field> Fields { get; set; }
}

public class Field
{
	[YamlMember(0)]
	public string? Name { get; set; }
	
	[YamlMember(1)]
	public int? Count { get; set; }
	
	[YamlMember(2)]
	[DefaultValue(FieldType.Scalar)]
	public FieldType Type { get; set; }
	
	[YamlMember(3)]
	public string? Comment { get; set; }
	
	[YamlMember(4)]
	public List<Field>? Fields { get; set; }
	
	[YamlMember(5)]
	public Link? Link { get; set; }
}

public class Link
{
	[YamlMember(0)]
	public Condition? Condition { get; set; }
	
	[YamlMember(1)]
	[YamlStyle(YamlStyle.Flow)]
	public List<string> Target { get; set; }
}

public class Condition
{
	[YamlMember(0)]
	public string Switch { get; set; }
	
	[YamlMember(1)]
	public Dictionary<int, List<string>> Cases { get; set; } 
}