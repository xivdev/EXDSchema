// using YamlDotNet.Core;
// using YamlDotNet.Core.Events;
// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.EventEmitters;
// using YamlDotNet.Serialization.NamingConventions;
//
// namespace SchemaConverter;
//
// public static class SerializeUtil
// {
// 	private static readonly ISerializer _serializer;
//
// 	static SerializeUtil()
// 	{
// 		_serializer = new SerializerBuilder()
// 			.WithIndentedSequences()
// 			.WithNamingConvention(CamelCaseNamingConvention.Instance)
// 			.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
// 			.DisableAliases()
// 			// .WithEventEmitter(nextEmitter => new FlowEverythingEmitter(nextEmitter))
// 			.Build();
// 	}
//
// 	public static string Serialize(object o)
// 	{
// 		return _serializer.Serialize(o);
// 	}
// 	
// 	public class FlowEverythingEmitter : ChainedEventEmitter
// 	{
// 		public FlowEverythingEmitter(IEventEmitter nextEmitter) : base(nextEmitter) { }
//
// 		public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
// 		{
// 			Console.WriteLine($"Type: {eventInfo.Source.Type} Style: {eventInfo.Source.StaticType} Value: {eventInfo.Source.Value}");
// 			
// 			eventInfo.Style = MappingStyle.Flow;
// 			base.Emit(eventInfo, emitter);
// 		}
//
// 		public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
// 		{
// 			Console.WriteLine($"Type: {eventInfo.Source.Type} StaticType: {eventInfo.Source.StaticType} Value: {eventInfo.Source.Value}");
// 			eventInfo.Style = SequenceStyle.Flow;
// 			nextEmitter.Emit(eventInfo, emitter);
// 		}
// 	}
// }