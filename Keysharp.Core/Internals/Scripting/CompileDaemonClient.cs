using System.IO.Pipes;
using Keysharp.Components.Scripting;

namespace Keysharp.Internals.Scripting;

/// <summary>Reuses a compatible compile daemon without starting or replacing one.</summary>
internal static class CompileDaemonClient
{
	private static readonly Lazy<CompileDaemonEndpoint> installedEndpoint = new(FindInstalledEndpoint);

	internal static bool Enabled => Conversions.ParseBoolish(Environment.GetEnvironmentVariable("KEYSHARP_DAEMON")?.Trim())
#if DEBUG
		?? false;
#else
		?? true;
#endif

	internal static bool ShouldRetryLocally(DaemonReply reply, bool allowPackageRestore)
	{
		if (reply == null || reply.Status == CompileDaemonStatus.Unavailable
			|| (reply.Status == CompileDaemonStatus.PackageRestoreNeeded && allowPackageRestore))
			return true;
#if WINDOWS
		if (CompileDaemonSecurity.IsElevated && reply.Status is (CompileDaemonStatus.CompileFailed or CompileDaemonStatus.PackageRestoreNeeded))
			return true;
#endif
		return false;
	}

	// Wait for a recorded owner's pipe between requests, without delaying local compilation when no daemon exists.
	internal static DaemonReply TryCompile(ScriptCompileRequest request, int connectTimeoutMs = 50) =>
		CompileDaemonOwner.Read() is { } owner && installedEndpoint.Value is { } endpoint && owner.Pipe == endpoint.PipeName
			? TryCompile(request, endpoint, connectTimeoutMs) : DaemonReply.Unavailable;

	internal static DaemonReply TryCompile(ScriptCompileRequest request, CompileDaemonEndpoint endpoint,
		int connectTimeoutMs = 0, string workingDirectory = null)
	{
		if (endpoint?.PipeName == null || request.Output != ScriptCompilationOutput.InMemory || request.EmitGeneratedCode)
			return DaemonReply.Unavailable;
#if WINDOWS
		if (CompileDaemonSecurity.IsElevated && Path.GetFileNameWithoutExtension(Environment.ProcessPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			return DaemonReply.Unavailable;
		using var client = new NamedPipeClientStream(".", endpoint.PipeName, PipeDirection.InOut,
			PipeOptions.None, TokenImpersonationLevel.Identification);
#else
		using var client = new NamedPipeClientStream(".", endpoint.PipeName, PipeDirection.InOut, PipeOptions.CurrentUserOnly);
#endif
		try
		{
			client.Connect(connectTimeoutMs);
#if WINDOWS
			if (!CompileDaemonSecurity.IsExpectedServer(client, endpoint.PipeName, endpoint.ImagePath, endpoint.LauncherAssembly, endpoint.AlternateImagePath))
				return DaemonReply.Unavailable;
#endif
		}
		catch (Exception ex) when (ex is TimeoutException or IOException)
		{
			return null;
		}
		catch (Exception ex) when (ex is UnauthorizedAccessException or Win32Exception or ArgumentException or InvalidOperationException or EntryPointNotFoundException or DllNotFoundException)
		{
			return DaemonReply.Unavailable;
		}

		using var serialized = new MemoryStream();
		using var writer = new BinaryWriter(serialized, Encoding.UTF8, leaveOpen: true);
		using var reader = new BinaryReader(client, Encoding.UTF8, leaveOpen: true);
		try
		{
			var fromStdin = request.SourceText != null;
			writer.Write(fromStdin);
			writer.Write(fromStdin ? request.SourceText : Path.GetFullPath(request.ScriptPath));
			writer.Write(workingDirectory ?? Directory.GetCurrentDirectory());
			writer.Write(request.CompilationName != null);
			if (request.CompilationName != null)
				writer.Write(request.CompilationName);
			writer.Write(request.IncludeDirectory ?? "");
			CompileDaemonProtocol.WriteStrings(writer, request.Defines);
			CompileDaemonProtocol.WriteStrings(writer, request.AdditionalComponents);
			CompileDaemonProtocol.WriteStrings(writer, request.ExcludedComponents);
			writer.Write(request.CodePage);
			writer.Write(request.IncludeFile ?? "");
			CompileDaemonProtocol.WriteStrings(writer, System.Array.ConvertAll(CompileDaemonProtocol.CompileEnvironment,
				name => Environment.GetEnvironmentVariable(name) ?? ""));
			writer.Flush();
			serialized.WriteTo(client);

			switch ((CompileDaemonStatus)reader.ReadByte())
			{
				case CompileDaemonStatus.Compiled:
					var count = reader.ReadInt32();
					var bytes = reader.ReadBytes(count);
					if (bytes.Length != count)
						return null;
					return new(CompileDaemonStatus.Compiled, bytes, WarningText: reader.ReadString());
				case var status and (CompileDaemonStatus.CompileFailed or CompileDaemonStatus.PackageRestoreNeeded):
					var error = reader.ReadString();
					var errorStdOut = reader.ReadBoolean();
					return new(status, ErrorText: error, WarningText: reader.ReadString(), ErrorStdOut: errorStdOut);
				default:
					return new(CompileDaemonStatus.Unavailable, ErrorText: reader.ReadString());
			}
		}
		catch (Exception ex) when (ex is IOException or ObjectDisposedException)
		{
			return null;
		}
	}

	private static CompileDaemonEndpoint FindInstalledEndpoint()
	{
		try
		{
			var corePath = typeof(Script).Assembly.Location;
			if (string.IsNullOrEmpty(corePath))
				return null;
			var directory = Path.GetDirectoryName(corePath);
			var launcher = Path.Combine(directory, "Keysharp.dll");
			var fingerprint = CompileDaemonFingerprint.Compute(CompileDaemonFingerprint.ReadMvid(launcher), directory);
			if (fingerprint == null)
				return null;
			var image = Path.Combine(directory, OperatingSystem.IsWindows() ? "Keysharp.exe" : "Keysharp");
			var dotnet = Path.GetFileNameWithoutExtension(Environment.ProcessPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase)
				? Environment.ProcessPath : FindDotnetHost();
			if (!File.Exists(image))
				(image, dotnet) = (dotnet, null);
			return image == null ? null : new(CompileDaemonProtocol.CreatePipeName(fingerprint), launcher, image, dotnet);
		}
		catch
		{
			return null;
		}
	}

	internal static string FindDotnetHost()
	{
		var coreLibrary = typeof(object).Assembly.Location;
		if (string.IsNullOrEmpty(coreLibrary))
			return null;
		var version = Directory.GetParent(coreLibrary);
		var family = version?.Parent;
		var shared = family?.Parent;
		if (!Path.GetFileName(coreLibrary).Equals("System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase)
			|| version == null || !Version.TryParse(version.Name.Split('-')[0], out _)
			|| !string.Equals(family?.Name, "Microsoft.NETCore.App", StringComparison.OrdinalIgnoreCase)
			|| !string.Equals(shared?.Name, "shared", StringComparison.OrdinalIgnoreCase))
			return null;
		var host = Path.Combine(shared.Parent.FullName, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
		return File.Exists(host) ? host : null;
	}
}

internal sealed record CompileDaemonEndpoint(string PipeName, string LauncherAssembly, string ImagePath, string AlternateImagePath = null);

internal static class CompileDaemonProtocol
{
	internal static readonly string[] CompileEnvironment =
	[
		"AhkLibPath", "COMSPEC", "HOME", "TEMP", "TMP", "TMPDIR", "USERPROFILE",
		"XDG_CONFIG_DIRS", "XDG_CONFIG_HOME", "XDG_DESKTOP_DIR", "XDG_DOCUMENTS_DIR"
	];
	internal static readonly TimeSpan RequestDeadline = TimeSpan.FromSeconds(60);
	internal static readonly string RuntimeDirectory =
		Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") is { Length: > 0 } directory && Directory.Exists(directory)
		? directory : Path.GetTempPath();
	internal static readonly string UserKey = unchecked((uint)CompileDaemonFingerprint.Hash(Encoding.UTF8.GetBytes(
#if WINDOWS
		CompileDaemonSecurity.AccountId
#else
		Environment.UserName
#endif
	))).ToString("X8");
	internal static readonly string LockFile = Path.Combine(RuntimeDirectory, $"keysharp-compile-server-{UserKey}.lock");

	internal static string CreatePipeName(string fingerprint)
	{
		if (fingerprint == null)
			return null;
		var name = $"ksc-{fingerprint}-{UserKey}";
		return OperatingSystem.IsWindows() ? name : Path.Combine(RuntimeDirectory, name);
	}

	internal static void WriteStrings(BinaryWriter writer, IReadOnlyCollection<string> values)
	{
		writer.Write(values.Count);
		foreach (var value in values)
			writer.Write(value);
	}

	internal static string[] ReadStrings(BinaryReader reader)
	{
		var values = new string[reader.ReadInt32()];

		for (var i = 0; i < values.Length; i++)
			values[i] = reader.ReadString();

		return values;
	}
}

internal enum CompileDaemonStatus
{
	Compiled,
	CompileFailed,
	PackageRestoreNeeded,
	Unavailable
}

internal sealed record DaemonReply(CompileDaemonStatus Status, byte[] AssemblyBytes = null, string ErrorText = null,
	string WarningText = null, bool ErrorStdOut = false)
{
	internal static readonly DaemonReply Unavailable = new(CompileDaemonStatus.Unavailable);
}

internal sealed record CompileDaemonOwner(int Pid, string ProcName, long StartStamp, string Pipe)
{
	internal static CompileDaemonOwner Read()
	{
		try
		{
			if (!File.Exists(CompileDaemonProtocol.LockFile))
				return null;
			var parts = File.ReadAllText(CompileDaemonProtocol.LockFile).Split('|');
			if (parts.Length >= 4 && int.TryParse(parts[0], out var pid) && long.TryParse(parts[2], out var stamp))
				return new(pid, parts[1], stamp, parts[3]);
		}
		catch { }
		return null;
	}

	internal static bool IsOwner(string pipeName, int pid, long startStamp) =>
		Read() is { } owner && owner.Pid == pid && owner.StartStamp == startStamp
		&& string.Equals(owner.Pipe, pipeName, StringComparison.Ordinal);
}
