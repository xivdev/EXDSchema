namespace SchemaValidator.Validation;

public enum ValidationStatus
{
	Success,
	Error,
	Warning,
	Failed,
	Info,
}

public class ValidationResults
{
	public List<ValidationResult> Results { get; set; } = new();

	public ValidationResults() { }
	public ValidationResults(ValidationResult result) => Results.Add(result);
	
	public void Add(ValidationResult result) => Results.Add(result);
	public void Add(ValidationResults results) => Results.AddRange(results.Results);
	
	public static ValidationResults Success(string sheetName, string validatorName, string message = "") => new(ValidationResult.Success(sheetName, validatorName, message));
	public static ValidationResults Error(string sheetName, string validatorName, string message = "") => new(ValidationResult.Error(sheetName, validatorName, message));
	public static ValidationResults Warning(string sheetName, string validatorName, string message = "") => new(ValidationResult.Warning(sheetName, validatorName, message));
	public static ValidationResults Failed(string sheetName, string validatorName, string message = "") => new(ValidationResult.Failed(sheetName, validatorName, message));
	public static ValidationResults Info(string sheetName, string validatorName, string message = "") => new(ValidationResult.Info(sheetName, validatorName, message));
}

public class ValidationResult
{
	public ValidationStatus Status { get; set; }
	public string SheetName { get; set; }
	public string ValidatorName { get; set; }
	public string Message { get; set; }
	
	private ValidationResult() {}
	
	public static ValidationResult Success(string sheetName, string validatorName, string message = "")
	{
		return new ValidationResult
		{
			SheetName = sheetName,
			ValidatorName = validatorName,
			Status = ValidationStatus.Success,
			Message = message,
		};
	}
	
	public static ValidationResult Error(string sheetName, string validatorName, string message = "")
	{
		return new ValidationResult
		{
			SheetName = sheetName,
			ValidatorName = validatorName,
			Status = ValidationStatus.Error,
			Message = message,
		};
	}
	
	public static ValidationResult Warning(string sheetName, string validatorName, string message = "")
	{
		return new ValidationResult
		{
			SheetName = sheetName,
			ValidatorName = validatorName,
			Status = ValidationStatus.Warning,
			Message = message,
		};
	}

	public static ValidationResult Failed(string sheetName, string validatorName, string message = "")
	{
		return new ValidationResult
		{
			SheetName = sheetName,
			ValidatorName = validatorName,
			Status = ValidationStatus.Failed,
			Message = message,
		};
	}

	public static ValidationResult Info(string sheetName, string validatorName, string message = "")
	{
		return new ValidationResult
		{
			SheetName = sheetName,
			ValidatorName = validatorName,
			Status = ValidationStatus.Info,
			Message = message,
		};
	}
}