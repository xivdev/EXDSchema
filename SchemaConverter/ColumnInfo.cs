using Lumina.Data.Structs.Excel;

namespace SchemaConverter;

public class ColumnInfo
{
	public int BitOffset { get; set; }
	public string Name { get; set; }
	public int Index;
	public string? Type { get; set; } // icon, color etc
	public ExcelColumnDataType DataType { get; set; }
	public bool IsArrayMember { get; set; }
	public int? ArrayIndex { get; set; }
	public New.Link? Link { get; set; }
	
	public ColumnInfo() { }

	public ColumnInfo(Old.Definition def, int index, bool isArrayMember, int? arrayIndex, New.Link link)
	{
		var converterType = def.Converter?.Type;
		var nameSuffix = isArrayMember ? $"[{arrayIndex}]" : "";
		Name = Util.StripDefinitionName(def.Name);// + nameSuffix;
		Index = index;
		Type = converterType;
		IsArrayMember = isArrayMember;
		ArrayIndex = arrayIndex;
		Link = link;
	}

	public override string ToString() => $"{Name} ({Index}@{BitOffset / 8}&{BitOffset % 8}) {Type} {IsArrayMember} {ArrayIndex}";
}