using System.Diagnostics;
using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class ConditionValidator : Validator
{
	public override string ValidatorName() => "ConditionValidator";
	
	public ConditionValidator(GameData gameData) : base(gameData) { }

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		var results = new ValidationResults();
		var fields = SchemaUtil.Flatten(exh, sheet);
		var dataFile = GameData.Excel.GetSheetRaw($"{sheet.Name}");

		// Get all link fields with null targets (filters out target-based links)
		var conditionFields = fields.Where(f => f.Field is { Type: FieldType.Link, Condition: not null, Targets: null }).ToList();
		
		foreach (var field in conditionFields)
		{
			var offset = field.Definition.Offset;
			var switchOn = field.Field.Condition.Switch;
			var switchField = fields.First(f => f.Field.Name == switchOn);
			var definedSwitchColumnValues = field.Field.Condition.Cases.Keys.ToHashSet();
			var switchColumnValues = new HashSet<long>();
			
			foreach (var row in dataFile.GetRowParsers())
			{
				var columnValue = ReadColumnIntegerValue(dataFile, row, switchField);
				if (columnValue == null)
				{
					var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} is not valid for type 'link' condition switch, failed to read.";
					results.Results.Add(ValidationResult.Failed(sheet.Name, ValidatorName(), msg));
					break; // don't spam the same error
				}
				switchColumnValues.Add(columnValue.Value);
			}

			foreach (var switchColumnValue in switchColumnValues)
			{
				if (!definedSwitchColumnValues.Contains((int)switchColumnValue))
				{
					var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} is not valid for type 'link' condition switch, switch column {switchOn} value {switchColumnValue} is not defined in the schema.";
					results.Add(ValidationResult.Warning(sheet.Name, ValidatorName(), msg));
				}
			}

			foreach (var definedSwitchColumnValue in definedSwitchColumnValues)
			{
				if (!switchColumnValues.Contains(definedSwitchColumnValue))
				{
					var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} is not valid for type 'link' condition switch, switch column {switchOn} value {definedSwitchColumnValue} is not present in the column values.";
					results.Add(ValidationResult.Warning(sheet.Name, ValidatorName(), msg));
				}
			}
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
}