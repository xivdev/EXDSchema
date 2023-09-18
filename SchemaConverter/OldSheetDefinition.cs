using Newtonsoft.Json;

namespace SchemaConverter.Old;

public class When
{
    [JsonProperty( "key" )] public string Key { get; set; }
    [JsonProperty( "value" )] public int Value { get; set; }
}

public class Link
{
    [JsonProperty( "when" )] public When When { get; set; }
    [JsonProperty( "project" )] public string Project { get; set; }
    [JsonProperty( "key" )] public string Key { get; set; }
    [JsonProperty( "sheet" )] public string LinkedSheet { get; set; }
    [JsonProperty( "sheets" )] public List< string > Sheets { get; set; }
}

public class Converter
{
    [JsonProperty( "type" )] public string Type { get; set; }
    [JsonProperty( "target" )] public string Target { get; set; }
    [JsonProperty( "links" )] public List< Link > Links { get; set; }
    [JsonProperty( "targets" )] public List< string > Targets { get; set; }
}

public class Definition
{
    [JsonProperty( "index" )] public uint Index { get; set; }
    [JsonProperty( "name" )] public string? Name { get; set; }
    [JsonProperty( "converter" )] public Converter Converter { get; set; }
    [JsonProperty( "type" )] public string Type { get; set; }
    [JsonProperty( "count" )] public int Count { get; set; }
    
    // Valid for repeats only
    [JsonProperty( "definition" )] public Definition RepeatDefinition { get; set; }
    
    // Valid for groups only
    [JsonProperty( "members" )] public List< Definition > GroupDefinitions { get; set; }

    public string GetName()
    {
        return Name ?? $"Unknown{Type.FirstCharToUpper()}{Index}";
    }
}

public class Sheet
{
    [JsonProperty( "sheet" )] public string SheetName { get; set; }
    [JsonProperty( "defaultColumn" )] public string? DefaultColumn { get; set; }
    [JsonProperty( "isGenericReferenceTarget" )] public bool IsGenericReferenceTarget { get; set; }
    [JsonProperty( "definitions" )] public List< Definition > Definitions { get; set; }
}