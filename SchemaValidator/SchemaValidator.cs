using Lumina;
using Lumina.Data.Files.Excel;
using SchemaValidator.New;
using SchemaValidator.Util;
using SchemaValidator.Validation;
using SchemaValidator.Validation.Validators;

namespace SchemaValidator;

public class SchemaValidator
{
	public static void Main(string[] args)
	{
		// we need 3 args
		if (args.Length != 3)
		{
			Console.WriteLine("Usage: SchemaValidator.exe <game install directory> <json schema file> <schema directory>");
			return;
		}
		
		var gameDir = args[0];
		var schemaFile = args[1];
		var schemaDir = args[2];
		
		var gameData = new GameData(gameDir);
		var schemaText = File.ReadAllText(schemaFile);

		var testDict = new Dictionary<string, List<int>>()
		{
			{"all", new() {0}},
		};
		
		var validators = new List<Validator>
		{
			new SchemaFileValidator(gameData, schemaText),
			new ColumnCountValidator(gameData),
			new IconTypeValidator(gameData),
			new NamedInnerNamedOuterValidator(gameData),
			new ModelIdTypeValidator(gameData),
			new ColorTypeValidator(gameData),
			new IconPathExistsValidator(gameData),
			new SingleLinkRefValidator(gameData, testDict),
			new MultiLinkRefValidator(gameData, testDict),
			new ConditionValidator(gameData),
			new ConditionRefValidator(gameData, testDict),
		};

		// var exl = gameData.GetFile<ExcelListFile>("exd/root.exl");
		var results = new ValidationResults();
		
		foreach (var schemaPath in Directory.GetFiles(schemaDir, "*.yml"))
		{
			var sheetName = Path.GetFileNameWithoutExtension(schemaPath);
			var exh = gameData.GetFile<ExcelHeaderFile>($"exd/{sheetName}.exh");
			if (exh == null)
			{
				Console.Error.WriteLine($"Sheet {sheetName} does not exist!");
				continue;
			}

			Sheet sheet;
			try
			{
				sheet = SerializeUtil.Deserialize<Sheet>(File.ReadAllText(schemaPath));
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Sheet {sheetName} encountered an exception when deserializing!");
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.StackTrace);
				continue;
			}
			
			if (sheet == null)
			{
				Console.Error.WriteLine($"Sheet {sheetName} could not be deserialized!");
				continue;
			}

			// SerializeUtil.EvaluateSchema(schemaPath);
			
			foreach (var validator in validators)
				results.Add(validator.Validate(exh, sheet));
		}
		
		foreach (var result in results.Results.Where(r => r.Status == ValidationStatus.Warning))
		{
			var msgfmt = result.Message == "" ? "" : $" - ";
			Console.WriteLine($"{result.Status}: {result.SheetName} - {result.ValidatorName}{msgfmt}{result.Message}");
		}
		
		foreach (var result in results.Results.Where(r => r.Status == ValidationStatus.Error))
		{
			var msgfmt = result.Message == "" ? "" : $" - ";
			Console.WriteLine($"{result.Status}: {result.SheetName} - {result.ValidatorName}{msgfmt}{result.Message}");
		}
		
		var successCount = results.Results.Count(r => r.Status == ValidationStatus.Success);
		var warningCount = results.Results.Count(r => r.Status == ValidationStatus.Warning);
		var errorCount = results.Results.Count(r => r.Status == ValidationStatus.Error);
		Console.WriteLine($"{successCount} success, {warningCount} warnings, {errorCount} errors");
	}
}