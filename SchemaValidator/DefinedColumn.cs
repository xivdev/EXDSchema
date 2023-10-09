using Lumina.Data.Structs.Excel;
using SchemaValidator.New;

namespace SchemaValidator;

public class DefinedColumn
{
	public ExcelColumnDefinition Definition { get; set; }
	public Field Field { get; set; }

	private int? _bitOffset;
	public int BitOffset {
		get
		{
			if (_bitOffset == null)
				_bitOffset = CalculateBitOffset(Definition.Offset, Definition.Type);
			return _bitOffset.Value;
		}
	}

	public override string ToString() => $"{Field} @ 0x{BitOffset / 8:X}&{BitOffset % 8}";

	public static int CalculateBitOffset(ExcelColumnDefinition def)
	{
		return CalculateBitOffset(def.Offset, def.Type);
	}
	
	public static int CalculateBitOffset(int offset, ExcelColumnDataType type)
	{
		var bitOffset = offset * 8;
		return type switch
		{
			ExcelColumnDataType.PackedBool0 => bitOffset + 0,
			ExcelColumnDataType.PackedBool1 => bitOffset + 1,
			ExcelColumnDataType.PackedBool2 => bitOffset + 2,
			ExcelColumnDataType.PackedBool3 => bitOffset + 3,
			ExcelColumnDataType.PackedBool4 => bitOffset + 4,
			ExcelColumnDataType.PackedBool5 => bitOffset + 5,
			ExcelColumnDataType.PackedBool6 => bitOffset + 6,
			ExcelColumnDataType.PackedBool7 => bitOffset + 7,
			_ => bitOffset,
		};	
	}

	
}