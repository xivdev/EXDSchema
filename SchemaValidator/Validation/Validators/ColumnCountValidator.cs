using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class ColumnCountValidator : Validator
{
	public override string ValidatorName() => "ColumnCountValidator";
	
	public ColumnCountValidator(GameData gameData) : base(gameData) { }
	
	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		var colCount = SchemaUtil.GetColumnCount(sheet);
		if (colCount != exh.ColumnDefinitions.Length)
			return ValidationResults.Error(sheet.Name, ValidatorName(), $"Column count mismatch! exh count {exh.ColumnDefinitions.Length} != schema count {colCount}");
		return ValidationResults.Success(sheet.Name, ValidatorName());
	}
}