using Keysharp.Components.Input;
using Microsoft.CodeAnalysis.CSharp;

namespace Keysharp.Tests;

[TestFixture, Category("Curated")]
public class KeyStateProviderTests : TestRunner
{
	[Test]
	public void StateQueries()
	{
		var root = Path.Combine(Path.GetTempPath(), "ks-input-" + Guid.NewGuid().ToString("N"));
		_ = Directory.CreateDirectory(root);

		try
		{
			var plugin = BuildProvider(root);
			var source = "#NoTrayIcon\n#ErrorStdOut\n#Warn All, StdOut\n#Warn Experimental, Off\n"
				+ "#Import Ks { Clr }\n#Include <assert>\n#Startup Registration := (Provider := Clr.Load(\"" + plugin + "\").Provider).Install(\"configured\")\n" + """
				AssertEq(GetKeyState("A"), true, A_LineNumber)
				AssertEq(GetKeyState("A", "physical"), false, A_LineNumber)
				AssertEq(GetKeyState("CapsLock", "toggle"), true, A_LineNumber)
				AssertEq(GetKeyState("LButton", "logical"), true, A_LineNumber)
				AssertEq(KeyWait("A", "T0"), true, A_LineNumber)
				AssertEq(KeyWait("A", "D T0"), false, A_LineNumber)
				AssertEq(KeyWait("A", "D L T0"), true, A_LineNumber)
				AssertEq(KeyWait("A", "L T0"), false, A_LineNumber)
				Throws(() => GetKeyState("C"), A_LineNumber, OSError)
				Throws(() => KeyWait("C", "T0"), A_LineNumber, OSError)
				continued := []
				handler := (Exception, Mode) => (continued.Push(Exception), -1)
				OnError(handler)
				GetKeyState("C")
				AssertEq(KeyWait("C", "T0"), false, A_LineNumber)
				AssertEq(KeyWait("C", "D T0"), false, A_LineNumber)
				OnError(handler, 0)
				AssertEq(continued.Length, 3, A_LineNumber)
				for caught in continued
				    Assert(caught is OSError, A_LineNumber)
				GetKeyState("Ctrl")
				AssertEq(Provider.FallbackQueries, 1, A_LineNumber)
				Registration.Dispose()
				GetKeyState("Ctrl")
				AssertEq(Provider.FallbackQueries, 1, A_LineNumber)
				FileAppend "pass", "*"
				""";
			Assert.That(RunScript(source, "input_state", true, false).Trim(), Is.EqualTo("pass"));
		}
		finally
		{
			try { Directory.Delete(root, true); }
			catch { }
		}
	}

	[Test, Category("Internal")]
	public void RegistrationOwnership()
	{
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.False);
		Assert.Throws<ArgumentNullException>(() => s.RegisterKeyStateProvider(null));
		var providers = new TrackingProvider[] { new(), new() };
		using var start = new Barrier(2);
		var registrations = providers.Select(provider => Task.Run(() =>
		{
			Assert.IsTrue(start.SignalAndWait(TimeSpan.FromSeconds(10)));
			try { s.RegisterKeyStateProvider(provider); return true; }
			catch (InvalidOperationException) { return false; }
		})).ToArray();
		Assert.IsTrue(Task.WaitAll(registrations, TimeSpan.FromSeconds(15)));
		Assert.That(registrations.Count(task => task.Result), Is.EqualTo(1));
		var winner = providers[registrations[0].Result ? 0 : 1];
		var loser = providers[registrations[0].Result ? 1 : 0];
		Assert.Throws<InvalidOperationException>(() => s.RegisterKeyStateProvider(winner));
		Assert.That(providers.Sum(provider => provider.DisposeCount), Is.Zero);
		s.Dispose();
		Assert.That(winner.DisposeCount, Is.EqualTo(1));
		Assert.Throws<ObjectDisposedException>(() => s.RegisterKeyStateProvider(loser));
		Assert.That(loser.DisposeCount, Is.Zero, "rejected registration leaves ownership with the caller");
		loser.Dispose();
		s.Dispose();
		Assert.That(providers.Select(provider => provider.DisposeCount), Is.EqualTo(new[] { 1, 1 }));
		s = new Script();
		s.Dispose();
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.Null);
		using var lateProvider = new TrackingProvider();
		Assert.Throws<ObjectDisposedException>(() => s.RegisterKeyStateProvider(lateProvider));
		Assert.That(lateProvider.DisposeCount, Is.Zero, "native-only shutdown must not acquire a rejected provider");
	}

	[Test, Category("Internal")]
	public void RegistrationRestoration()
	{
		var first = new TrackingProvider();
		var firstRegistration = s.RegisterKeyStateProvider(first);
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.True);
		firstRegistration.Dispose();
		Assert.That(first.DisposeCount, Is.EqualTo(1));
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.False);
		var second = new TrackingProvider();
		var secondRegistration = s.RegisterKeyStateProvider(second);
		firstRegistration.Dispose();
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.True,
			"a stale registration must not remove the current provider");
		Assert.That(second.DisposeCount, Is.Zero);
		s.Dispose();
		secondRegistration.Dispose();
		firstRegistration.Dispose();
		Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.Null,
			"a late registration disposal must not restore native input after shutdown");
		Assert.That(new[] { first.DisposeCount, second.DisposeCount }, Is.EqualTo(new[] { 1, 1 }));
	}

	[TestCase(false, true), TestCase(true, true), TestCase(false, false), Category("Internal")]
	public void ActiveQueryRetirement(bool remove, bool handled)
	{
		var provider = new BlockingProvider(handled);
		using var entered = provider.Entered;
		using var release = provider.Release;
		var registration = s.RegisterKeyStateProvider(provider);
		TrackingProvider replacement = null;
		var query = Task.Run(() => s.KeyState.GetState(0, KeyStateTypes.Logical));

		try
		{
			Assert.IsTrue(provider.Entered.Wait(TimeSpan.FromSeconds(10)), "the provider query did not start");
			if (remove)
			{
				registration.Dispose();
				Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.False);
				s.RegisterKeyStateProvider(replacement = new TrackingProvider());
			}
			else
			{
				s.Dispose();
				Assert.That(s.KeyState.GetState(0x41, KeyStateTypes.Logical), Is.Null,
					"shutdown must reject further queries even while an earlier query is active");
			}
			Assert.That(provider.DisposeCount, Is.Zero, "retirement must leave an admitted query's provider alive");
		}
		finally
		{
			provider.Release.Set();
			Assert.IsTrue(query.Wait(TimeSpan.FromSeconds(10)), "the provider query did not finish");
		}
		Assert.That(query.Result, Is.EqualTo(handled ? true : (bool?)null));
		Assert.That(provider.DisposeCount, Is.EqualTo(1));
		Assert.That(provider.DisposeThread, Is.EqualTo(provider.QueryThread), "the last query owns deferred disposal");
		if (remove)
		{
			Assert.That(s.KeyState.GetState(0, KeyStateTypes.Logical), Is.True);
			Assert.That(replacement.DisposeCount, Is.Zero,
				"the retired provider's final query must not dispose its replacement");
		}
		registration.Dispose();
		s.Dispose();
		Assert.That(provider.DisposeCount, Is.EqualTo(1));
	}

	private static string BuildProvider(string root)
	{
		var source = """
			using System;
			using Keysharp.Components.Input;
			using Keysharp.Runtime;
			public sealed class Provider(string Configuration) : IKeyStateProvider
			{
			    public static int FallbackQueries { get; private set; }
			    public static IDisposable Install(string Configuration) => Script.TheScript.RegisterKeyStateProvider(new Provider(Configuration));
			    public bool TryGetKeyState(uint VirtualKey, string Mode, out bool State)
			    {
			        if (Mode is not ("L" or "P" or "T")) throw new InvalidOperationException("unexpected state mode");
			        if (VirtualKey == 0x43) throw new InvalidOperationException("provider query failed");
			        State = Configuration == "configured" && (VirtualKey == 0x41 && Mode == "L"
			            || VirtualKey == 0x14 && Mode == "T" || VirtualKey == 1 && Mode == "L");
			        if (VirtualKey != 0x11) return true;
			        FallbackQueries++;
			        return false;
			    }
			}
			""";
		var compilation = CSharpCompilation.Create("InputProvider_" + Guid.NewGuid().ToString("N"),
			[CSharpSyntaxTree.ParseText(source)], CompilerHelper.CompilationReferences(root, true),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var plugin = Path.Combine(root, "state.dll");
		using var stream = File.Create(plugin);
		var emitted = compilation.Emit(stream);
		Assert.IsTrue(emitted.Success, string.Join("\n", emitted.Diagnostics));
		return plugin;
	}

	private class TrackingProvider : IKeyStateProvider, IDisposable
	{
		internal int DisposeCount;
		public virtual bool TryGetKeyState(uint VirtualKey, string Mode, out bool State) { State = true; return true; }
		public virtual void Dispose() => Interlocked.Increment(ref DisposeCount);
	}

	private sealed class BlockingProvider(bool handled) : TrackingProvider
	{
		internal readonly ManualResetEventSlim Entered = new(), Release = new();
		internal int QueryThread, DisposeThread;
		public override bool TryGetKeyState(uint VirtualKey, string Mode, out bool State)
		{
			QueryThread = Environment.CurrentManagedThreadId;
			Entered.Set();
			_ = Release.Wait(TimeSpan.FromSeconds(20));
			State = true;
			return handled;
		}
		public override void Dispose() { DisposeThread = Environment.CurrentManagedThreadId; base.Dispose(); }
	}
}
