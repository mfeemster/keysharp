using static Keysharp.Core.Accessors;
using static Keysharp.Core.COM.Com;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
using static Keysharp.Core.Dll;
using static Keysharp.Core.Drive;
using static Keysharp.Core.EditX;
using static Keysharp.Core.Env;
using static Keysharp.Core.Errors;
using static Keysharp.Core.External;
using static Keysharp.Core.Files;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Functions;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Images;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Input;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Processes;
using static Keysharp.Core.RealThreads;
using static Keysharp.Core.RegEx;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Types;
using static Keysharp.Core.WindowX;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script.Operator;
using static Keysharp.Scripting.Script;
using static Keysharp.Core.Common.Invoke.Reflections;

[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.8")]
namespace Keysharp.CompiledMain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Keysharp.Core;
    using Keysharp.Core.Common;
    using Keysharp.Core.Common.File;
    using Keysharp.Core.Common.Invoke;
    using Keysharp.Core.Common.ObjectBase;
    using Keysharp.Core.Common.Strings;
    using Keysharp.Core.Common.Threading;
    using Keysharp.Scripting;
    using Array = Keysharp.Core.Array;
    using Buffer = Keysharp.Core.Buffer;

    public class program
    {
        public static object classobj;
        public static object classobj2;
        public static object classobj3;
        public static object a;
        public static object temp;
        public static object val;
        public static object numtypesize;
        public class myclass : KeysharpObject
        {
            public myclass(params object[] args)
            {
                __New(args);
            }

            public static string __Class
            {
                get
                {
                    return "myclass";
                }
            }

            static public object a { get; set; }
            static public object b { get; set; }
            static public object c { get; set; }
            static public object x { get; set; }
            static public object y { get; set; }

            protected override void __Init()
            {
                base.__Init();
            }

            static myclass()
            {
                a = null;
                b = "";
                c = "asdf";
                x = 123L;
                y = x;
                __StaticInit();
            }

            public static myclass Call(params object[] args)
            {
                return new myclass(args);
            }

            public object __init { get; set; }
        }

        public class typesizemapper : KeysharpObject
        {
            public typesizemapper(params object[] args)
            {
                __New(args);
            }

            public static string __Class
            {
                get
                {
                    return "typesizemapper";
                }
            }

            static public object numtypesize { get; set; }

            static public object Mapinit()
            {
                temp = Keysharp.Core.Collections.Map();
                {
                    Keysharp.Core.Loops.Push();
                    object t;
                    object _;
                    for (var _ks_e1 = Keysharp.Core.Loops.MakeEnumerator(new Keysharp.Core.Array(new object[] { new Keysharp.Core.Array(new object[] { 1L, "Int8", "char" }), new Keysharp.Core.Array(new object[] { 1L, "UInt8", "uchar" }), new Keysharp.Core.Array(new object[] { 2L, "Int16", "short" }), new Keysharp.Core.Array(new object[] { 2L, "UInt16", "ushort" }), new Keysharp.Core.Array(new object[] { 4L, "Int32", "int" }), new Keysharp.Core.Array(new object[] { 4L, "UInt32", "uint" }), new Keysharp.Core.Array(new object[] { 8L, "Int64", "int64" }), new Keysharp.Core.Array(new object[] { 8L, "UInt64", "uint64" }), new Keysharp.Core.Array(new object[] { 4L, "Single", "float" }), new Keysharp.Core.Array(new object[] { 8L, "Double", "double" }), new Keysharp.Core.Array(new object[] { A_PtrSize, "IntPtr", "ptr" }), new Keysharp.Core.Array(new object[] { A_PtrSize, "UIntPtr", "uptr" }) }), 1); IsTrueAndRunning(_ks_e1.MoveNext());)
                    {
                        (t, _) = _ks_e1.Current;
                        Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Index(t, 1L), temp, Keysharp.Scripting.Script.Index(t, 3L));
                    _ks_e1_next:
                        ;
                    }

                _ks_e1_end:
                    Keysharp.Core.Loops.Pop();
                }

                return temp;
            }

            protected override void __Init()
            {
                base.__Init();
            }

            static typesizemapper()
            {
                __StaticInit();
            }

            public static typesizemapper Call(params object[] args)
            {
                return new typesizemapper(args);
            }

            public object __init { get; set; }
        }

        public class typesizemapper2 : KeysharpObject
        {
            public typesizemapper2(params object[] args)
            {
                __New(args);
            }

            public static string __Class
            {
                get
                {
                    return "typesizemapper2";
                }
            }

            static public object numtypesize { get; set; }

            static public object __Staticinit()
            {
                numtypesize = Keysharp.Core.Collections.Map();
                {
                    Keysharp.Core.Loops.Push();
                    object t;
                    object _;
                    for (var _ks_e1 = Keysharp.Core.Loops.MakeEnumerator(new Keysharp.Core.Array(new object[] { new Keysharp.Core.Array(new object[] { 1L, "Int8", "char" }), new Keysharp.Core.Array(new object[] { 1L, "UInt8", "uchar" }), new Keysharp.Core.Array(new object[] { 2L, "Int16", "short" }), new Keysharp.Core.Array(new object[] { 2L, "UInt16", "ushort" }), new Keysharp.Core.Array(new object[] { 4L, "Int32", "int" }), new Keysharp.Core.Array(new object[] { 4L, "UInt32", "uint" }), new Keysharp.Core.Array(new object[] { 8L, "Int64", "int64" }), new Keysharp.Core.Array(new object[] { 8L, "UInt64", "uint64" }), new Keysharp.Core.Array(new object[] { 4L, "Single", "float" }), new Keysharp.Core.Array(new object[] { 8L, "Double", "double" }), new Keysharp.Core.Array(new object[] { A_PtrSize, "IntPtr", "ptr" }), new Keysharp.Core.Array(new object[] { A_PtrSize, "UIntPtr", "uptr" }) }), 1); IsTrueAndRunning(_ks_e1.MoveNext());)
                    {
                        (t, _) = _ks_e1.Current;
                        Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Index(t, 1L), numtypesize, Keysharp.Scripting.Script.Index(t, 3L));
                    _ks_e1_next:
                        ;
                    }

                _ks_e1_end:
                    Keysharp.Core.Loops.Pop();
                }

                return "";
            }

            protected override void __Init()
            {
                base.__Init();
            }

            static typesizemapper2()
            {
                numtypesize = "";
                __StaticInit();
            }

            public static typesizemapper2 Call(params object[] args)
            {
                return new typesizemapper2(args);
            }

            public object __init { get; set; }
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = "*";
                Keysharp.Scripting.Script.Variables.InitGlobalVars();
                Keysharp.Scripting.Script.SetName(name);
                if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.Prompt))
                {
                    return 0;
                }

                Keysharp.Core.Env.HandleCommandLineParams(args);
                Keysharp.Scripting.Script.CreateTrayMenu();
                Keysharp.Scripting.Script.RunMainWindow(name, _ks_UserMainCode, false);
                Keysharp.Scripting.Script.WaitThreads();
                return 0;
            }
            catch (Keysharp.Core.Error kserr)
            {
                if (ErrorOccurred(kserr))
                {
                    var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                    MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                    Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                }

                Keysharp.Core.Flow.ExitApp(1);
                return 1;
            }
            catch (System.Exception mainex)
            {
                var ex = mainex.InnerException ?? mainex;
                if (ex is Keysharp.Core.Error kserr)
                {
                    if (ErrorOccurred(kserr))
                    {
                        var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                        MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                        Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                    }
                }
                else
                {
                    var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                    MsgBox("Uncaught exception:\r\n" + "Message: " + ex.Message + "\r\nStack: " + ex.StackTrace, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                    Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                };
                Keysharp.Core.Flow.ExitApp(1);
                return 1;
            }
        }

        public static object _ks_UserMainCode()
        {
            var test = Reflections.stringToTypeProperties["Map"];

            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}