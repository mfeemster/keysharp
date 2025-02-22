namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal class InternalMethods
		{
			internal static MethodReference AddHotkey => new (typeof(HotkeyDefinition), "AddHotkey");
			internal static MethodReference AddHotstring => new (typeof(HotstringManager), "AddHotstring");
			internal static MethodReference CreateTrayMenu => new (typeof(Script), "CreateTrayMenu");
			internal static MethodReference Dictionary => new (typeof(Collections), "Dictionary");
			internal static MethodReference Exit => new (typeof(Flow), "Exit");
			internal static MethodReference ExitApp => new (typeof(Flow), "ExitApp");
			internal static MethodReference ExtendArray => new (typeof(Script), "ExtendArray");
			internal static MethodReference ForceBool => new (typeof(Script), "ForceBool");
			internal static MethodReference FlattenParam => new (typeof(Script), "FlattenParam");
			internal static MethodReference Func => new (typeof(Functions), "Func");
			internal static MethodReference GetMethodOrProperty => new (typeof(Script), "GetMethodOrProperty");
			internal static MethodReference GetPropertyValue => new (typeof(Script), "GetPropertyValue");
			internal static MethodReference HandleSingleInstance => new (typeof(Script), "HandleSingleInstance");
			internal static MethodReference HotIf => new (typeof(HotkeyDefinition), "HotIf");
			internal static MethodReference Hotkey => new (typeof(Keyboard), "Hotkey");
			internal static MethodReference Hotstring => new (typeof(Keyboard), "Hotstring");
            internal static MethodReference IfElse => new (typeof(Script), "IfTest");
			internal static MethodReference IfLegacy => new (typeof(Script), "IfLegacy");
			internal static MethodReference Inc => new (typeof(Loops), "Inc");
			internal static MethodReference Index => new (typeof(Script), "Index");
			internal static MethodReference Invoke => new (typeof(Script), "Invoke");
			internal static MethodReference InvokeWithRefs => new (typeof(Script), "InvokeWithRefs");
			internal static MethodReference LabelCall => new (typeof(Script), "LabelCall");
			internal static MethodReference Loop => new (typeof(Loops), "Loop");
			internal static MethodReference LoopFile => new (typeof(Loops), "LoopFile");
			internal static MethodReference LoopParse => new (typeof(Loops), "LoopParse");
			internal static MethodReference LoopRead => new (typeof(Loops), "LoopRead");
#if WINDOWS
			internal static MethodReference LoopRegistry => new (typeof(Loops), "LoopRegistry");
#endif
			internal static MethodReference MakeObjectTuple => new (typeof(Script), "MakeObjectTuple");
			internal static MethodReference MultiStatement => new (typeof(Script), "MultiStatement");
			internal static MethodReference Object => new (typeof(Objects), "Object");
			internal static MethodReference Operate => new (typeof(Script), "Operate");
			internal static MethodReference OperateTernary => new (typeof(Script), "OperateTernary");
			internal static MethodReference OperateUnary => new (typeof(Script), "OperateUnary");
			internal static MethodReference OperateZero => new (typeof(Script), "OperateZero");
			internal static MethodReference PostfixIncDecProp => new (typeof(Script), "PostfixIncDecProp");
			internal static MethodReference OrMaybe => new (typeof(Script), "OrMaybe");
			internal static MethodReference Parameter => new (typeof(Script), "Parameter");
			internal static MethodReference Parameters => new (typeof(Script), "Parameters");
			internal static MethodReference Pop => new (typeof(Loops), "Pop");
			internal static MethodReference Push => new (typeof(Loops), "Push");
			internal static MethodReference RunMainWindow => new (typeof(Script), "RunMainWindow");
			internal static MethodReference Send => new (typeof(Keyboard), "Send");
			internal static MethodReference SetObject => new (typeof(Script), "SetObject");
			internal static MethodReference SetPropertyValue => new (typeof(Script), "SetPropertyValue");
			internal static MethodReference SetReady => new (typeof(Script), "SetReady");
			internal static MethodReference StringConcat => new (typeof(string), "Concat", [typeof(object)]);
			internal static MethodReference WaitThreads => new (typeof(Script), "WaitThreads");
		}
	}
}