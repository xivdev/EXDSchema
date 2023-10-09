using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class IconPathExistsValidator : Validator
{
	public override string ValidatorName() => "IconPathExistsValidator";
	
	public IconPathExistsValidator(GameData gameData) : base(gameData) { }

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		var results = new ValidationResults();
		var fields = SchemaUtil.Flatten(exh, sheet);
		var dataFile = GameData.Excel.GetSheetRaw($"{sheet.Name}");
		
		foreach (var field in fields)
		{
			if (field.Field.Type == FieldType.Icon)
			{
				var offset = field.Definition.Offset;
				foreach (var row in dataFile.GetRowParsers())
				{
					var columnValue = ReadColumnIntegerValue(dataFile, row, field);
					if (columnValue == null)
					{
						var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} is not valid for type 'icon', failed to read.";
						results.Results.Add(ValidationResult.Failed(sheet.Name, ValidatorName(), msg));
						break; // don't spam the same error
					}
					
					if (!IconPathExists(columnValue.Value))
					{
						var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} at row {row.RowId} icon '{columnValue}' does not exist.";
						results.Results.Add(ValidationResult.Warning(sheet.Name, ValidatorName(), msg));
						break; // don't spam the same error
					}
				}
			}
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
	
	private bool IconPathExists(long iconId)
	{
		var path = $"ui/icon/{iconId / 1000 * 1000:000000}/{iconId:000000}.tex";
		return GameData.FileExists(path);
	}
}