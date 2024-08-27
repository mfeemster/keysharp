using System.ComponentModel;
using Keysharp.Core.Windows;

namespace Keysharp.Core
{
	public static class Sound
	{
#if LINUX
		private static Dictionary<int, string> GetDevices(bool sinks)
		{
			var devices = new Dictionary<int, string>();
			var arg = sinks ? "sinks" : "sources";
			var str = $"pactl list {arg} short".Bash();

			foreach (var line in str.SplitLines())
			{
				var splits = line.Split(Keywords.SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				if (splits.Length > 1)
				{
					if (int.TryParse(splits[0], out var index))
					{
						_ = devices.GetOrAdd(index, splits[1]);
					}
				}
			}

			return devices;
		}
#endif
		/// <summary>
		/// Emits a tone from the PC speaker.
		/// </summary>
		/// <param name="frequency">The frequency of the sound which should be between 37 and 32767.
		/// If omitted, the frequency will be 523.</param>
		/// <param name="duration">The duration of the sound in ms. If omitted, the duration will be 150.</param>
		public static void SoundBeep(object obj0 = null, object obj1 = null)
		{
			var freq = obj0.Ai(523);
			var time = obj1.Ai(150);
#if LINUX
			var seconds = time / 1000.0;
			$"speaker-test -t sine -f {freq} -l 1 & sleep {seconds} && kill -9 $!".Bash();
#elif WINDOWS
			Console.Beep(freq, time);
#endif
		}

#if WINDOWS
		public static object SoundGetInterface(object obj0, object obj1 = null, object obj2 = null) => DoSound(SoundCommands.SoundGetInterface, obj0, obj1, obj2);
#endif
		public static object SoundGetMute(object obj0 = null, object obj1 = null) => DoSound(SoundCommands.SoundGetMute, obj0, obj1);

		public static object SoundGetName(object obj0 = null, object obj1 = null) => DoSound(SoundCommands.SoundGetName, obj0, obj1);

		public static object SoundGetVolume(object obj0 = null, object obj1 = null) => DoSound(SoundCommands.SoundGetVolume, obj0, obj1);

#if WINDOWS
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
#endif
		public static void SoundSetMute(object obj0, object obj1 = null, object obj2 = null) => _ = DoSound(SoundCommands.SoundSetMute, obj0, obj1, obj2);

		public static void SoundSetVolume(object obj0, object obj1 = null, object obj2 = null) => _ = DoSound(SoundCommands.SoundSetVolume, obj0, obj1, obj2);

#if LINUX
		private static object DoSound(SoundCommands soundCmd, object obj0, object obj1 = null, object obj2 = null)
		{
			var soundSet = false;
			var device = obj1;
			SoundControlType type;
			var sink = true;

			if (soundCmd >= SoundCommands.SoundSetVolume)
			{
				soundSet = true;
				type = (SoundControlType)((int)soundCmd - (int)SoundCommands.SoundSetVolume);
				sink = obj1.Ab();
				device = obj2;
			}
			else
			{
				sink = obj0.Ab();
				type = (SoundControlType)(int)soundCmd;
			}

			float settingScalar = 0.0f;

			if (soundSet)
				settingScalar = Math.Clamp(obj0.Af() * 65536.0f, -65536.0f, 65536.0f);//pactl uses a range of 0-65536.

			//var resultFloat = 0.0f;
			//var resultBool = false;
			var valStr = obj0 == null ? "" : obj0.ToString();
			var adjust = valStr.Length > 0 && (valStr[0] == '-' || valStr[0] == '+');
			var found = false;
			var sinkStr = sink ? "Sink" : "Source";
			var devices = GetDevices(sink);
			var devStr = device.ToString();

			if (device == null)
			{
				device = "@DEFAULT_SINK@";
				found = true;
			}
			else
			{
				if (!int.TryParse(devStr, out var deviceIndex))
				{
					if (!devices.TryGetValue(deviceIndex, out var deviceName))
					{
						foreach (var devKv in devices)
						{
							if (devKv.Value.StartsWith(devStr, StringComparison.OrdinalIgnoreCase))
							{
								found = true;
								break;
							}
						}
					}
				}
				else
				{
					found = true;
				}

				if (!found)
					throw new TargetError($"{sinkStr} device {device} not found.");
			}

			sinkStr = sinkStr.ToLower();

			switch (soundCmd)
			{
				case SoundCommands.SoundGetVolume:
				{
					var ret = $"pactl get-{sinkStr}-volume {device}".Bash();
					var lines = ret.SplitLines().ToList();

					if (lines.Count > 1)
					{
						var firstPercent = lines[0].IndexOf('%');
						var lastPercent = lines[0].LastIndexOf('%');
						double prc1 = 1.0, prc2 = 1.0;

						if (firstPercent != -1)
						{
							var val1Index = lines[0].Substring(0, firstPercent).LastIndexOf(' ');

							if (val1Index != -1)
							{
								var val1 = lines[0].Substring(val1Index + 1, firstPercent - val1Index);

								if (!double.TryParse(val1, out prc1))
									throw new OSError($"Could not parse first volume value of {val1}.");
							}
						}

						if (lastPercent != -1 && lastPercent > firstPercent)
						{
							var val2Index = lines[0].Substring(0, lastPercent).LastIndexOf(' ');

							if (val2Index != -1)
							{
								var val2 = lines[0].Substring(val2Index + 1, lastPercent - val2Index);

								if (!double.TryParse(val2, out prc2))
									throw new OSError($"Could not parse first volume value of {val2}.");
							}
						}

						//var balance = 1.0;
						//var splits = lines[1].Split(Keywords.SpaceTab, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
						//if (splits.Length > 1 && string.Compare(splits[0], "balance", true) == 0)
						//{
						//  if (double.TryParse(splits[splits.Length - 1], out var tempBalance))
						//      balance = tempBalance;
						//}
						return (prc1 + prc2) / 2.0;
					}
				}
				break;

				case SoundCommands.SoundGetMute:
				{
					var ret = $"pactl get-{sinkStr}-mute {device}".Bash();
					return string.Compare(ret, "yes", true) == 0 ? 1L : 0L;
				}

				case SoundCommands.SoundGetName:
				{
					if (!int.TryParse(devStr, out var deviceIndex))
					{
						if (!devices.TryGetValue(deviceIndex, out var deviceName))
							throw new TargetError($"{sinkStr} device {device} not found.");
						else
							return deviceName;
					}
					else
						throw new TargetError($"{devStr} was not a valid integer.");
				}

				case SoundCommands.SoundSetVolume:
				{
					_ = $"pactl set-{sinkStr}-volume {device} {(int)settingScalar}".Bash();
				}
				break;

				case SoundCommands.SoundSetMute:
				{
					var act = Conversions.ConvertOnOffToggle(obj0);
					_ = $"pactl set-{sinkStr}-mute {device} {(act == ToggleValueType.On ? "1" : act == ToggleValueType.Toggle ? "-1" : "0")}".Bash();
				}
				break;

				default:
					break;
			}

			return "";
		}

#elif WINDOWS
		private static object DoSound(SoundCommands soundCmd, object obj0, object obj1 = null, object obj2 = null)
		{
			var soundSet = false;
			var search = new SoundComponentSearch();
			var component = obj0;
			var device = obj1;

			if (soundCmd >= SoundCommands.SoundSetVolume)
			{
				soundSet = true;
				search.targetControl = (SoundControlType)((int)soundCmd - (int)SoundCommands.SoundSetVolume);
				component = obj1;
				device = obj2;
			}
			else
			{
				search.targetControl = (SoundControlType)(int)soundCmd;
			}

			switch (search.targetControl)
			{
				case SoundControlType.Volume:
					search.targetIid = new Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC");
					break;

				case SoundControlType.Mute:
					search.targetIid = new Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E");
					break;

				case SoundControlType.IID:
					search.targetIid = new Guid(obj0.As());
					component = obj1;
					device = obj2;
					break;
			}

			float settingScalar = 0.0f;

			if (soundSet)
				settingScalar = Math.Clamp((float)(obj0.Ad() * 0.01), -1.0f, 1.0f);

			var resultFloat = 0.0f;
			var resultBool = false;
			var valStr = obj0 == null ? "" : obj0.ToString();
			var adjust = valStr.Length > 0 && (valStr[0] == '-' || valStr[0] == '+');
			var mmDev = GetDevice(device);

			if (mmDev == null)
				throw new TargetError($"Component {component}, device {device} not found.");

			if (component == null || component.ToString().Length == 0)//Component is Master (omitted).
			{
				if (search.targetControl == SoundControlType.IID)
				{
					_ = mmDev.deviceInterface.Activate(ref search.targetIid, ClsCtx.ALL, IntPtr.Zero, out var result);
					//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
					var iptr = Marshal.GetIUnknownForObject(result);

					if (Marshal.QueryInterface(iptr, ref search.targetIid, out var ptr) >= 0)
						result = ptr;

					_ = Marshal.Release(iptr);
					return result;
				}
				else if (search.targetControl == SoundControlType.Name)
				{
					return mmDev.FriendlyName;
				}
				else
				{
					var aev = mmDev.AudioEndpointVolume;

					if (search.targetControl == SoundControlType.Volume)
					{
						if (!soundSet || adjust)
						{
							resultFloat = aev.MasterVolumeLevelScalar;
						}

						if (soundSet)
						{
							if (adjust)
								settingScalar = Math.Clamp(settingScalar + resultFloat, 0.0f, 1.0f);

							aev.MasterVolumeLevelScalar = settingScalar;
						}
						else
						{
							resultFloat *= 100;
							return (double)resultFloat;
						}
					}
					else//Mute.
					{
						if (!soundSet || adjust)
							resultBool = aev.Mute;

						if (soundSet)
							aev.Mute = adjust ? !resultBool : settingScalar > 0;
						else
							return resultBool;
					}
				}
			}
			else
			{
				if (component is int i || component is long l)
				{
					search.targetName = "";
					search.targetInstance = component.Ai();
				}
				else if (component is string cs && cs != "")
				{
					var splits = search.targetName.Split(':');

					if (splits.Length > 1)
					{
						search.targetName = splits[0];
						search.targetInstance = splits[1].Ai();
					}
					else
					{
						search.name = cs;
						search.targetInstance = 1;
					}
				}

				if (!FindComponent(mmDev, search))
				{
					throw new Error($"Component {component} not found.");
				}
				else if (search.targetControl == SoundControlType.IID)
				{
					return search.control;//The IntPtr.
				}
				else if (search.targetControl == SoundControlType.Name)
				{
					return search.name;
				}
				else if (search.control == null)
				{
					//Throw?
				}
				else if (search.targetControl == SoundControlType.Volume)
				{
					object comobj = search.control is IntPtr ip ? Marshal.GetObjectForIUnknown(ip) : search.control;

					if (comobj is IAudioVolumeLevel avl)
					{
						if (avl.GetChannelCount(out var channelCount) < 0)
							throw new Error("Could not get channel count.");

						float[] level = new float[3 * channelCount];
						float f, maxLevel = 0;

						for (var ii = 0u; ii < 0; ++ii)
						{
							if (avl.GetLevel(ii, out var db) < 0 ||
									avl.GetLevelRange(ii, out var minDb, out var maxDb, out f) < 0)
								throw new Error("Could not get level or level range.");

							//Convert dB to scalar.
							var levelMin = 0 + ii;
							var levelRange = levelMin + 0;
							level[levelMin] = (float)Math.Pow(10.0, minDb / 20.0);
							level[levelRange] = (float)Math.Pow(10.0, maxDb / 20.0) - level[levelMin];
							//Compensate for differing level ranges. (No effect if range is -96..0 dB.)
							level[ii] = ((float)Math.Pow(10.0, db / 20.0) - level[levelMin]) / level[levelRange];

							// Windows reports the highest level as the overall volume.
							if (maxLevel < level[ii])
								maxLevel = level[ii];
						}

						if (soundSet)
						{
							if (adjust)
								settingScalar = Math.Clamp(settingScalar + maxLevel, 0.0f, 1.0f);

							for (var ii = 0; ii < (uint)0; ++ii)
							{
								var levelMin = (uint)0 + ii;
								var levelRange = levelMin + 0;
								f = settingScalar;

								if (maxLevel != 0)
									f *= level[ii] / maxLevel;//Preserve balance.

								f = level[levelMin] + f * level[levelRange];//Compensate for differing level ranges.
								level[ii] = 20 * (float)Math.Log(10.0, f);//Convert scalar to dB.
							}

							Guid guid = Guid.Empty;
							_ = avl.SetLevelAllChannel(level, 0, ref guid);
						}
						else
							resultFloat = maxLevel * 100;
					}
				}
				else if (search.targetControl == SoundControlType.Mute)
				{
					object comobj = search.control is IntPtr ip ? Marshal.GetObjectForIUnknown(ip) : search.control;

					if (comobj is IAudioMute am)
					{
						var res = 0;

						if (!soundSet || adjust)
							res = am.GetMute(out resultBool);

						if (soundSet && res >= 0)
						{
							Guid guid = Guid.Empty;
							_ = am.SetMute(adjust ? !resultBool : settingScalar > 0, ref guid);
						}
					}
				}
			}

			switch (search.targetControl)
			{
				case SoundControlType.Volume:
					return (double)resultFloat;

				case SoundControlType.Mute:
					return resultBool;
			}

			return null;
		}
		private static bool FindComponent(MMDevice mmDev, SoundComponentSearch search)
		{
			search.count = 0;
			search.control = null;
			search.name = null;
			search.ignoreRemainingSubunits = false;
			var top = mmDev.DeviceTopology;

			if (top.GetConnector(0, out var conn) >= 0)
			{
				if (conn.GetDataFlow(out var flow) >= 0)
				{
					if (conn.GetConnectedTo(out var conTo) >= 0)
					{
						if (conTo is IPart part)
							_ = FindComponent(part, search);
					}
				}
			}

			return search.count == search.targetInstance;
		}
		private static bool FindComponent(IPart root, SoundComponentSearch search)
		{
			IPartsList partsList;

			if ((search.dataFlow == DataFlow.Render ?
					root.EnumPartsIncoming(out partsList) :
					root.EnumPartsOutgoing(out partsList)) < 0)
				return false;

			if (partsList.GetCount(out var partCount) < 0)
				partCount = 0;

			for (var i = 0u; i < partCount; i++)
			{
				if (partsList.GetPart(i, out var part) < 0)
					continue;

				if (root.GetPartType(out var partType) >= 0)
				{
					if (partType == PartTypeEnum.Connector)
					{
						if (partCount == 1//Ignore Connectors with no Subunits of their own.
								&& (!string.IsNullOrEmpty(search.targetName) ||
									(part.GetName(out var partName) >= 0 && partName.StartsWith(search.targetName, StringComparison.OrdinalIgnoreCase))
								   )
						   )
						{
							if (++search.count == search.targetInstance)
							{
								switch (search.targetControl)
								{
									case SoundControlType.Volume:
										break;

									case SoundControlType.Mute:
										break;

									case SoundControlType.Name:
										_ = part.GetName(out search.name);
										break;

									case SoundControlType.IID:
									{
										//Permit retrieving the IPart or IConnector itself.  Since there may be
										//multiple connected Subunits (and they can be enumerated or retrieved
										//via the Connector IPart), this is only done for the Connector.
										//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
										var iptr = Marshal.GetIUnknownForObject(part);

										if (Marshal.QueryInterface(iptr, ref search.targetIid, out var ptr) >= 0)
										{
											if (ptr != IntPtr.Zero)
												search.control = ptr;
										}

										_ = Marshal.Release(iptr);
										break;
									}
								}

								return true;
							}
						}
						else//Subunit.
						{
							//Recursively find the Connector nodes linked to this part.
							if (FindComponent(part, search))
							{
								//A matching connector part has been found with this part as one of the nodes used
								//to reach it.  Therefore, if this part supports the requested control interface,
								//it can in theory be used to control the component.  An example path might be:
								//   Output < Master Mute < Master Volume < Sum < Mute < Volume < CD Audio
								//Parts are considered from right to left, as we return from recursion.
								if (search.control == null && !search.ignoreRemainingSubunits)
								{
									//Query this part for the requested interface and let caller check the result.
									_ = part.Activate(ClsCtx.ALL, ref search.targetIid, out search.control);
									//Need the specific interface pointer, else ComCall() will fail when using IAudioMeterInformation.
									var iptr = Marshal.GetIUnknownForObject(search.control);

									if (Marshal.QueryInterface(iptr, ref search.targetIid, out var ptr) >= 0)
									{
										if (ptr != IntPtr.Zero)
											search.control = ptr;
									}

									_ = Marshal.Release(iptr);

									//If this subunit has siblings, ignore any controls further up the line
									//as they're likely shared by other components (i.e. master controls).
									if (partCount > 1)
										search.ignoreRemainingSubunits = true;
								}

								return true;
							}
						}
					}
				}
			}

			return false;
		}
		private static MMDevice GetDevice(object obj0)
		{
			var deviceEnum = new MMDeviceEnumerator();
			MMDevice mmDev = null;

			if (obj0 == null || obj0.ToString() == "")
			{
				mmDev = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
			}
			else
			{
				var targetIndex = 0;
				var targetName = "";

				if (obj0 is int || obj0 is long)
				{
					targetName = "";
					targetIndex = obj0.Ai() - 1;
				}
				else if (obj0 is string ds && ds.Length > 0)
				{
					var splits = ds.Split(':');

					if (splits.Length > 1)
					{
						targetName = splits[0];
						targetIndex = splits[1].Ai() - 1;
					}
					else
						targetName = ds;
				}

				var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active | DeviceState.Unplugged).ToList();

				if (targetName.Length > 0)
				{
					foreach (var device in devices)
					{
						//Keysharp.Core.Dialogs.MsgBox(device.FriendlyName
						//                           + "\r\n" + device.DeviceFriendlyName
						//                           + "\r\n" + device.ID
						//                           + "\r\n" + device.InstanceId);
						if (device.FriendlyName.StartsWith(targetName, StringComparison.OrdinalIgnoreCase) && targetIndex-- == 0)
						{
							mmDev = device;
							break;
						}
					}
				}
				else
				{
					if (targetIndex < devices.Count)
						mmDev = devices[targetIndex];
				}
			}

			return mmDev;
		}
		private class SoundComponentSearch
		{
			//Internal use/results:
			internal object control;
			internal int count;

			//Internal use:
			internal DataFlow dataFlow = DataFlow.Render;
			internal bool ignoreRemainingSubunits;
			internal string name;
			internal SoundControlType targetControl;

			//Parameters of search:
			internal Guid targetIid;
			internal int targetInstance;
			internal string targetName;
			// Valid only when target_control == SoundControlType::Name.
		};
#endif
		private enum SoundCommands
		{
			SoundGetVolume = 0, SoundGetMute, SoundGetName
#if WINDOWS
			, SoundGetInterface
#endif
			, SoundSetVolume, SoundSetMute
		}
		private enum SoundControlType
		{
			Volume,
			Mute,
			Name,
			IID
		};
#endif
	}
}