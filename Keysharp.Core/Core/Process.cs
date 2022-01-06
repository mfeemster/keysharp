using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT

namespace Keysharp.Core
{
	public static class Processes
	{
		private const int LoopFrequency = 50;

		[ThreadStatic]
		private static string runDomain;

		[ThreadStatic]
		private static SecureString runPassword;

		[ThreadStatic]
		private static string runUser;

		//internal static int CurrentThreadID = Process.GetCurrentProcess().Threads[0].Id; //WindowsAPI.GetCurrentThread();
		internal static uint CurrentThreadID;

		internal static uint MainThreadID;

		internal static int MsgFilterMax() => IsInterruptible() ? 0 : WindowsAPI.WM_HOTKEY - 1;

		internal static bool IsInterruptible() => true;//Threads are going to be done totally differently in Keysharp vs. AHK. So unsure what to do with any threading related code for now.//TODO

		/// <summary>
		/// Performs one of the following operations on a process: checks if it exists; changes its priority; closes it; waits for it to close.
		/// </summary>
		/// <param name="command">
		/// <list type="bullet">
		/// <item><term>Exist</term>: <description>set <see cref="Accessors.A_ErrorLevel"/> to the process ID (PID) of the matching process;</description></item>
		/// <item><term>Close</term>: <description>terminate the process and set <see cref="Accessors.A_ErrorLevel"/> to its PID;</description></item>
		/// <item><term>Priority</term>: <description>change the priority of the process to:
		/// <c>Low</c>, <c>BelowNormal</c>, <c>Normal</c>, <c>AboveNormal</c>, <c>High</c> or <c>RealTime</c> specified in <paramref name="arg"/>;</description></item>
		/// <item><term>Wait</term>: <description>wait <paramref name="arg"/> seconds for the process to exist;</description></item>
		/// <item><term>WaitClose</term>: <description>wait <paramref name="arg"/> seconds for the process to close.</description></item>
		/// </list>
		/// </param>
		/// <param name="name">A process name or PID. Leave blank to use the current running process.</param>
		/// <param name="arg">See <paramref name="command"/>.</param>
		//public static void Process(string command, string name, string arg)
		//{
		//  var prc = string.IsNullOrEmpty(name) ? System.Diagnostics.Process.GetCurrentProcess() : Misc.FindProcess(name);
		//  var none = prc == null;
		//  const int scale = 1000;
		//  command = command.ToLowerInvariant();
		//  Accessors.A_ErrorLevel = none ? 0 : prc.Id;

		//  if (none && command != Core.Keyword_WaitClose)
		//      return;

		//  switch (command)
		//  {
		//      case Core.Keyword_Exist:
		//          break;

		//      case Core.Keyword_Close:
		//          try { prc.Kill(); }
		//          catch (System.ComponentModel.Win32Exception) { }

		//          break;

		//      case Core.Keyword_Priority:
		//          arg = string.IsNullOrEmpty(arg) ? string.Empty : arg.ToLowerInvariant();

		//          if (arg.Length == 1)
		//          {
		//              foreach (var mode in new[] { Core.Keyword_Low, Core.Keyword_BelowNormal, Core.Keyword_Normal, Core.Keyword_AboveNormal, Core.Keyword_High, Core.Keyword_Realtime })

		//                  if (mode[0] == arg[0])
		//                      arg = mode;
		//          }

		//          switch (arg.ToLowerInvariant())
		//          {
		//              case Core.Keyword_Low: prc.PriorityClass = ProcessPriorityClass.Idle; break;

		//              case Core.Keyword_BelowNormal: prc.PriorityClass = ProcessPriorityClass.BelowNormal; break;

		//              case Core.Keyword_Normal: prc.PriorityClass = ProcessPriorityClass.Normal; break;

		//              case Core.Keyword_AboveNormal: prc.PriorityClass = ProcessPriorityClass.AboveNormal; break;

		//              case Core.Keyword_High: prc.PriorityClass = ProcessPriorityClass.High; break;

		//              case Core.Keyword_Realtime: prc.PriorityClass = ProcessPriorityClass.RealTime; break;
		//          }

		//          break;

		//      case Core.Keyword_Wait:
		//      {
		//          var t = -1;

		//          if (!string.IsNullOrEmpty(arg) && double.TryParse(arg, out var d))
		//              t = (int)(d * scale);

		//          var start = Environment.TickCount;

		//          while (0 == (Accessors.A_ErrorLevel = Misc.FindProcess(name).Id))
		//          {
		//              System.Threading.Thread.Sleep(LoopFrequency);

		//              if (t != -1 && Environment.TickCount - start > t)
		//                  break;
		//          }
		//      }
		//      break;

		//      case Core.Keyword_WaitClose:
		//          if (string.IsNullOrEmpty(arg))
		//              prc.WaitForExit();
		//          else
		//          {
		//              if (double.TryParse(arg, out var d))
		//                  _ = prc.WaitForExit((int)(d * scale));
		//              else
		//                  prc.WaitForExit();
		//          }

		//          break;
		//  }
		//}

		/// <summary>
		/// Runs an external program.
		/// </summary>
		/// <param name="target">A document, URL, executable file, shortcut or system verb to launch.</param>
		/// <param name="workingDir">The working directory of the new process.</param>
		/// <param name="showMode">Optionally one of the following: <c>min</c> (minimised), <c>max</c> (maximised) or <c>hide</c> (hidden).</param>
		/// <param name="pid">The variable to store the newly created process ID.</param>
		/// <param name="wait"><c>true</c> to wait for the process to close before continuing, <c>false</c> otherwise.</param>
		//public static void Run(string target, string workingDir, string showMode, out int pid, bool wait = false)
		//{
		//  var prc = new Process
		//  {
		//      StartInfo = new ProcessStartInfo
		//      {
		//          UseShellExecute = true,
		//          FileName = target,
		//          WorkingDirectory = string.IsNullOrEmpty(workingDir) ? null : workingDir,
		//          UserName = string.IsNullOrEmpty(runUser) ? null : runUser,
		//          Domain = string.IsNullOrEmpty(runDomain) ? null : runDomain,
		//          Password = (runPassword == null || runPassword.Length == 0) ? null : runPassword,
		//      }
		//  };

		//  if (prc.StartInfo.UserName != null || prc.StartInfo.Domain != null)
		//  {
		//      prc.StartInfo.UseShellExecute = false;
		//  }

		//  var error = false;

		//  switch (showMode.ToLowerInvariant())
		//  {
		//      case Core.Keyword_Max: prc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; break;

		//      case Core.Keyword_Min: prc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; break;

		//      case Core.Keyword_Hide: prc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; break;

		//      case Core.Keyword_UseErrorLevel: error = true; break;
		//  }

		//  Accessors.A_ErrorLevel = 0;
		//  pid = 0;

		//  try
		//  {
		//      _ = prc.Start();

		//      if (wait)
		//          prc.WaitForExit();

		//      pid = prc.Id;
		//  }
		//  catch (Exception)
		//  {
		//      if (error)
		//          Accessors.A_ErrorLevel = 2;
		//      else if (wait)
		//          Accessors.A_ErrorLevel = prc.ExitCode;
		//  }
		//}

		/// <summary>
		/// Specifies a set of user credentials to use for all subsequent uses of <see cref="Run"/>.
		/// </summary>
		/// <param name="user">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="domain">The user domain.</param>
		/// <remarks>Leave all parameters blank to use no credentials.</remarks>
		public static void RunAs(params object[] obj)
		{
			var (user, password, domain) = obj.L().S3();
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
		public static void Shutdown(params object[] obj)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return;//Does linux have a shutdown command?//MATT

			_ = WindowsAPI.ExitWindowsEx((uint)obj.L().I1(), 0);
		}

		public static void ProcessClose(params object[] obj)
		{
			var name = obj.L().S1();
			var proc = string.IsNullOrEmpty(name) ? System.Diagnostics.Process.GetCurrentProcess() : Misc.FindProcess(name);//Will handle name string or pid int.

			try { proc?.Kill(); }
			catch (System.ComponentModel.Win32Exception) { }
		}

		public static long ProcessExist(params object[] obj)
		{
			var name = obj.L().S1();
			var proc = string.IsNullOrEmpty(name) ? System.Diagnostics.Process.GetCurrentProcess() : Misc.FindProcess(name);
			return proc != null ? proc.Id : 0;
		}

		public static long ProcessSetPriority(params object[] obj)
		{
			var (level, name) = obj.L().S2();
			var arg = level.ToLowerInvariant();
			var proc = string.IsNullOrEmpty(name) ? System.Diagnostics.Process.GetCurrentProcess() : Misc.FindProcess(name);

			if (proc != null)
			{
				if (arg.Length == 1)
				{
					foreach (var mode in new[] { Core.Keyword_Low, Core.Keyword_BelowNormal, Core.Keyword_Normal, Core.Keyword_AboveNormal, Core.Keyword_High, Core.Keyword_Realtime })
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
					case Core.Keyword_Low: proc.PriorityClass = ProcessPriorityClass.Idle; break;

					case Core.Keyword_BelowNormal: proc.PriorityClass = ProcessPriorityClass.BelowNormal; break;

					case Core.Keyword_Normal: proc.PriorityClass = ProcessPriorityClass.Normal; break;

					case Core.Keyword_AboveNormal: proc.PriorityClass = ProcessPriorityClass.AboveNormal; break;

					case Core.Keyword_High: proc.PriorityClass = ProcessPriorityClass.High; break;

					case Core.Keyword_Realtime: proc.PriorityClass = ProcessPriorityClass.RealTime; break;
				}

				return proc.Id;
			}

			return 0;
		}

		public static long ProcessWaitClose(params object[] obj)
		{
			var (name, timeout) = obj.L().Sd("", -1);
			var proc = Misc.FindProcess(name);

			if (proc != null)
			{
				var pid = proc.Id;

				if (timeout >= 0)
					_ = proc.WaitForExit((int)(timeout * 1000));
				else
					proc.WaitForExit();

				return pid;
			}

			return 0;
		}

		public static long ProcessWait(params object[] obj)
		{
			var (name, timeout) = obj.L().Sd("", -1);
			var t = (int)timeout;
			System.Diagnostics.Process proc;

			if (t >= 0)
				t = (int)(timeout * 1000);

			var start = Environment.TickCount;

			while ((proc = Misc.FindProcess(name)) == null)
			{
				System.Threading.Thread.Sleep(LoopFrequency);

				if (t >= 0 && Environment.TickCount - start > t)
					break;
			}

			return proc != null ? proc.Id : 0;
		}

		public static long Run(params object[] obj)
		{
			var (target, workingDir, options, arg) = obj.L().S4();
			return RunInternal(target, workingDir, options, arg);
		}

		public static long RunWait(params object[] obj)
		{
			var (target, workingDir, options, arg) = obj.L().S4();
			return RunInternal(target, workingDir, options, arg, true);
		}

		private static long RunInternal(string target, string workingDir, string showMode, string arg, bool wait = false)
		{
			var pid = 0;
			var error = false;
			Process prc = null;

			try
			{
				var splits = target.Split("\" ");
				target = splits.Length > 0 ? splits[0] : target;
				prc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						UseShellExecute = true,
						FileName = target.Trim(),
						WorkingDirectory = string.IsNullOrEmpty(workingDir) ? null : workingDir.Trim(),
						UserName = string.IsNullOrEmpty(runUser) ? null : runUser,
						Domain = string.IsNullOrEmpty(runDomain) ? null : runDomain,
						Password = (runPassword == null || runPassword.Length == 0) ? null : runPassword
					}
				};

				if (!string.IsNullOrEmpty(arg))
					prc.StartInfo.Arguments = arg.Trim();

				if (!string.IsNullOrEmpty(showMode))
				{
					switch (showMode.ToLowerInvariant())
					{
						case Core.Keyword_Max: prc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized; break;

						case Core.Keyword_Min: prc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized; break;

						case Core.Keyword_Hide: prc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; break;

						case Core.Keyword_UseErrorLevel: error = true; break;
					}
				}
				else if (prc.StartInfo.UserName != null || prc.StartInfo.Domain != null)
				{
					prc.StartInfo.UseShellExecute = false;
				}

				Accessors.A_ErrorLevel = 0;
				_ = prc.Start();
				pid = prc.Id;

				if (wait)
					prc.WaitForExit();
			}
			catch (Exception)
			{
				if (error)
					Accessors.A_ErrorLevel = 2;
				else if (wait && prc != null)
					Accessors.A_ErrorLevel = prc.ExitCode;
			}

			return pid;
		}
	}
}