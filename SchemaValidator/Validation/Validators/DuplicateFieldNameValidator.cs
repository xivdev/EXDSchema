using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class DuplicateFieldNameValidator : Validator
{
	public override string ValidatorName() => "DuplicateFieldNameValidator";
	
	public DuplicateFieldNameValidator(GameData gameData) : base(gameData) { }

	private string _sheetName;
	
	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		_sheetName = sheet.Name;
		
		var fieldNames = new HashSet<string>();
		var results = new ValidationResults();
		foreach (var field in sheet.Fields)
		{
			results.Add(Validate(field));
			if (fieldNames.Contains(field.Name))
				return ValidationResults.Error(sheet.Name, ValidatorName(), $"Duplicate field name {field.Name}");
			fieldNames.Add(field.Name);
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}

	private ValidationResults Validate(Field field)
	{
		var results = new ValidationResults();
		var fieldNames = new HashSet<string>();

		if (field.Fields != null)
		{
			foreach (var subField in field.Fields)
			{
				Validate(subField);
				if (fieldNames.Contains(subField.Name))
					results.Add(ValidationResult.Error(_sheetName, ValidatorName(), $"Duplicate field name {subField.Name}"));
				fieldNames.Add(subField.Name);
			}	
		}
		
		return results;
	}
}