namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal class InternalMethods
		{
			internal static MethodReference AddHotkey => new MethodReference(typeof(Keysharp.Core.Common.Keyboard.HotkeyDefinition), "AddHotkey");
			internal static MethodReference AddHotstring => new MethodReference(typeof(Keysharp.Core.Common.Keyboard.HotstringManager), "AddHotstring");
			internal static MethodReference CreateTrayMenu => new MethodReference(typeof(Script), "CreateTrayMenu");
			internal static MethodReference Dictionary => new MethodReference(typeof(Keysharp.Core.Misc), "Dictionary");
			internal static MethodReference Exit => new MethodReference(typeof(Core.Flow), "Exit");
			internal static MethodReference ExitApp => new MethodReference(typeof(Core.Flow), "ExitApp");
			internal static MethodReference ExtendArray => new MethodReference(typeof(Script), "ExtendArray");
			internal static MethodReference ForceBool => new MethodReference(typeof(Script), "ForceBool");
			internal static MethodReference GetMethodOrProperty => new MethodReference(typeof(Script), "GetMethodOrProperty");
			internal static MethodReference GetPropertyValue => new MethodReference(typeof(Script), "GetPropertyValue");
			internal static MethodReference HandleSingleInstance => new MethodReference(typeof(Script), "HandleSingleInstance");
			internal static MethodReference Hotkey => new MethodReference(typeof(Core.Keyboard), "Hotkey");
			internal static MethodReference Hotstring => new MethodReference(typeof(Core.Keyboard), "Hotstring");
			internal static MethodReference IfElse => new MethodReference(typeof(Script), "IfTest");
			internal static MethodReference IfLegacy => new MethodReference(typeof(Script), "IfLegacy");
			internal static MethodReference Inc => new MethodReference(typeof(Loops), "Inc");
			internal static MethodReference Index => new MethodReference(typeof(Script), "Index");
			internal static MethodReference Invoke => new MethodReference(typeof(Script), "Invoke");
			internal static MethodReference InvokeWithRefs => new MethodReference(typeof(Script), "InvokeWithRefs");
			internal static MethodReference LabelCall => new MethodReference(typeof(Script), "LabelCall");
			internal static MethodReference Loop => new MethodReference(typeof(Core.Loops), "Loop");
			internal static MethodReference LoopEach => new MethodReference(typeof(Core.Loops), "LoopEach");
			internal static MethodReference LoopFile => new MethodReference(typeof(Core.Loops), "LoopFile");
			internal static MethodReference LoopParse => new MethodReference(typeof(Core.Loops), "LoopParse");
			internal static MethodReference LoopRead => new MethodReference(typeof(Core.Loops), "LoopRead");
#if WINDOWS
			internal static MethodReference LoopRegistry => new MethodReference(typeof(Core.Loops), "LoopRegistry");
#endif
			internal static MethodReference MakeObjectTuple => new MethodReference(typeof(Script), "MakeObjectTuple");
			internal static MethodReference Operate => new MethodReference(typeof(Script), "Operate");
			internal static MethodReference OperateTernary => new MethodReference(typeof(Script), "OperateTernary");
			internal static MethodReference OperateUnary => new MethodReference(typeof(Script), "OperateUnary");
			internal static MethodReference OperateZero => new MethodReference(typeof(Script), "OperateZero");
			internal static MethodReference OrMaybe => new MethodReference(typeof(Script), "OrMaybe");
			internal static MethodReference Parameter => new MethodReference(typeof(Script), "Parameter");
			internal static MethodReference Parameters => new MethodReference(typeof(Script), "Parameters");
			internal static MethodReference Pop => new MethodReference(typeof(Loops), "Pop");
			internal static MethodReference Push => new MethodReference(typeof(Loops), "Push");
			internal static MethodReference RunMainWindow => new MethodReference(typeof(Script), "RunMainWindow");
			internal static MethodReference Send => new MethodReference(typeof(Core.Keyboard), "Send");
			internal static MethodReference SetObject => new MethodReference(typeof(Script), "SetObject");
			internal static MethodReference SetPropertyValue => new MethodReference(typeof(Script), "SetPropertyValue");
			internal static MethodReference SetReady => new MethodReference(typeof(Script), "SetReady");
			internal static MethodReference StringConcat => new MethodReference(typeof(string), "Concat", [typeof(object)]);
		}
	}
}