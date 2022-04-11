using System;
using System.Collections.Generic;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Core.Linux
{
	internal class LinuxHookThread : Keysharp.Core.Common.Threading.HookThread
	{
		public override void SimulateKeyPress(uint key)
		{
		}
		internal override void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false)
		{ }

		internal override void ChangeHookState(List<HotkeyDefinition> hk, HookType whichHook, HookType whichHookAlways)
		{ }

		internal override int CharToVKAndModifiers(char ch, ref int? modifiersLr, IntPtr keybdLayout, bool enableAZFallback = false) => 0;

		internal override bool IsKeyDown(int vk) => false;

		internal override bool IsKeyDownAsync(int vk) => false;

		internal override bool IsKeyToggledOn(int vk) => false;

		internal override bool IsMouseVK(int vk) => false;

		internal override bool IsWheelVK(int vk) => false;

		internal override int KeyToModifiersLR(int vk, int sc, ref bool? isNeutral) => 0;

		internal override int MapScToVk(int sc) => 0;

		internal override int MapVkToSc(int sc, bool returnSecondary = false) => 0;

		internal override void ParseClickOptions(string options, ref int x, ref int y, ref int vk, ref KeyEventTypes eventType, ref int repeatCount, ref bool moveOffset)
		{ }

		internal override bool SystemHasAnotherKeybdHook() => false;

		internal override bool SystemHasAnotherMouseHook() => false;

		internal override int TextToSC(string text, ref bool? specifiedByNumber) => 0;

		internal override int TextToSpecial(string text, ref KeyEventTypes eventType, ref int modifiersLr, bool updatePersistent) => 0;

		internal override int TextToVK(string text, ref int? modifiersLr, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout) => 0;

		internal override bool TextToVKandSC(string text, ref int vk, ref int sc, ref int? modifiersLr, IntPtr keybdLayout) => false;

		internal override char VKtoChar(int vk, IntPtr keybdLayout) => (char)0;

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