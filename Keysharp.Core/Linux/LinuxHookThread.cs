#if LINUX
using EvDevSharp;
using EvDevSharp.Wrappers.Keyboard;
using EvDevSharp.Wrappers.Mouse;

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of HookThread for the linux platfrom.
	/// </summary>
	internal class LinuxHookThread : Keysharp.Core.Common.Threading.HookThread
	{
		internal LinuxHookThread()
		{
			/*
			    EvDev.RegisterDevices<EvDevMouseDevice>();
			    EvDev.RegisterDevices<EvDevKeyboardDevice>();
			    var kbs = EvDev.GetRegisteredDevices<EvDevKeyboardDevice>().OrderBy(d => d.DevicePath).ToList();
			    var mice = EvDev.GetRegisteredDevices<EvDevMouseDevice>().OrderBy(d => d.DevicePath).ToList();

			    foreach (var kb in kbs)
			    {
			        kb.OnKeyEvent += (s, e) =>
			        {
			            Keysharp.Scripting.Script.OutputDebug($"You pressed {e.Key}\tState: {e.Value}");
			        };
			    }
			*/
		}

		public override void SimulateKeyPress(uint key)
		{
		}

		internal override void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false)
		{ }

		internal override void ChangeHookState(List<HotkeyDefinition> hk, HookType whichHook, HookType whichHookAlways)
		{ }

		internal override uint CharToVKAndModifiers(char ch, ref uint? modifiersLr, nint keybdLayout, bool enableAZFallback = false) => 0;

		internal override uint ConvertMouseButton(ReadOnlySpan<char> buf, bool allowWheel = true) => 0u;

		internal override bool IsKeyDown(uint vk) => false;

		internal override bool IsKeyDownAsync(uint vk) => false;

		internal override bool IsKeyToggledOn(uint vk) => false;

		internal override bool IsMouseVK(uint vk) => false;

		internal override bool IsWheelVK(uint vk) => false;

		internal override uint KeyToModifiersLR(uint vk, uint sc, ref bool? isNeutral) => 0;

		internal override uint MapScToVk(uint sc) => 0;

		internal override uint MapVkToSc(uint sc, bool returnSecondary = false) => 0;

		internal override void ParseClickOptions(ReadOnlySpan<char> options, ref int x, ref int y, ref uint vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset)
		{ }

		internal override bool SystemHasAnotherKeybdHook() => false;

		internal override bool SystemHasAnotherMouseHook() => false;

		internal override uint TextToSC(ReadOnlySpan<char> text, ref bool? specifiedByNumber) => 0u;

		internal override uint TextToSpecial(ReadOnlySpan<char> text, ref KeyEventTypes eventType, ref uint modifiersLr, bool updatePersistent) => 0u;

		internal override uint TextToVK(ReadOnlySpan<char> text, ref uint? modifiersLr, bool excludeThoseHandledByScanCode, bool allowExplicitVK, nint keybdLayout) => 0u;

		internal override bool TextToVKandSC(ReadOnlySpan<char> text, ref uint vk, ref uint sc, ref uint? modifiersLr, nint keybdLayout) => false;

		internal override void Unhook() { }

		internal override void Unhook(nint hook) { }

		internal override char VKtoChar(uint vk, nint keybdLayout) => (char)0;

		internal override void WaitHookIdle()
		{ }

		protected internal override void DeregisterHooks()
		{
		}

		//protected internal override void DeregisterKeyboardHook()
		//{
		//}
		//protected internal override void DeregisterMouseHook()
		//{
		//}

		protected internal override void Start()
		{
		}
	}
}

#endif