using System.Diagnostics;
using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class SingleLinkRefValidator : Validator
{
	public override string ValidatorName() => "SingleLinkRefValidator";

	private Dictionary<string, List<int>> _ignoredValues = new();
	
	public SingleLinkRefValidator(GameData gameData) : base(gameData) { }

	public SingleLinkRefValidator(GameData gameData, Dictionary<string, List<int>> ignoredValues) : base(gameData)
	{
		_ignoredValues = ignoredValues;
	}

	private class LinkTargetData
	{
		public string SourceSheet { get; set; }
		public DefinedColumn Source { get; set; }
		
		public string TargetSheet { get; set; }
		public HashSet<long> TargetKeys { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not LinkTargetData other)
				return false;
			return SourceSheet == other.SourceSheet && Source.Field.Equals(other.Source.Field) && TargetSheet == other.TargetSheet && TargetKeys.SetEquals(other.TargetKeys);
		}
	}

	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		var results = new ValidationResults();
		var fields = SchemaUtil.Flatten(exh, sheet);
		var dataFile = GameData.Excel.GetSheetRaw($"{sheet.Name}");
		if (dataFile == null)
		{
			var msg = $"Failed to obtain sheet {sheet.Name} from game data.";
			results.Results.Add(ValidationResult.Failed(sheet.Name, ValidatorName(), msg));
			return results;
		}

		// Get all link fields with null condition, 1 target
		var linkFields = fields.Where(f => f.Field is { Type: FieldType.Link, Condition: null, Targets.Count: 1 }).ToList();
		
		var linkVals = new List<LinkTargetData>();
		foreach (var field in linkFields)
		{
			var offset = field.Definition.Offset;
			
			var columnValues = new HashSet<long>();
			
			foreach (var row in dataFile.GetRowParsers())
			{
				var columnValue = ReadColumnIntegerValue(dataFile, row, field);
				if (columnValue == null)
				{
					var msg = $"Row {row.RowId} column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} failed to read properly.";
					results.Results.Add(ValidationResult.Failed(sheet.Name, ValidatorName(), msg));
					break; // don't spam the same error
				}
				columnValues.Add(columnValue.Value);
			}

			// our filtering ensures we have exactly one target
			var targetData = new LinkTargetData
			{
				SourceSheet = sheet.Name,
				Source = field,
				TargetSheet = field.Field.Targets[0],
				TargetKeys = columnValues,
			};
			linkVals.Add(targetData);
		}
		
		foreach (var linkData in linkVals.Distinct())
		{
			var sheetName = linkData.TargetSheet;
			var sheetKeys = linkData.TargetKeys;
			
			var linkedDataFile = GameData.Excel.GetSheetRaw(sheetName);
			if (linkedDataFile == null)
			{
				var msg = $"Source {linkData.SourceSheet} field {linkData.Source.Field.Name}@0x{linkData.Source.Definition.Offset:X} references non-existent sheet {sheetName}.";
				results.Add(ValidationResult.Error(sheet.Name, ValidatorName(), msg));
				continue;
			}
			
			foreach (var key in sheetKeys)
			{
				if (_ignoredValues.TryGetValue(sheetName, out var ignoredKeys) && ignoredKeys.Contains((int)key)) continue;
				if (_ignoredValues.TryGetValue("all", out ignoredKeys) && ignoredKeys.Contains((int)key)) continue;
				
				if (linkedDataFile.GetRow((uint)key) == null)
				{
					var display = $"{linkData.Source.Field.Name}@0x{linkData.Source.Definition.Offset:X}";
					var msg = $"Source {linkData.SourceSheet} field {display} references {sheetName} key '{key}' which does not exist.";
					results.Add(ValidationResult.Warning(sheet.Name, ValidatorName(), msg));
				}
			}
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
}