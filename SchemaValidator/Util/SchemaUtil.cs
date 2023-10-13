using Lumina.Data.Files.Excel;
using SchemaValidator.New;

namespace SchemaValidator.Util;

/// <summary>
/// Useful methods for working with the EXDSchema object model.
/// </summary>
public static class SchemaUtil
{
	public static int GetColumnCount(Sheet sheet)
	{
		var total = 0;
		foreach (var field in sheet.Fields)
			total += GetFieldCount(field);
		return total;
	}

	public static List<DefinedColumn> Flatten(ExcelHeaderFile exh, Sheet sheet)
	{
		var fields = new List<DefinedColumn>();
		foreach (var field in sheet.Fields)
			Emit(fields, field);

		var exhDefList = exh.ColumnDefinitions.ToList();
		exhDefList.Sort((c1, c2) => DefinedColumn.CalculateBitOffset(c1).CompareTo(DefinedColumn.CalculateBitOffset(c2)));
		
		var min = Math.Min(exhDefList.Count, fields.Count);
		for(int i = 0; i < min; i++)
		{
			var field = fields[i];
			field.Definition = exhDefList[i];
		}

		return fields;
	}

	private static void Emit(List<DefinedColumn> list, Field field, string nameOverride = "")
	{
		if (field.Type != FieldType.Array)
		{
			// Single field
			list.Add(new DefinedColumn { Field = CreateField(field, nameOverride) });
		}
		else if (field.Type == FieldType.Array)
		{
			// We can have an array without fields, it's just scalars
			if (field.Fields == null)
			{
				for (int i = 0; i < field.Count.Value; i++)
				{
					list.Add(new DefinedColumn { Field = CreateField(field, "") });	
				}
			}
			else
			{
				for (int i = 0; i < field.Count.Value; i++)
				{
					foreach (var nestedField in field.Fields)
					{
						Emit(list, nestedField, field.Name);
					}	
				}
			}
		}
	}

	private static Field CreateField(Field baseField, string nameOverride)
	{
		var addedField = new Field
		{
			Name = baseField.Name,
			Comment = baseField.Comment,
			Count = null,
			Type = baseField.Type == FieldType.Array ? FieldType.Scalar : baseField.Type,
			Fields = null,
			Condition = baseField.Condition,
			Targets = baseField.Targets,
		};
		
		// This is for unnamed inner fields of arrays such as arrays of links
		// We don't want to override the name of unnamed scalars though
		if (baseField.Name == null && baseField.Type != FieldType.Scalar && nameOverride != "")
			addedField.Name = nameOverride;
		return addedField;
	}

	private static int GetFieldCount(Field field)
	{
		if (field.Type == FieldType.Array)
		{
			var total = 0;
			if (field.Fields != null)
			{
				foreach (var nestedField in field.Fields)
					total += GetFieldCount(nestedField);
			}
			else
			{
				total = 1;
			}
			return total * field.Count.Value;
		}
		return 1;
	}
}