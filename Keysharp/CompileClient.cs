namespace Keysharp.Main;

/// <summary>
/// Thin client for the compile server. Connects to (and, on request, spawns) a daemon with a matching
/// build fingerprint, and returns compiled assembly bytes so the caller can run them via the lean path.
/// </summary>
internal static class CompileClient
{
#if WINDOWS
	private const int HANDLE_FLAG_INHERIT = 1;

	private const int STD_ERROR_HANDLE = -12;

	private const int STD_INPUT_HANDLE = -10;

	private const int STD_OUTPUT_HANDLE = -11;
#endif

	// A daemon that cannot start promptly leaves compilation to the caller.
	private static readonly TimeSpan SpawnWaitTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Compiles through the shared daemon, starting one if needed. Waiting lets concurrent launches share one
	/// compiler; DaemonCoordinator resolves racing spawns. Unavailable leaves compilation to the caller.
	/// </summary>
	internal static DaemonReply CompileViaServer(CliCommand command)
	{
		// Wait for an existing owner before spawning; do not retry a request that exhausted its deadline.
		var sw = Stopwatch.StartNew();

		if (TryCompile(command, 0) is { } reply)
			return reply;

		if (sw.Elapsed >= CompileDaemonProtocol.RequestDeadline || (!DaemonCoordinator.HasLiveOwner(CompileServer.PipeName) && !TrySpawnServer()))
			return DaemonReply.Unavailable;

		// Another attempt follows only when a daemon went away mid-request, such as one replaced by a different build.
		sw.Restart();

		do
			if (TryCompile(command, (int)Math.Max(1, (SpawnWaitTimeout - sw.Elapsed).TotalMilliseconds)) is { } answer)
				return answer;
		while (sw.Elapsed < SpawnWaitTimeout);

		return DaemonReply.Unavailable;
	}

	/// <summary>
	/// Attempts a single compile against an already-running daemon. Returns null (no exception) when no daemon
	/// answered: none is listening, or it went away mid-request. A build without a pipe, and a daemon which
	/// itself failed, answer <see cref="CompileDaemonStatus.Unavailable"/>.
	/// </summary>
	internal static DaemonReply TryCompile(CliCommand command, int connectTimeoutMs = 1000) =>
		CompileDaemonClient.TryCompile(command.CreateCompileRequest(),
			new CompileDaemonEndpoint(CompileServer.PipeName, Assembly.GetEntryAssembly()?.Location, Environment.ProcessPath, CompileDaemonClient.FindDotnetHost()),
			connectTimeoutMs, command.FromStdin ? command.ScriptDir : null,
			command.Kind == CliCommandKind.RunSource && !command.Validate ? Keysharp.Internals.Invoke.Reflections.PrewarmBuiltinTypes : null);

	/// <summary>
	/// Launches "<this host> --daemon" detached so it outlives the current process. The spawned daemon runs
	/// DaemonCoordinator.TryBecomeOwner at startup, which kills any different-build daemon and defers to an
	/// identical one, so racing spawns converge on a single owner.
	/// A daemon which cannot be spawned only means compiling in-process, so the reason is not reported.
	/// </summary>
	/// <returns></returns>
	internal static bool TrySpawnServer()
	{
		try
		{
			var processPath = Environment.ProcessPath;

			if (string.IsNullOrEmpty(processPath))
				return false;

			var psi = new ProcessStartInfo
			{
				FileName = processPath,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = Path.GetDirectoryName(processPath),
			};

			// When launched as "dotnet Keysharp.dll", re-pass the managed dll so the child runs Keysharp.
			var entryDll = Assembly.GetEntryAssembly()?.Location;

			if (Path.GetFileNameWithoutExtension(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase)
					&& !string.IsNullOrEmpty(entryDll)
					&& entryDll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				psi.ArgumentList.Add(entryDll);

			psi.ArgumentList.Add("--daemon");

			// A compile is short and allocation-heavy: Dynamic PGO's instrumented tier never pays for itself in the
			// daemon, and workstation GC leaves Roslyn's parallel binding waiting on collections (3.5 s → 1.7 s per
			// warm compile of a 24k-line script). Scripts run in the launching process, which keeps the defaults.
			psi.Environment.TryAdd("DOTNET_TieredPGO", "0");
			psi.Environment.TryAdd("DOTNET_gcServer", "1");
			psi.Environment.TryAdd("DOTNET_gcConcurrent", "0");

#if WINDOWS
			if (CompileDaemonSecurity.IsElevated)
				return CompileDaemonSecurity.TryStartUnelevated(psi);
#endif

			// Windows copies inheritable handles even when the child's streams are redirected. Clear inheritance
			// on our standard handles so the daemon cannot keep pipelines and files open after the caller exits.
			// This runs before script loading; other handles are not covered, and Process.Start must not run
			// concurrently while these process-wide flags are changed.
#if !WINDOWS
			psi.RedirectStandardInput = psi.RedirectStandardOutput = psi.RedirectStandardError = true;
#endif
			using (SuppressStandardHandleInheritance())
			{
				using var daemon = Process.Start(psi);

				if (daemon == null)
					return false;
#if !WINDOWS
				daemon.StandardInput.Close();
				daemon.StandardOutput.Close();
				daemon.StandardError.Close();
#endif
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

#if WINDOWS

	[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetHandleInformation(nint hObject, out int lpdwFlags);

	[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint GetStdHandle(int nStdHandle);

	[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetHandleInformation(nint hObject, int dwMask, int dwFlags);

	/// <summary>
	/// Clears the inheritable flag on this process's standard handles, restoring it on dispose. Scoped as
	/// tightly as possible around the spawn, since it is process-global state.
	/// </summary>
	private static IDisposable SuppressStandardHandleInheritance()
	{
		var restore = new List<(nint Handle, int Flags)>(3);

		foreach (var id in new[] { STD_INPUT_HANDLE, STD_OUTPUT_HANDLE, STD_ERROR_HANDLE })
		{
			var handle = GetStdHandle(id);

			// A GUI process launched from Explorer has no standard handles at all; nothing to do.
			if (handle is 0 or -1)
				continue;

			if (GetHandleInformation(handle, out var flags) && (flags & HANDLE_FLAG_INHERIT) != 0)
				if (SetHandleInformation(handle, HANDLE_FLAG_INHERIT, 0))
					restore.Add((handle, flags));
		}

		return new HandleInheritanceScope(restore);
	}

	private sealed class HandleInheritanceScope(List<(nint Handle, int Flags)> restore) : IDisposable
	{
		public void Dispose()
		{
			foreach (var (handle, flags) in restore)
				_ = SetHandleInformation(handle, HANDLE_FLAG_INHERIT, flags & HANDLE_FLAG_INHERIT);
		}
	}

#else

	// Only Windows needs this. On Unix, .NET opens its own descriptors close-on-exec, and TrySpawnServer replaces the
	// three standard ones with pipes of the daemon's own.
	private static IDisposable SuppressStandardHandleInheritance() => null;

#endif
}
