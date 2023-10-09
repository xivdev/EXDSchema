using Lumina;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class ModelIdTypeValidator : Validator
{
	public override string ValidatorName() => "ModelIdTypeValidator";
	
	private readonly HashSet<ExcelColumnDataType> _validTypes = new()
	{
		ExcelColumnDataType.UInt32,
		ExcelColumnDataType.UInt64,
	};
	
	public ModelIdTypeValidator(GameData gameData) : base(gameData) { }

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		var results = new ValidationResults();
		var fields = SchemaUtil.Flatten(exh, sheet);
		foreach (var field in fields)
		{
			if (field.Field.Type == FieldType.ModelId && !_validTypes.Contains(field.Definition.Type))
			{
				var msg = $"Column {field.Field.Name}@0x{field.Definition.Offset:X} type {field.Definition.Type} is not valid for type 'modelId'.";
				results.Results.Add(ValidationResult.Error(sheet.Name, ValidatorName(), msg));
			}
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
}