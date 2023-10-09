using Lumina;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using SchemaValidator.New;

namespace SchemaValidator.Validation;

public abstract class Validator
{
	protected GameData GameData;
	
	public Validator(GameData gameData)
	{
		GameData = gameData;
	}
	
	public abstract string ValidatorName();
	public abstract ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet);
	
	protected long? ReadColumnIntegerValue(RawExcelSheet sheet, RowParser parser, DefinedColumn column)
	{
		var offset = column.Definition.Offset;
		var type = column.Definition.Type;
		Int128? value = type switch
		{
			ExcelColumnDataType.Int8 => parser.ReadOffset<sbyte>(offset),
			ExcelColumnDataType.UInt8 => parser.ReadOffset<byte>(offset),
			ExcelColumnDataType.Int16 => parser.ReadOffset<short>(offset),
			ExcelColumnDataType.UInt16 => parser.ReadOffset<ushort>(offset),
			ExcelColumnDataType.Int32 => parser.ReadOffset<int>(offset),
			ExcelColumnDataType.UInt32 => parser.ReadOffset<uint>(offset),
			ExcelColumnDataType.Int64 => parser.ReadOffset<long>(offset),
			ExcelColumnDataType.UInt64 => parser.ReadOffset<ulong>(offset),
			_ => null,
		};

		if (value != null)
			return (long)value;
		return null;
	}
}