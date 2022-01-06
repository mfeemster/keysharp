using System;
using System.Media;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using System.Collections.Generic;
using System.Linq;

namespace Keysharp.Core
{
	public static class Sound
	{
		/// <summary>
		/// Emits a tone from the PC speaker.
		/// </summary>
		/// <param name="frequency">The frequency of the sound which should be between 37 and 32767.
		/// If omitted, the frequency will be 523.</param>
		/// <param name="duration">The duration of the sound in ms. If omitted, the duration will be 150.</param>
		public static void SoundBeep(params object[] obj)
		{
			var (frequency, duration) = obj.L().I2(523, 150);
			Console.Beep(frequency, duration);
		}

		private static MMDevice GetDevice(int offset, params object[] obj)
		{
			var o = obj.L();
			var deviceEnum = new MMDeviceEnumerator();
			var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();

			if (o.Count == offset)
			{
				var defdev = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
				return defdev;
			}
			else if (o[offset].ParseObject() is string s)
			{
				foreach (var device in devices)
					if (device.FriendlyName.Contains(s))
						return device;
			}
			else
			{
				var i = o.Ai(offset, 1);

				if (i >= 1 && i <= devices.Count)
					return devices[i - 1];
			}

			return null;
		}

		public static object SoundGetMute(params object[] obj)
		{
			var device = GetDevice(0, obj);

			if (device != null)
				return device.AudioEndpointVolume.Mute;

			//Many of these functions state that they should throw if something is wrong, but we haven't done that yet.//MATT
			return false;
		}

		public static string SoundGetName(params object[] obj)
		{
			var device = GetDevice(0, obj);

			if (device != null)
				return device.FriendlyName;

			//Many of these functions state that they should throw if something is wrong, but we haven't done that yet.//MATT
			return "";
		}

		public static double SoundGetVolume(params object[] obj)
		{
			var device = GetDevice(0, obj);

			if (device != null)
				return device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;

			//Many of these functions state that they should throw if something is wrong, but we haven't done that yet.//MATT
			return 0;
		}

		/// <summary>
		/// Retrieves the wave output volume for a sound device.
		/// </summary>
		/// <param name="output">The variable to store the result.</param>
		/// <param name="device">If this parameter is omitted, it defaults to 1 (the first sound device),
		/// which is usually the system's default device for recording and playback.
		/// Specify a higher value to operate upon a different sound device.</param>
		//public static void SoundGetWaveVolume(out int output, int device)
		//{
		//  output = 0;

		//  if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		//      return;

		//  _ = WindowsAPI.waveOutGetVolume(new IntPtr(device), out var vol);
		//  output = (int)vol;
		//  // UNDONE: cross platform SoundGetWaveVolume
		//}

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
		/// <remarks><see cref="Accessors.A_ErrorLevel"/> is set to <c>1</c> if an error occured, <c>0</c> otherwise.</remarks>
		public static void SoundPlay(params object[] obj)
		{
			var (filename, wait) = obj.L().S2();
			Accessors.A_ErrorLevel = 0;

			if (filename.Length > 1 && filename[0] == '*')
			{
				if (!int.TryParse(filename.AsSpan(1), out var n))
				{
					Accessors.A_ErrorLevel = 1;
					return;
				}

				switch (n)
				{
					case -1: SystemSounds.Beep.Play(); return;

					case 16: SystemSounds.Hand.Play(); return;

					case 32: SystemSounds.Question.Play(); return;

					case 48: SystemSounds.Exclamation.Play(); return;

					case 64: SystemSounds.Asterisk.Play(); return;

					default: Accessors.A_ErrorLevel = 1; return;
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
			catch (Exception)
			{
				Accessors.A_ErrorLevel = 1;
			}
		}

		public static void SoundSetMute(params object[] obj)
		{
			var device = GetDevice(1, obj);

			if (device != null)
			{
				var o = obj.L();
				var sval = o.S1();
				var plus = sval.StartsWith('+');
				var minus = sval.StartsWith('-');

				if (plus || minus)
				{
					device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
				}
				else
				{
					var muteval = o.I1();
					device.AudioEndpointVolume.Mute = muteval > 0;
				}
			}

			//Many of these functions state that they should throw if something is wrong, but we haven't done that yet.//MATT
		}

		public static void SoundSetVolume(params object[] obj)
		{
			var device = GetDevice(1, obj);

			if (device != null)
			{
				var o = obj.L();
				var sval = o.S1();
				var plus = sval.StartsWith('+');
				var minus = sval.StartsWith('-');
				var vol = (float)o.D1() * 0.01f;

				if (plus || minus)
					device.AudioEndpointVolume.MasterVolumeLevelScalar += vol;
				else
					device.AudioEndpointVolume.MasterVolumeLevelScalar = vol;
			}

			//Many of these functions state that they should throw if something is wrong, but we haven't done that yet.//MATT
		}

		/// <summary>
		/// Not implemented. COM will never be cross platform anyway.//MATT
		/// </summary>
		/// <returns></returns>
		public static string SoundGetInterface()
		{
			return "";
		}

		/// <summary>
		/// Changes the wave output volume for a sound device.
		/// </summary>
		/// <param name="percent">Percentage number between -100 and 100 inclusive.
		/// If the number begins with a plus or minus sign, the current volume level will be adjusted up or down by the indicated amount.</param>
		/// <param name="device">If this parameter is omitted, it defaults to 1 (the first sound device),
		/// which is usually the system's default device for recording and playback.</param>
		//public static void SoundSetWaveVolume(string percent, int device)
		//{
		//  if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		//      return;

		//  if (string.IsNullOrEmpty(percent))
		//      percent = "0";

		//  var dev = new IntPtr(device);
		//  uint vol;
		//  var p = percent[0];

		//  if (p == '+' || p == '-')
		//  {
		//      _ = WindowsAPI.waveOutGetVolume(dev, out vol);
		//      vol = (uint)(vol * double.Parse(percent.Substring(1)) / 100);
		//  }
		//  else
		//      vol = (uint)(0xfffff * (double.Parse(percent) / 100));

		//  _ = WindowsAPI.waveOutSetVolume(dev, vol);
		//  // TODO: cross platform SoundSetWaveVolume
		//}
	}
}