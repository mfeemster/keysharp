using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using Keysharp.Core.Common.Platform;
using Keysharp.Core.Common.Threading;
using static Keysharp.Scripting.Keywords;

namespace Keysharp.Core
{
	public static class Processes
	{
		public static System.Threading.SynchronizationContext mainContext;

		//internal static int CurrentThreadID = Process.GetCurrentProcess().Threads[0].Id; //WindowsAPI.GetCurrentThread();
		internal static uint CurrentThreadID;
		internal static uint MainThreadID;
		internal static int ManagedMainThreadID;
		internal static string runDomain;
		internal static SecureString runPassword;
		internal static string runUser;

		private const int LoopFrequency = 50;

		public static long ProcessClose(object obj)
		{
			var name = obj.As();
			var proc = string.IsNullOrEmpty(name) ? Process.GetCurrentProcess() : FindProcess(name);//Will handle name string or pid int.

			if (proc == null)
				return 0L;

			try
			{
				proc.Kill();
				return proc.Id;
			}
			catch (System.ComponentModel.Win32Exception) { }

			return 0L;
		}

		public static long ProcessExist(object obj = null)
		{
			var name = obj.As();
			var proc = string.IsNullOrEmpty(name) ? Process.GetCurrentProcess() : FindProcess(name);
			return proc != null ? proc.Id : 0L;
		}

		public static long ProcessSetPriority(object obj0, object obj1 = null)
		{
			var level = obj0.As();
			var name = obj1.As();
			var arg = level.ToLowerInvariant();
			var proc = string.IsNullOrEmpty(name) ? Process.GetCurrentProcess() : FindProcess(name);

			if (proc != null)
			{
				if (arg.Length == 1)
				{
					foreach (var mode in new[] { Keyword_Low, Keyword_BelowNormal, Keyword_Normal, Keyword_AboveNormal, Keyword_High, Keyword_Realtime })
					{
						if (mode[0] == arg[0])
						{
							arg = mode;
							break;
						}
					}
				}

				switch (arg)
				{
					case Keyword_Low: proc.PriorityClass = ProcessPriorityClass.Idle; break;

					case Keyword_BelowNormal: proc.PriorityClass = ProcessPriorityClass.BelowNormal; break;

					case Keyword_Normal: proc.PriorityClass = ProcessPriorityClass.Normal; break;

					case Keyword_AboveNormal: proc.PriorityClass = ProcessPriorityClass.AboveNormal; break;

					case Keyword_High: proc.PriorityClass = ProcessPriorityClass.High; break;

					case Keyword_Realtime: proc.PriorityClass = ProcessPriorityClass.RealTime; break;
				}

				return proc.Id;
			}

			return 0;
		}

		public static long ProcessWait(object obj0, object obj1 = null)
		{
			var name = obj0.As();
			var timeout = obj1.Ad(-1.0);
			var t = timeout;
			System.Diagnostics.Process proc;

			if (t >= 0)
				t = timeout * 1000;

			var start = DateTime.Now;

			while ((proc = FindProcess(name)) == null)
			{
				Flow.Sleep(LoopFrequency);

				if (t >= 0.0 && (DateTime.Now - start).TotalMilliseconds > t)
					break;
			}

			return proc != null ? proc.Id : 0;
		}

		public static long ProcessWaitClose(object obj0, object obj1 = null)
		{
			var name = obj0.As();
			var timeout = obj1.Ad(-1.0);
			var proc = FindProcess(name);

			if (proc != null)
			{
				var pid = proc.Id;

				if (timeout >= 0)
					_ = proc.WaitForExit((int)(timeout * 1000));
				else
					proc.WaitForExit();

				return pid;
			}

			return 0L;
		}

		/// <summary>
		/// Overload that is needed because ref parameters can't be optional.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="workingDir"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static long Run(object target, object workingDir = null, object options = null)
		{
			object pid = null;
			return Run(target, workingDir, options, ref pid, null);
		}

		public static long Run(object target, object workingDir, object options, ref object outputVarPID, object args = null)
		{
			return RunInternal(target.As(), workingDir.As(), options.As(), ref outputVarPID, args.As());
		}

		/// <summary>
		/// Specifies a set of user credentials to use for all subsequent uses of <see cref="Run"/>.
		/// </summary>
		/// <param name="user">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="domain">The user domain.</param>
		/// <remarks>Leave all parameters blank to use no credentials.</remarks>
		public static void RunAs(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var user = obj0.As();
			var password = obj1.As();
			var domain = obj2.As();
			runUser = user;
			runDomain = domain;

			if (string.IsNullOrEmpty(password))
			{
				runPassword = null;
			}
			else
			{
				runPassword = new SecureString();

				foreach (var sym in password)
					runPassword.AppendChar(sym);

				runPassword.MakeReadOnly();
			}
		}

		/// <summary>
		/// Overload that is needed because ref parameters can't be optional.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="workingDir"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static long RunWait(object target, object workingDir = null, object options = null)
		{
			object pid = null;
			return RunWait(target, workingDir, options, ref pid, null);
		}

		public static long RunWait(object target, object workingDir, object options, ref object outputVarPID, object args = null)
		{
			return RunInternal(target.As(), workingDir.As(), options.As(), ref outputVarPID, args.As(), true);
		}

		/// <summary>
		/// Shuts down, restarts, or logs off the system.
		/// <param name="code">A combination of the following codes:
		/// <list type="bullet">
		/// <item><term>0</term>: <description>logoff</description></item>
		/// <item><term>1</term>: <description>shutdown - appears not to work</description></item>
		/// <item><term>2</term>: <description>reboot</description></item>
		/// <item><term>4</term>: <description>force</description></item>
		/// <item><term>8</term>: <description>power down</description></item>
		/// </list>
		/// </param>
		/// </summary>
		public static void Shutdown(object obj)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return;//Does linux have a shutdown command?//TODO

			_ = PlatformProvider.Manager.ExitProgram((uint)obj.Al(), 0);
		}

		//internal static int MsgFilterMax() => Threads.IsInterruptible() ? 0 : WindowsAPI.WM_HOTKEY - 1;

		private static Process FindProcess(string name)
		{
			if (int.TryParse(name, out var id))
			{
				try
				{
					return Process.GetProcessById(id);
				}
				catch
				{
					return null;
				}
			}

			const string exe = ".exe";

			if (name.EndsWith(exe, StringComparison.OrdinalIgnoreCase))
				name = name.Substring(0, name.Length - exe.Length);

			try
			{
				var prc = Process.GetProcessesByName(name);
				return prc.Length > 0 ? prc[0] : null;
			}
			catch
			{
				return null;
			}
		}

		private static long RunInternal(string target, string workingDir, string showMode, ref object outputVarPID, string args, bool wait = false)
		{
			var pid = 0;
			var isadmin = false;

			try
			{
				if (target.StartsWith("*runas ", StringComparison.OrdinalIgnoreCase))
				{
					isadmin = true;
					target = target.ReplaceFirst("*runas ", "", StringComparison.OrdinalIgnoreCase);
				}

				var parsedArgs = "";

				if (target.StartsWith('"'))
				{
					var nextQuote = target.IndexOf('"', 1);

					if (nextQuote > 0)
					{
						parsedArgs = target.Substring(nextQuote + 1).Trim();
						target = target.Substring(0, nextQuote + 1).Trim();
					}
					else
						target += '"';//Add the quote because it was missing, which is very unlikely.
				}
				else
				{
					var nextSpace = target.IndexOf(' ', 1);

					if (nextSpace > 0)
					{
						parsedArgs = target.Substring(nextSpace + 1).Trim();
						target = target.Substring(0, nextSpace).Trim();
					}
				}

				var tv = Threads.GetThreadVariables();
				var prc = new Process//Unsure what to do about this on linux.//TODO
				{
					StartInfo = new ProcessStartInfo
					{
						UseShellExecute = true,
						FileName = target,
						WorkingDirectory = string.IsNullOrEmpty(workingDir) ? null : workingDir.Trim(),
						UserName = string.IsNullOrEmpty(runUser) ? null : runUser,
						Domain = string.IsNullOrEmpty(runDomain) ? null : runDomain,
						Password = (runPassword == null || runPassword.Length == 0) ? null : runPassword
					}
				};

				if (!string.IsNullOrEmpty(args))
					prc.StartInfo.Arguments = args.Trim();
				else if (parsedArgs.Length > 0)
					prc.StartInfo.Arguments = parsedArgs;

				if (!string.IsNullOrEmpty(showMode))
				{
					switch (showMode.ToLowerInvariant())
					{
						case Keyword_Max: prc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; break;

						case Keyword_Min: prc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; break;

						case Keyword_Hide: prc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; break;
					}
				}
				else if (prc.StartInfo.UserName != null || prc.StartInfo.Domain != null)
				{
					prc.StartInfo.UseShellExecute = false;
				}

				if (isadmin)
				{
					prc.StartInfo.UseShellExecute = true;
					prc.StartInfo.Verb = "runas";
				}

				if (prc.Start())
				{
					pid = prc.Id;

					if (wait)
					{
						prc.WaitForExit();
						outputVarPID = pid;
						return prc.ExitCode;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Error(ex.Message);
			}

			outputVarPID = pid;
			return 0L;
		}
	}
}