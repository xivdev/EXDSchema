namespace SchemaConverter;

public class DetectedArraySpecs
{
	public int Count;
	public int StartOffset { get; set; }
	public int EndOffset { get; set; }
	public List<ColumnInfo> Members { get; set; } = new();
}