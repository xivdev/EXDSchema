using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;

namespace SchemaValidator.Validation.Validators;

public class NamedInnerNamedOuterValidator : Validator
{
	public override string ValidatorName() => "NamedInnerNamedOuterValidator";
	private string _sheetName = "";

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		_sheetName = sheet.Name;
		var results = new ValidationResults();
		foreach (var field in sheet.Fields)
		{
			CheckNames(results, null, field);
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
	
	public NamedInnerNamedOuterValidator(GameData gameData) : base(gameData) { }
	
	private void CheckNames(ValidationResults results, Field? parentField, Field field)
	{
		if (parentField != null)
		{
			if (parentField.Fields.Count == 1 && !string.IsNullOrEmpty(parentField.Name) && !string.IsNullOrEmpty(field.Name) && field.Type == FieldType.Scalar)
			{
				var msg = $"Parent field {parentField.Name} has a single named field {field.Name}.";
				results.Results.Add(ValidationResult.Warning(_sheetName, ValidatorName(), msg));
			}
		}
		
		if (field.Type != FieldType.Array)
		{
			// Single field
			return;
		}
		
		if (field.Type == FieldType.Array)
		{
			if (field.Fields != null)
			{
				foreach (var nestedField in field.Fields)
				{
					CheckNames(results, field, nestedField);
				}
			}
		}
	}
}