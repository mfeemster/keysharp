using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		internal override uint CharToVKAndModifiers(char ch, ref uint? modifiersLr, IntPtr keybdLayout, bool enableAZFallback = false) => 0;

		internal override uint ConvertMouseButton(string buf, bool allowWheel = true) => 0u;

		internal override bool IsKeyDown(uint vk) => false;

		internal override bool IsKeyDownAsync(uint vk) => false;

		internal override bool IsKeyToggledOn(uint vk) => false;

		internal override bool IsMouseVK(uint vk) => false;

		internal override bool IsWheelVK(uint vk) => false;

		internal override uint KeyToModifiersLR(uint vk, uint sc, ref bool? isNeutral) => 0;

		internal override uint MapScToVk(uint sc) => 0;

		internal override uint MapVkToSc(uint sc, bool returnSecondary = false) => 0;

		internal override void ParseClickOptions(string options, ref int x, ref int y, ref uint vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset)
		{ }

		internal override bool SystemHasAnotherKeybdHook() => false;

		internal override bool SystemHasAnotherMouseHook() => false;

		internal override uint TextToSC(string text, ref bool? specifiedByNumber) => 0u;

		internal override uint TextToSpecial(string text, ref KeyEventTypes eventType, ref uint modifiersLr, bool updatePersistent) => 0u;

		internal override uint TextToVK(string text, ref uint? modifiersLr, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout) => 0u;

		internal override bool TextToVKandSC(string text, ref uint vk, ref uint sc, ref uint? modifiersLr, IntPtr keybdLayout) => false;

		internal override char VKtoChar(uint vk, IntPtr keybdLayout) => (char)0;

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