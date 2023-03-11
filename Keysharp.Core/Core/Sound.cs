using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//TODO

namespace Keysharp.Core
{
	/// <summary>
	/// Various functions to get information about the sound devices on the system.
	/// Most of these have variadic parameters because the parameter scheme is complex.
	/// </summary>
	public static class Sound
	{
		/// <summary>
		/// Emits a tone from the PC speaker.
		/// </summary>
		/// <param name="frequency">The frequency of the sound which should be between 37 and 32767.
		/// If omitted, the frequency will be 523.</param>
		/// <param name="duration">The duration of the sound in ms. If omitted, the duration will be 150.</param>
		public static void SoundBeep(object obj0 = null, object obj1 = null) => Console.Beep((int)obj0.Al(523), (int)obj1.Al(150));

		/// <summary>
		/// Not implemented. COM will never be cross platform anyway.//TODO
		/// </summary>
		/// <returns></returns>
		public static string SoundGetInterface() => "";

		public static object SoundGetMute(object obj0 = null, object obj1 = null)
		{
			var device = GetDevice(obj0, obj1);
			return device != null ? device.AudioEndpointVolume.Mute : (object)false;
		}

		public static string SoundGetName(object obj0 = null, object obj1 = null)
		{
			var device = GetDevice(obj0, obj1);
			return device != null ? device.FriendlyName : "";
		}

		public static double SoundGetVolume(object obj0 = null, object obj1 = null)
		{
			var device = GetDevice(obj0, obj1);
			return device != null ? (double)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100) : 0;
		}

		/// <summary>
		/// Plays a sound, video, or other supported file type.
		/// </summary>
		/// <param name="filename">
		/// <para>The name of the file to be played.</para>
		/// <para>To produce standard system sounds, specify an asterisk followed by a number as shown below.</para>
		/// <list type="bullet">
		/// <item><term>*-1</term>: <description>simple beep</description></item>
		/// <item><term>*16</term>: <description>hand (stop/error)</description></item>
		/// <item><term>*32</term>: <description>question</description></item>
		/// <item><term>*48</term>: <description>exclamation</description></item>
		/// <item><term>*64</term>: <description>asterisk (info)</description></item>
		/// </list>
		/// </param>
		/// <param name="wait"><c>true</c> to block the current thread until the sound has finished playing, false otherwise.</param>
		public static void SoundPlay(object obj0, object obj1 = null)
		{
			var filename = obj0.As();
			var wait = obj1.As();

			if (filename.Length > 1 && filename[0] == '*')
			{
				if (!int.TryParse(filename.AsSpan(1), out var n))
				{
					return;
				}

				switch (n)
				{
					case -1: SystemSounds.Beep.Play(); return;

					case 16: SystemSounds.Hand.Play(); return;

					case 32: SystemSounds.Question.Play(); return;

					case 48: SystemSounds.Exclamation.Play(); return;

					case 64: SystemSounds.Asterisk.Play(); return;

					default: return;
				}
			}

			try
			{
				var sound = new SoundPlayer(filename);

				if (wait == "1" || wait == "WAIT")
					sound.PlaySync();
				else
					sound.Play();
			}
			catch (Exception ex)
			{
				throw new Error(ex.Message);
			}
		}

		public static void SoundSetMute(object obj0, object obj1 = null, object obj2 = null)
		{
			var device = GetDevice(obj1, obj2);

			if (device != null)
			{
				var s = obj0.As();
				var plus = s.StartsWith('+');
				var minus = s.StartsWith('-');

				if (plus || minus)
					device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
				else
					device.AudioEndpointVolume.Mute = obj0.Al() > 0;
			}
			else
				throw new TargetError($"Component {obj1}, device {obj2} not found.");
		}

		public static void SoundSetVolume(object obj0, object obj1 = null, object obj2 = null)
		{
			var device = GetDevice(obj1, obj2);

			if (device != null)
			{
				var s = obj0.As();
				var vol = (float)obj0.Ad() * 0.01f;
				var plus = s.StartsWith('+');
				var minus = s.StartsWith('-');

				if (plus || minus)
					device.AudioEndpointVolume.MasterVolumeLevelScalar += vol;
				else
					device.AudioEndpointVolume.MasterVolumeLevelScalar = vol;
			}
			else
				throw new TargetError($"Component {obj1}, device {obj2} not found.");
		}

		/// <summary>
		/// The AHK documentation says this should take a component and device. Nowever, NAudio doesn't support the concept of a component
		/// so it's only possible to retrieve a device by its name or index.
		/// </summary>
		/// <param name="obj0"></param>
		/// <param name="obj1"></param>
		/// <returns></returns>
		/// <exception cref="TargetError"></exception>
		private static MMDevice GetDevice(object obj0 = null, object obj1 = null)
		{
			var component = obj0;
			var dev = obj1;
			var deviceEnum = new MMDeviceEnumerator();
			var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();

			if (component == null && dev == null)
			{
				return deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
			}
			else if (dev is string s)
			{
				foreach (var device in devices)
					if (device.FriendlyName.Contains(s))
						return device;
			}
			else
			{
				var i = dev.Ai(1);

				if (i >= 1 && i <= devices.Count)
					return devices[i - 1];
			}

			throw new TargetError("Audio device or endpoint not found.");
		}
	}
}