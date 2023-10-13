using Lumina;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using Newtonsoft.Json;
using SchemaConverter.New;
using SchemaConverter.Old;

namespace SchemaConverter;

public class SchemaConverter
{
	private static readonly List<string> _genericReferenceLink = new();

	private static void EnumerateGenericReferenceTargets(string oldSchemaDir)
	{
		var targets = new List<string>();
		foreach (var oldSchemaPath in Directory.GetFiles(oldSchemaDir, "*.json"))
		{
			var oldSchema = JsonConvert.DeserializeObject<Old.Sheet>(File.ReadAllText(oldSchemaPath));
			if (oldSchema is { IsGenericReferenceTarget: true })
			{
				targets.Add(oldSchema.SheetName);
			}
		}
		_genericReferenceLink.AddRange(targets);
	}
	
	public static void Main(string[] args)
	{
		// we need 3 args
		if (args.Length != 3)
		{
			Console.WriteLine("Usage: SchemaConverter.exe <game install directory> <schema directory> <output directory>");
			return;
		}
		
		var gameDir = args[0];
		var oldSchemaDir = args[1];
		var newSchemaDir = args[2];
		
		var gameData = new GameData(gameDir);

		EnumerateGenericReferenceTargets(oldSchemaDir);
		
		foreach (var oldSchemaPath in Directory.GetFiles(oldSchemaDir, "*.json"))
		{
			var sheetName = Path.GetFileNameWithoutExtension(oldSchemaPath);
			var newSchemaPath = Path.Combine(newSchemaDir, $"{sheetName}.yml");
			Directory.CreateDirectory(Path.GetDirectoryName(newSchemaPath));
			var exh = gameData.GetFile<ExcelHeaderFile>($"exd/{sheetName}.exh");
			if (exh == null)
			{
				Console.Error.WriteLine($"Sheet {sheetName} does not exist!");
				continue;
			}
			var result = Convert(exh, oldSchemaPath, newSchemaPath);
			var strResult = result ? "succeeded!" : "failed...";
			// Console.WriteLine($"Conversion of {sheetName} {strResult}");
		}
	}

	public static bool Convert(ExcelHeaderFile exh, string oldSchemaPath, string newSchemaPath)
	{
		// Loading and validation
		var oldSchema = JsonConvert.DeserializeObject<Old.Sheet>(File.ReadAllText(oldSchemaPath));
		if (oldSchema == null)
		{
			Console.Error.WriteLine($"Failed to parse old schema for {exh.FilePath}!");
			return false;
		}
		if (oldSchema.Definitions.Count == 0)
		{
			// Console.WriteLine($"{exh.FilePath.Path} has no column definitions in old schema!");
		}

		// Load and parse the old schema to supplement exh information
		var columnInfos = new List<ColumnInfo>();
		Emit(columnInfos, oldSchema);
		var definedColumnMap = columnInfos.ToDictionary(c => c.Index, c => c);
		
		for (int i = 0; i < exh.ColumnDefinitions.Length; i++)
		{
			var definition = exh.ColumnDefinitions[i];
			if (definedColumnMap.TryGetValue(i, out var columnInfo))
			{
				columnInfo.BitOffset = Util.GetBitOffset(exh.ColumnDefinitions[i].Offset, exh.ColumnDefinitions[i].Type);
				columnInfo.DataType = definition.IsBoolType ? ExcelColumnDataType.Bool : definition.Type;
				
				// fixup ulongs lol
				if (columnInfo.DataType == ExcelColumnDataType.Int64)
					columnInfo.Type = "quad";
			}
			else
			{
				columnInfos.Add(
					new ColumnInfo
					{
						Name = $"Unknown{i}",
						Index = i,
						Type = definition.Type == ExcelColumnDataType.Int64 ? "quad" : "null",
						DataType = definition.IsBoolType ? ExcelColumnDataType.Bool : definition.Type,
						BitOffset = Util.GetBitOffset(definition.Offset, definition.Type)
					});
			}
		}

		columnInfos.Sort((c1, c2) => c1.BitOffset.CompareTo(c2.BitOffset));
		
		var columnCountsByName = new Dictionary<string, int>();
		for (int i = 0; i < columnInfos.Count; i++)
		{
			if (columnCountsByName.TryGetValue(columnInfos[i].Name, out var count))
				columnCountsByName[columnInfos[i].Name] = count + 1;
			else
				columnCountsByName[columnInfos[i].Name] = 1;
		}
		if (columnCountsByName.Any(c => c.Value > 1))
		{
			Console.WriteLine($"{oldSchema.SheetName} is a shitty fucking stupid sheet!");
		}

		var name = oldSchema?.SheetName;
		if (name == null)
			name = Path.GetFileNameWithoutExtension(exh.FilePath.Path).FirstCharToUpper();
		var newSchema = new New.Sheet
		{
			Name = name,
			DisplayField = oldSchema?.DefaultColumn,
			Fields = new List<New.Field>(),
		};
		
		foreach (var col in columnInfos)
		{
			var field = new New.Field
			{
				Name = col.Name,
				Type = col.Type switch
				{
					"quad" => FieldType.ModelId,
					"icon" => FieldType.Icon,
					"color" => FieldType.Color,
					_ => FieldType.Scalar,
				},
				Condition = col.Condition,
				Targets = col.Targets,
			};
			if (field.Condition != null || field.Targets != null)
				field.Type = FieldType.Link;
			newSchema.Fields.Add(field);
		}

		// PlaceArray(newSchema);
		
		var newSchemaStr = SerializeUtil.Serialize(newSchema);
		File.WriteAllText(newSchemaPath, newSchemaStr);
		// Console.WriteLine(newSchemaStr);
		
		return true;
	}

	// private static void PlaceArray(New.Sheet sheet)
	// {
	// 	var seenColumns = new Dictionary<string, int>();
	// 	
	// 	for (int i = 0; i < sheet.Fields.Count; i++)
	// 	{
	// 		var field = sheet.Fields[i];
	// 		
	// 		// How many times does it occur?
	// 		var occurrences = sheet.Fields.Count(f => f.Name == field.Name);
	// 		
	// 		// When does it occur?
	// 		var firstOccurrence = seenColumns.
	// 		var distance = sheet.Fields.FindIndex(i + 1, f => f.Name == field.Name) - i;
	// 		
	// 		
	// 	}
	// }
	
	private static void Emit(List<ColumnInfo> infos, Old.Sheet sheet)
	{
		var index = 0;
		foreach (var definition in sheet.Definitions)
		{
			if (index != definition.Index)
			{
				for (int i = index; i < definition.Index; i++)
				{
					infos.Add(new ColumnInfo {Name = $"Unknown{i}", Index = i });
				}
				// Console.WriteLine($"{sheet.SheetName}: skipped and generated {definition.Index - index} columns from {index} to {definition.Index}");
			}
			index = (int)definition.Index;
			if (definition.Type == null)
				EmitSingle(infos, definition, false, null, ref index);
			else if (definition.Type == "repeat")
				EmitRepeat(infos, definition, ref index);
			else if (definition.Type == "group")
				EmitGroup(infos, definition, ref index);
			else
				throw new Exception($"Unknown type {definition.Type}!");
		}
	}

	private static void EmitSingle(List<ColumnInfo> infos, Old.Definition definition, bool isArray, int? arrayIndex, ref int index)
	{
		var link = ConvertLink(definition.Converter);
		infos.Add(new ColumnInfo(definition, index++, isArray, arrayIndex, link.condition, link.targets));
	}
	
	private static void EmitRepeat(List<ColumnInfo> infos, Old.Definition definition, ref int index)
	{
		for (int i = 0; i < definition.Count; i++)
		{
			if (definition.RepeatDefinition.Type == null)
				EmitSingle(infos, definition.RepeatDefinition, true, i, ref index);
			else if (definition.RepeatDefinition.Type == "repeat")
				EmitRepeat(infos, definition.RepeatDefinition, ref index);
			else if (definition.RepeatDefinition.Type == "group")
				EmitGroup(infos, definition.RepeatDefinition, ref index);
			else
				throw new Exception($"Unknown repeat type {definition.Type}!");
		}
	}

	private static void EmitGroup(List<ColumnInfo> infos, Old.Definition definition, ref int index)
	{
		foreach (var member in definition.GroupDefinitions)
		{
			if (member.Type == null)
				EmitSingle(infos, member, false, null, ref index);
			else if (member.Type == "repeat")
				EmitRepeat(infos, member, ref index);
			else if (member.Type == "group")
				EmitGroup(infos, member, ref index);
			else
				throw new Exception($"Unknown group member type {member.Type}!");
		}
	}
	
	private static (Condition? condition, List<string>? targets) ConvertLink(Old.Converter oldLink)
	{
		if (oldLink == null) return (null, null);
		
		if (oldLink.Type == "generic")
		{
			return (null, _genericReferenceLink);
		}
		else if (oldLink.Type == "link")
		{
			return (null, new List<string> {oldLink.Target});
		}
		else if (oldLink.Type == "multiref")
		{
			return (null, oldLink.Targets);
		}
		else if (oldLink.Type == "complexlink")
		{
			if (oldLink.Links[0].Project != null)
			{
				return (null, null);
			}
			var condition = new Condition();
			condition.Switch = Util.StripDefinitionName(oldLink.Links[0].When.Key);
			condition.Cases = new Dictionary<int, List<string>>();
			foreach (var oldLinkLink in oldLink.Links)
				condition.Cases.Add(oldLinkLink.When.Value, oldLinkLink.LinkedSheet == null ? oldLinkLink.Sheets : new List<string> { oldLinkLink.LinkedSheet });
			return (condition, null);
		}
		else
		{
			return (null, null);
		}
	}
}