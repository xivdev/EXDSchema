using System.Text.Json.Nodes;
using Json.Schema;
using Lumina;
using Lumina.Data.Files.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SchemaValidator.New;

namespace SchemaValidator.Validation.Validators;

public class SchemaFileValidator : Validator
{
	public override string ValidatorName() => "SchemaFileValidator";

	private readonly JsonSerializerSettings _settings = new()
	{
		NullValueHandling = NullValueHandling.Ignore,
		DefaultValueHandling = DefaultValueHandling.Ignore,
		ContractResolver = new CamelCasePropertyNamesContractResolver(),
	};

	private readonly JsonSchema _schema;
	
	public SchemaFileValidator(GameData gameData, string schemaText) : base(gameData)
	{
		_schema = JsonSchema.FromText(schemaText);
	}
	
	public override ValidationResults Validate(ExcelHeaderFile exh, Sheet sheet)
	{
		// Re-serialize the yml sheet into json
		var json = JsonConvert.SerializeObject(sheet, _settings);
		
		if (json == null) return ValidationResults.Failed(sheet.Name, ValidatorName(), "Json serialization returned null.");
		
		var node = JsonNode.Parse(json);
		var schemaResult = _schema.Evaluate(node);
		
		if (schemaResult == null) return ValidationResults.Failed(sheet.Name, ValidatorName(), "Schema validation returned null.");
		if (schemaResult.IsValid) return ValidationResults.Success(sheet.Name, ValidatorName());
		return ValidationResults.Error(sheet.Name, ValidatorName());
	}
}