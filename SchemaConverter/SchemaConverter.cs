using Lumina;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using Newtonsoft.Json;
using SchemaConverter.New;
using SchemaConverter.Old;

namespace SchemaConverter;

public class SchemaConverter
{
	private static readonly New.Link _genericReferenceLink = new() {Target = new List<string>()};

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
		_genericReferenceLink.Target.AddRange(targets);
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
			Console.WriteLine($"Conversion of {sheetName} {strResult}");
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
			Console.WriteLine($"{exh.FilePath.Path} has no column definitions in old schema!");
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
						Type = definition.Type == ExcelColumnDataType.Int64 ? null : "quad",
						DataType = definition.IsBoolType ? ExcelColumnDataType.Bool : definition.Type,
						BitOffset = Util.GetBitOffset(definition.Offset, definition.Type)
					});
			}
		}

		columnInfos.Sort((c1, c2) => c1.BitOffset.CompareTo(c2.BitOffset));

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
				Link = col.Link,
			};
			newSchema.Fields.Add(field);
		}
		
		var newSchemaStr = SerializeUtil3.Serialize(newSchema);
		File.WriteAllText(newSchemaPath, newSchemaStr);
		
		return true;
	}

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
				Console.WriteLine($"{sheet.SheetName}: skipped and generated {definition.Index - index} columns from {index} to {definition.Index}");
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
		infos.Add(new ColumnInfo(definition, index++, isArray, arrayIndex, ConvertLink(definition.Converter)));
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
	
	private static New.Link ConvertLink(Old.Converter oldLink)
	{
		if (oldLink == null) return null;
		
		var newLink = new New.Link();
		if (oldLink.Type == "generic")
		{
			return _genericReferenceLink;
		}
		else if (oldLink.Type == "link")
		{
			newLink.Target = new List<string>() {oldLink.Target};
		}
		else if (oldLink.Type == "multiref")
		{
			newLink.Target = oldLink.Targets;
		}
		else if (oldLink.Type == "complexlink")
		{
			if (oldLink.Links[0].Project != null)
			{
				return null;
			}
			newLink.Condition = new Condition();
			newLink.Condition.Switch = oldLink.Links[0].When.Key;
			newLink.Condition.Cases = new Dictionary<int, List<string>>();
			foreach (var oldLinkLink in oldLink.Links)
				newLink.Condition.Cases.Add(oldLinkLink.When.Value, oldLinkLink.Sheets);
		}
		else
		{
			return null;
		}

		return newLink;
	}
}