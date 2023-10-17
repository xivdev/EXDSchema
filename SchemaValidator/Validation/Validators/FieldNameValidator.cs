using System.Text.RegularExpressions;
using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public partial class FieldNameValidator : Validator
{
	public override string ValidatorName() => "FieldNameValidator";
	private string _sheetName = "";
	
	[GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled)]
	private static partial Regex _nameRegex();

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		_sheetName = sheet.Name;
		
		// I just don't have the brainpower to recurse right now
		var flat = SchemaUtil.Flatten(exh, sheet);
		
		var results = new ValidationResults();
		foreach (var fieldName in flat.Select(d => d.Field.Name).Distinct())
		{
			if (string.IsNullOrEmpty(fieldName))
			{
				results.Add(ValidationResult.Error(_sheetName, ValidatorName(), "Field name is empty."));
				continue;
			}

			if (!_nameRegex().IsMatch(fieldName))
				results.Results.Add(ValidationResult.Error(_sheetName, ValidatorName(), $"Field name {fieldName} is not a valid name."));
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
	
	public FieldNameValidator(GameData gameData) : base(gameData) { }
}