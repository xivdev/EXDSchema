using System.Diagnostics;
using Lumina;
using Lumina.Data.Files.Excel;
using Lumina.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;

namespace SchemaValidator.Validation.Validators;

public class ConditionRefValidator : Validator
{
	public override string ValidatorName() => "ConditionRefValidator";

	private Dictionary<string, List<int>> _ignoredValues = new();
	
	public ConditionRefValidator(GameData gameData) : base(gameData) { }

	public ConditionRefValidator(GameData gameData, Dictionary<string, List<int>> ignoredValues) : base(gameData)
	{
		_ignoredValues = ignoredValues;
	}

	private class LinkTargetData
	{
		public string SourceSheet { get; set; }
		public DefinedColumn Source { get; set; }
		
		public string SwitchField { get; set; }
		public int SwitchValue { get; set; } 
			
		public HashSet<string> TargetSheets { get; set; }
		public HashSet<long> TargetKeys { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not LinkTargetData other)
				return false;
			return SourceSheet == other.SourceSheet && Source.Field.Equals(other.Source.Field) && TargetSheets == other.TargetSheets && TargetKeys.SetEquals(other.TargetKeys);
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

		// Get all link fields with condition
		var linkFields = fields.Where(f => f.Field is { Type: FieldType.Link, Condition: not null, Targets: null }).ToList();
		
		var linkVals = new List<LinkTargetData>();
		foreach (var field in linkFields)
		{
			var offset = field.Definition.Offset;
			var switchOn = field.Field.Condition.Switch;
			var switchField = fields.First(f => f.Field.Name == switchOn);
			// var definedSwitchColumnValues = field.Field.Condition.Cases.Keys.ToHashSet();

			// store the column values for each switch value
			var columnValues = new Dictionary<long, HashSet<long>>();
			
			foreach (var row in dataFile.GetRowParsers())
			{
				var columnValue = ReadColumnIntegerValue(dataFile, row, field);
				var switchColumnValue = ReadColumnIntegerValue(dataFile, row, switchField);
				if (columnValue == null || switchColumnValue == null)
				{
					var msg = $"Column {field.Field.Name}@0x{offset:X} type {field.Definition.Type} is not valid for type 'link' condition switch, failed to read.";
					results.Results.Add(ValidationResult.Failed(sheet.Name, ValidatorName(), msg));
					break; // don't spam the same error
				}
				
				if (columnValues.TryGetValue(switchColumnValue.Value, out var values))
				{
					values.Add(columnValue.Value);
				}
				else
				{
					columnValues.Add(switchColumnValue.Value, new HashSet<long> { columnValue.Value });
				}
			}

			foreach (var valueSet in columnValues)
			{
				// Skip if we don't have a defined target set for this value of the switch
				if (!field.Field.Condition.Cases.TryGetValue((int)valueSet.Key, out var switchTargets))
					continue;
				
				var targetData = new LinkTargetData
				{
					SourceSheet = sheet.Name,
					Source = field,
					TargetSheets = switchTargets.ToHashSet(),
					TargetKeys = valueSet.Value,
					SwitchField = switchField.Field.Name,
					SwitchValue = (int)valueSet.Key,
				};
				linkVals.Add(targetData);	
			}
		}
		
		foreach (var linkData in linkVals.Distinct())
		{
			var sheetNames = linkData.TargetSheets;
			var sheetKeys = linkData.TargetKeys;
			
			var dataFiles = new Dictionary<string, RawExcelSheet>();
			foreach (var sheetName in sheetNames)
			{
				var tmpDataFile = GameData.Excel.GetSheetRaw(sheetName);
				if (tmpDataFile == null)
				{
					var msg = $"Source {linkData.SourceSheet} field {linkData.Source.Field.Name}@0x{linkData.Source.Definition.Offset:X} references non-existent sheet {sheetName}.";
					results.Add(ValidationResult.Error(sheet.Name, ValidatorName(), msg));
					continue;
				}
				dataFiles.Add(sheetName, tmpDataFile);
			}
			
			foreach (var key in sheetKeys)
			{
				if (_ignoredValues.TryGetValue("all", out var ignoredKeys) && ignoredKeys.Contains((int)key))
				{
					sheetKeys.Remove(key);
					continue;
				}
				
				foreach (var linkedDataFile in dataFiles)
				{
					if (_ignoredValues.TryGetValue(linkedDataFile.Key, out ignoredKeys) && ignoredKeys.Contains((int)key))
					{
						sheetKeys.Remove(key);
					}
					else if (linkedDataFile.Value.GetRow((uint)key) != null)
					{
						// Console.WriteLine($"removing {key} because of {linkedDataFile.Key}");
						sheetKeys.Remove(key);
					}
				}
			}
			
			if (sheetKeys.Count > 0)
			{
				var contents = string.Join(", ", sheetKeys);
				var display = $"{linkData.Source.Field.Name}@0x{linkData.Source.Definition.Offset:X}";
				var msg = $"Source {linkData.SourceSheet} field {display} contains key references {contents} for case {linkData.SwitchField} = {linkData.SwitchValue} which do not exist in any linked sheet.";
				results.Add(ValidationResult.Warning(sheet.Name, ValidatorName(), msg));	
			}
		}
		
		if (results.Results.Count == 0)
			return ValidationResults.Success(sheet.Name, ValidatorName());
		return results;
	}
}