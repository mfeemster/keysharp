using Keysharp.Builtins;
using Keysharp.Internals.Input.Mouse;

namespace Keysharp.Internals.Input.Keyboard
{
	// Remaps use the running script's keyboard layout and share callbacks across all declarations.
	internal sealed class RemapDefinition
	{
		private static readonly string[] modifierNames = ["LCtrl", "RCtrl", "LAlt", "RAlt", "LShift", "RShift", "LWin", "RWin"];

		internal string Source { get; private init; }

		internal string Destination { get; private init; }

		internal bool DestinationIsMouse { get; private init; }

		internal bool SkipsWhileDestinationDown { get; private init; }

		internal bool IsWheel { get; private init; }

		internal string DownSend { get; private init; }

		internal string DownRelease { get; private init; }

		internal string UpSend { get; private init; }

		internal string[] RestoredModifiers { get; private init; }

		internal static void Add(Script script, string sourceKey, string targetKey, uint firmwareSourceModifiersLR, bool suspendExempt)
		{
			switch (HotkeyDefinition.TextInterpret(sourceKey, null))
			{
				case ResultType.Fail:
					return;

				case ResultType.ConditionFalse:
					_ = Errors.ValueErrorOccurred("Invalid hotkey.", sourceKey);
					return;

				case ResultType.ConditionTrue:
					HotkeyDefinition.NoteKeyNotInLayout(script, sourceKey);
					return;
			}

			Resolve(script, sourceKey, targetKey, firmwareSourceModifiersLR).Register(script, suspendExempt);
		}

		internal static RemapDefinition Resolve(Script script, string sourceKey, string targetKey, uint firmwareSourceModifiersLR)
		{
			uint remapDestVk = 0u, remapDestSc = 0u;
			uint? modLR = null, modifiersLR = null;
			var ht = script.HookThread;
			var kbLayout = new KeybdLayoutRef(); // Lazy: resolved only if a single-char remap key actually needs the layout.

			var remapName = HotkeyDefinition.TextToModifiers(targetKey, null);
			var remapDestSource = KeySource.None;
			_ = ht.TextToVKandSC(remapName, ref remapDestVk, ref remapDestSc, ref remapDestSource, ref modLR, kbLayout);

			var sourceName = HotkeyDefinition.TextToModifiers(sourceKey.TrimEnd(SpaceTab), null);
			var remapSourceVk = ht.TextToVK(sourceName, ref modifiersLR, kbLayout);
			var remapSourceIsCombo = sourceName.Contains(HotkeyDefinition.COMPOSITE_DELIMITER);
			var remapSourceIsMouse = MouseUtils.IsMouseVK(remapSourceVk);
			var remapDestIsMouse = MouseUtils.IsMouseVK(remapDestVk);
			var remapWheel = MouseUtils.IsWheelVK(remapSourceVk) || MouseUtils.IsWheelVK(remapDestVk);
			// An uppercase letter adds Shift, so that A::b differs from a::b.
			var source = (remapSourceIsCombo ? "" : "*") + (sourceName.Length == 1 && char.IsUpper(sourceName[0]) ? "+" : "") + sourceKey;
			var remapDest = remapName[0] == '"' ? "\"" : remapName;
			var remapDestModifiers = targetKey.Substring(0, targetKey.IndexOf(remapName));

			// Send requires explicit key events for sided modifiers.
			var destProperties = new HotkeyProperties();
			_ = HotkeyDefinition.TextToModifiers(targetKey, null, destProperties);
			// Send presses the left side of a neutral modifier; pressing both sides could add AltGr.
			var neutralAsLeft = 0u;

			if ((destProperties.modifiers & KeyboardUtils.MOD_CONTROL) != 0) neutralAsLeft |= KeyboardUtils.MOD_LCONTROL;

			if ((destProperties.modifiers & KeyboardUtils.MOD_ALT) != 0) neutralAsLeft |= KeyboardUtils.MOD_LALT;

			if ((destProperties.modifiers & KeyboardUtils.MOD_SHIFT) != 0) neutralAsLeft |= KeyboardUtils.MOD_LSHIFT;

			if ((destProperties.modifiers & KeyboardUtils.MOD_WIN) != 0) neutralAsLeft |= KeyboardUtils.MOD_LWIN;

			var remapDestModifiersLR = destProperties.modifiersLR | neutralAsLeft;
			// A neutral destination modifier keeps either side of its source modifier held.
			var destNamedModifiersLR = destProperties.modifiersLR | KeyboardUtils.ConvertModifiers(destProperties.modifiers);
			var remapDestIsSided = destProperties.modifiersLR != 0;
			var destModifiersDown = "";
			var destModifiersUp = "";

			if (remapDestIsSided)
				for (var i = 0; i < 8; ++i)
					if ((remapDestModifiersLR & (1u << i)) != 0)
					{
						destModifiersDown += $"{{{modifierNames[i]} down}}";
						destModifiersUp = $"{{{modifierNames[i]} up}}" + destModifiersUp;
					}

			var remapDestModifiersSend = remapDestIsSided ? destModifiersDown : remapDestModifiers;
			// The down hotkey as the hook registers it, which is where the layout adds the Shift or AltGr a character key needs.
			var remapSourceModifiersLR = new HotkeyDefinition(script, 999, null, (uint)HotkeyTypeEnum.Normal, source, 0).modifiersConsolidatedLR;
			bool? remapDestIsNeutral = null;
			var remapDestModifierLR = ht.KeyToModifiersLR(remapDestVk, remapDestSc, ref remapDestIsNeutral);
			var blindMods = "";
			var releasedSourceModifiers = new List<string>();
			var restoredSourceModifiers = new List<string>();

			for (var i = 0; i < 8; ++i)
				if ((remapSourceModifiersLR & (1 << i)) != 0 && (destNamedModifiersLR & (1u << i)) == 0)
				{
					blindMods += KeyboardMouseSender.ModLRString[i * 2];
					blindMods += KeyboardMouseSender.ModLRString[i * 2 + 1];

					// Selective Blind only releases a source modifier for the duration of one Send. If the destination
					// is itself a modifier, its useful lifetime extends through the separate up hotkey, so source-only
					// modifiers are released explicitly there.
					if ((remapDestModifierLR & (1u << i)) == 0)
					{
						releasedSourceModifiers.Add(modifierNames[i]);

						if ((firmwareSourceModifiersLR & (1u << i)) == 0)
							restoredSourceModifiers.Add(modifierNames[i]);
					}
				}

			// A wheel has no up event to release the destination, so such a remap keeps the plain press-and-release form.
			var explicitlyReleaseSourceModifiers = !remapWheel && remapDestModifierLR != 0 && releasedSourceModifiers.Count != 0;
			// Sided modifiers are scoped to this keystroke, as Send scopes neutral modifier prefixes.
			var downSend = explicitlyReleaseSourceModifiers
				? $"{{Blind}}{string.Concat(releasedSourceModifiers.Select(mod => $"{{{mod} up}}"))}{remapDestModifiersSend}{{{remapDest} DownR}}"
				: $"{{Blind{blindMods}}}{remapDestModifiersSend}{{{remapDest}{(remapWheel ? "" : " DownR")}}}";

			return new RemapDefinition
			{
				Source = source,
				Destination = remapDest,
				DestinationIsMouse = remapDestIsMouse,
				SkipsWhileDestinationDown = !remapSourceIsMouse && remapDestIsMouse && !remapWheel,
				IsWheel = remapWheel,
				DownSend = downSend,
				DownRelease = destModifiersUp.Length != 0 ? $"{{Blind}}{destModifiersUp}" : "",
				UpSend = $"{{Blind}}{{{remapDest} Up}}",
				RestoredModifiers = explicitlyReleaseSourceModifiers ? restoredSourceModifiers.ToArray() : [],
			};
		}

		internal void Register(Script script, bool suspendExempt)
		{
			if (HotkeyDefinition.AddHotkey(script, new KeysharpFunc((Func<object[], object>)Down), 0, Source, suspendExempt) == null)
				return;

			if (!IsWheel)
				_ = HotkeyDefinition.AddHotkey(script, new KeysharpFunc((Func<object[], object>)Up), 0, Source + " up", suspendExempt);
		}

		internal string DownSendFor(bool autoRepeat) => autoRepeat && !IsWheel ? string.Concat(DownSend.AsSpan(0, DownSend.Length - 1), " AutoRepeat}") : DownSend;

		private object Down(params object[] args)
		{
			SetDelay();

			if (!SkipsWhileDestinationDown || !Keysharp.Builtins.Keyboard.GetKeyState(Destination).Ab())
#if OSX
				_ = Keysharp.Builtins.Keyboard.Send(IsWheel ? DownSend : DownSendFor(Script.GetPropertyValue(Accessors.A_EventInfo, "IsAutoRepeat").Ab()));
#else
				_ = Keysharp.Builtins.Keyboard.Send(DownSend);
#endif

			if (DownRelease.Length != 0)
				_ = Keysharp.Builtins.Keyboard.Send(DownRelease);

			return "";
		}

		private object Up(params object[] args)
		{
			SetDelay();
			var send = UpSend;

			foreach (var mod in RestoredModifiers)
				if (Keysharp.Builtins.Keyboard.GetKeyState(mod, "P").Ab())
					send = string.Concat(send, "{", mod, " DownR}");

			_ = Keysharp.Builtins.Keyboard.Send(send);
			return "";
		}

		// Avoid sleeps between remapped key events.
		private void SetDelay() =>
			_ = DestinationIsMouse ? Keysharp.Builtins.Mouse.SetMouseDelay(-1L) : Keysharp.Builtins.Keyboard.SetKeyDelay(-1L);
	}
}
