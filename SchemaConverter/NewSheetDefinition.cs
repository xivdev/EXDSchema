// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace SchemaConverter.New;

public enum FieldType
{
	scalar,
	array,
	icon,
	modelId,
	color,
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
	public string? Comment { get; set; }
	public FieldType Type { get; set; }
	public List<Field>? Fields { get; set; }
	public Link? Link { get; set; }
}

public class Link
{
	public List<string> Target { get; set; }
	public Condition? Condition { get; set; }
}

public class Condition
{
	public string Switch { get; set; }
	public Dictionary<int, List<string>> Cases { get; set; } 
}