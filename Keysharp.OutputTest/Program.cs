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

[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.8")]
namespace Keysharp.CompiledMain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
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

    public class Program
    {
        public static object highlight = Keysharp.Core.Functions.Func("highlight");
        public static object HIGHLIGHT_guis = null;
        public static object HIGHLIGHT_timers = null;
        public static object sleep = Keysharp.Core.Functions.Func("sleep");
        public static object Highlight([Optional, DefaultParameterValue(null)] object x, [Optional, DefaultParameterValue(null)] object y, [Optional, DefaultParameterValue(null)] object w, [Optional, DefaultParameterValue(null)] object h, [Optional, DefaultParameterValue(null)] object showtime, [Optional, DefaultParameterValue("Red")] object color, [Optional, DefaultParameterValue(2L)] object d)
        {
            object k = null;
            object v = null;
            object loc = null;
            object guiobj = null;
            object iw = null;
            object ih = null;
            object tryremovetimer = null;
            object trydeletegui = null;
            object Tryremovetimer(object key)
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(HIGHLIGHT_timers, "Has", key)))
                {
                    Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.Index(HIGHLIGHT_timers, key), 0L);
                    Keysharp.Scripting.Script.Invoke(HIGHLIGHT_timers, "Delete", key);
                }

                return "";
            }

            tryremovetimer = Keysharp.Core.Functions.Func((Delegate)Tryremovetimer);
            object Trydeletegui(object key)
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(HIGHLIGHT_guis, "Has", key)))
                {
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(HIGHLIGHT_guis, key), "Destroy");
                    Keysharp.Scripting.Script.Invoke(HIGHLIGHT_guis, "Delete", key);
                }

                return "";
            }

            trydeletegui = Keysharp.Core.Functions.Func((Delegate)Trydeletegui);
            Keysharp.Scripting.Script.InitStaticVariable(ref HIGHLIGHT_guis, "Program_HIGHLIGHT_guis", () => Keysharp.Core.Collections.Map());
            Keysharp.Scripting.Script.InitStaticVariable(ref HIGHLIGHT_timers, "Program_HIGHLIGHT_timers", () => Keysharp.Core.Collections.Map());
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(x)))
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsObject(x)))
                {
                    {
                        d = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "d")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "d") : d;
                        color = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "color")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "color") : color;
                        showtime = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "showTime")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "showTime") : showtime;
                        h = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "h")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "h") : h;
                        w = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "w")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "w") : h;
                        y = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "y")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "y") : y;
                        x = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(x, "HasOwnProp", "x")) ? Keysharp.Scripting.Script.GetPropertyValue(x, "x") : null;
                    }
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(x)) && Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(y))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(w))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(h))).ParseObject())))
                {
                    throw Keysharp.Core.Errors.ValueError("x, y, w and h arguments must all be provided for a highlight", Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L));
                }

                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(HIGHLIGHT_guis, Misc.MakeVarRef(() => k, (Val) => k = Val), Misc.MakeVarRef(() => v, (Val) => v = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[2]
                    {
                        k,
                        v
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.GetPropertyValue(k, "x"), x)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.GetPropertyValue(k, "y"), y))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.GetPropertyValue(k, "w"), w))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.GetPropertyValue(k, "h"), h))).ParseObject()))
                            {
                                if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Types.IsSet(showtime))) || Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(showtime)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, showtime, "clear"))).ParseObject())).ParseObject()))
                                {
                                    Invoke(tryremovetimer, "Call", k);
                                    Invoke(trydeletegui, "Call", k);
                                }
                                else
                                {
                                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, showtime, 0L)))
                                    {
                                        Invoke(tryremovetimer, "Call", k);
                                    }
                                    else
                                    {
                                        if (Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsInteger(showtime)))
                                        {
                                            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.LessThan, showtime, 0L)))
                                            {
                                                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Scripting.Script.Invoke(HIGHLIGHT_timers, "Has", k))))
                                                {
                                                    Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Invoke(highlight, "Bind", x, y, w, h), HIGHLIGHT_timers, k);
                                                }

                                                Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.Index(HIGHLIGHT_timers, k), showtime);
                                            }
                                            else
                                            {
                                                Invoke(tryremovetimer, "Call", k);
                                                Keysharp.Core.Flow.Sleep(showtime);
                                                Invoke(trydeletegui, "Call", k);
                                            }
                                        }
                                        else
                                        {
                                            throw Keysharp.Core.Errors.ValueError(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Invalid showTime value \"", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Types.IsSet(showtime))) ? "unset" : Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsObject(showtime)) ? "{Object}" : showtime), "\""), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L));
                                        }
                                    }
                                }

                                return "";
                            }

                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        (k, v) = (_ks_e1_backup[0], _ks_e1_backup[1]);
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }
            }
            else
            {
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(HIGHLIGHT_timers, Misc.MakeVarRef(() => k, (Val) => k = Val), Misc.MakeVarRef(() => v, (Val) => v = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[2]
                    {
                        k,
                        v
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            Keysharp.Core.Flow.SetTimer(v, 0L);
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        (k, v) = (_ks_e1_backup[0], _ks_e1_backup[1]);
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(HIGHLIGHT_guis, Misc.MakeVarRef(() => k, (Val) => k = Val), Misc.MakeVarRef(() => v, (Val) => v = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[2]
                    {
                        k,
                        v
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            Keysharp.Scripting.Script.Invoke(v, "Destroy");
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        (k, v) = (_ks_e1_backup[0], _ks_e1_backup[1]);
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                {
                    HIGHLIGHT_guis = Keysharp.Core.Collections.Map();
                    HIGHLIGHT_timers = Keysharp.Core.Collections.Map();
                }

                return "";
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, showtime = showtime ?? 2000L, "clear")))
            {
                return "";
            }
            else
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Types.IsInteger(showtime))))
                {
                    throw Keysharp.Core.Errors.ValueError(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Invalid showTime value \"", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Types.IsSet(showtime))) ? "unset" : Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsObject(showtime)) ? "{Object}" : showtime), "\""), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L));
                }
            }

            loc = Keysharp.Core.Objects.Object(new object[] { "x", x, "y", y, "w", w, "h", h });
            Keysharp.Scripting.Script.SetObject(Keysharp.Core.GuiHelper.Gui("+AlwaysOnTop -Caption +ToolWindow -DPIScale +E0x08000000"), HIGHLIGHT_guis, loc);
            guiobj = Keysharp.Scripting.Script.Index(HIGHLIGHT_guis, loc);
            Keysharp.Scripting.Script.SetPropertyValue(guiobj, "BackColor", color);
            {
                iw = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, w, d);
                ih = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, h, d);
                w = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, w, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, d, 2L));
                h = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, h, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, d, 2L));
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, x, d);
                y = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, y, d);
            }

            Keysharp.Core.WindowX.WinSetRegion(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "0-0 ", w), "-0 "), w), "-"), h), " 0-"), h), " 0-0 "), d), "-"), d), " "), iw), "-"), d), " "), iw), "-"), ih), " "), d), "-"), ih), " "), d), "-"), d), Keysharp.Scripting.Script.GetPropertyValue(guiobj, "Hwnd"));
            Keysharp.Scripting.Script.Invoke(guiobj, "Show", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "NA x", x), " y"), y), " w"), w), " h"), h));
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.GreaterThan, showtime, 0L)))
            {
                Keysharp.Core.Flow.Sleep(showtime);
                Invoke(trydeletegui, "Call", loc);
            }
            else
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.LessThan, showtime, 0L)))
                {
                    Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Invoke(highlight, "Bind", Keysharp.Scripting.Script.GetPropertyValue(loc, "x"), Keysharp.Scripting.Script.GetPropertyValue(loc, "y"), Keysharp.Scripting.Script.GetPropertyValue(loc, "w"), Keysharp.Scripting.Script.GetPropertyValue(loc, "h")), HIGHLIGHT_timers, loc), showtime);
                }
            }

            Keysharp.Scripting.Script.MultiStatement(tryremovetimer);
            Keysharp.Scripting.Script.MultiStatement(trydeletegui);
            return "";
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\Source\Repos\Keysharp_clone\bin\debug\net9.0-windows\Keysharp.ks";
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
            }

            return Environment.ExitCode;
        }

        public static object _ks_UserMainCode()
        {
            var sw = Stopwatch.StartNew();
            {
                System.Collections.IEnumerator _ks_e1 = Keysharp.Core.Loops.Loop(100L).GetEnumerator();
                Keysharp.Core.Loops.Push(Keysharp.Core.LoopType.Normal);
                try
                {
                    for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                    {
                        Invoke(highlight, "Call", 200L, 200L, 400L, 400L, -100L);
                        Invoke(highlight, "Call");
                    _ks_e1_next:
                        ;
                    }
                }
                finally
                {
                    Keysharp.Core.Loops.Pop();
                }

            _ks_e1_end:
                ;
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Forms elapsed time: {0} ms", sw.ElapsedMilliseconds);

            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}