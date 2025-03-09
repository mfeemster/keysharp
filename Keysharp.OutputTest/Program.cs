using static Keysharp.Core.Accessors;
using static Keysharp.Core.COM.Com;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Debug;
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
        public static object wisegui = Keysharp.Core.Functions.Func("wisegui");
        public static object WISEGUI_names = null;
        public static object PLAYSOUND_names = null;
        public static object sleep = Keysharp.Core.Functions.Func("sleep");
        public static object Wisegui(object name, params object[] _ks_options)
        {
            object mygui = null;
            object parseoptions = null;
            object updategui = null;
            object creategui = null;
            object applysettings = null;
            object gettheme = null;
            object getsyscolor = null;
            object geticon = null;
            object playsound = null;
            object setclasslong = null;
            object guisetpos = null;
            object animatewindow = null;
            object onclose = null;
            object guiclose = null;
            object options = new Array(_ks_options);
            object Parseoptions(VarRef usropts)
            {
                object value = null;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(options, Misc.MakeVarRef(() => value, (Val) => value = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        value
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            switch (Keysharp.Core.Types.Type(value).ToString())
                            {
                                case string _ks_string_1 when _ks_string_1.Equals("String".ToString()):
                                    {
                                        value = Keysharp.Core.Strings.StrSplit(value, new Keysharp.Core.Array("=", ":"), A_Space, 2L);
                                        if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.GreaterThan, Keysharp.Scripting.Script.GetPropertyValue(value, "Length"), 1L)))
                                        {
                                            Keysharp.Scripting.Script.SetObject(Keysharp.Core.Strings.Trim(Keysharp.Scripting.Script.Index(value, 2L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "'", "\"")), ((VarRef)usropts).__Value, Keysharp.Core.Strings.SubStr(Keysharp.Scripting.Script.Index(value, 1L), 1L, 5L));
                                        }
                                    }

                                    break;
                                case string _ks_string_2 when _ks_string_2.Equals("Func".ToString()):
                                case string _ks_string_3 when _ks_string_3.Equals("BoundFunc".ToString()):
                                    {
                                        Keysharp.Scripting.Script.SetObject(value, ((VarRef)usropts).__Value, "Trigger");
                                    }

                                    break;
                            }

                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        value = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                return "";
            }

            parseoptions = Keysharp.Core.Functions.Func((Delegate)Parseoptions);
            object Updategui(VarRef mygui)
            {
                object usropts = null;
                object value = null;
                object guip = null;
                object period = null;
                usropts = Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.GetPropertyValue(options, "Length")))
                {
                    Invoke(parseoptions, "Call", new VarRef(() => usropts, (value) => usropts = value));
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Theme")))
                {
                    guip = Invoke(gettheme, "Call", Keysharp.Scripting.Script.Index(usropts, "Theme"));
                    Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "BackColor", Keysharp.Scripting.Script.Index(guip, "WindowColor"));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineL"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineT"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineB"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineR"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "MainText"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "c", Keysharp.Scripting.Script.Index(guip, "TextColor")));
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "SubText"), "Opt", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "c", Keysharp.Scripting.Script.Index(guip, "TextColor")));
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "Icon"), "Value")))
                    {
                        Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "Icon"), "Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "HICON:", Keysharp.Scripting.Script.Index(guip, "HICON")));
                    }

                    Keysharp.Core.WindowX.WinRedraw(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"));
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Show", "NA");
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Has", "MainT")))
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueInequality, Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "MainText"), "Text"), Keysharp.Scripting.Script.Index(usropts, "MainT"))))
                    {
                        Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "MainText"), "Text", Keysharp.Scripting.Script.Index(usropts, "MainT"));
                    }

                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "MainT");
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Has", "SubTe")))
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueInequality, Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "SubText"), "Text"), Keysharp.Scripting.Script.Index(usropts, "SubTe"))))
                    {
                        Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "SubText"), "Text", Keysharp.Scripting.Script.Index(usropts, "SubTe"));
                    }

                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "SubTe");
                }

                Invoke(onclose, "Call", new VarRef(() => ((VarRef)mygui).__Value, (value) => ((VarRef)mygui).__Value = value));
                Invoke(applysettings, "Call", new VarRef(() => ((VarRef)mygui).__Value, (value) => ((VarRef)mygui).__Value = value));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Has", "Timer")))
                {
                    period = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, 0L, Keysharp.Core.Maths.Min(20000L, Keysharp.Core.Maths.Max(200L, Keysharp.Core.Strings.Format("{:d}", Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Timer")))));
                    Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "MyTimer"), period);
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "Timer");
                }

                return "";
            }

            updategui = Keysharp.Core.Functions.Func((Delegate)Updategui);
            object Creategui(VarRef mygui)
            {
                object value = null;
                object theme = null;
                object guip = null;
                object txtp = null;
                object usropts = null;
                object font = null;
                object period = null;
                object dhw = null;
                object xy = null;
                object x = null;
                object y = null;
                object w = null;
                object h = null;
                object lx = null;
                object mw = null;
                object sw = null;
                object dwoldlong = null;
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui), "__Value", Keysharp.Core.GuiHelper.Gui("+DpiScale -Caption +AlwaysOnTop +Owner +E0x08000000", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "WiseGui\\", name)));
                Keysharp.Scripting.Script.SetObject(((VarRef)mygui).__Value, WISEGUI_names, name);
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "Name", name);
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MyTimer", Keysharp.Scripting.Script.Invoke(guiclose, "Bind", ((VarRef)mygui).__Value));
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "UsrOpts", Keysharp.Core.Collections.Map());
                usropts = Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts");
                Keysharp.Scripting.Script.SetPropertyValue(usropts, "CaseSense", "Off");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.GetPropertyValue(options, "Length")))
                {
                    Invoke(parseoptions, "Call", new VarRef(() => usropts, (value) => usropts = value));
                }

                xy = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Margi")) ? Keysharp.Core.Strings.StrSplit(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Index(usropts, "Margi"), ",,,,"), ",", A_Space, 5L) : new Keysharp.Core.Array(2L, 1L, 0L, 0L);
                {
                    System.Collections.IEnumerator _ks_e1 = Keysharp.Core.Loops.Loop(4L).GetEnumerator();
                    Keysharp.Core.Loops.Push(Keysharp.Core.LoopType.Normal);
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Core.Strings.Format("{:d}", Keysharp.Scripting.Script.Index(xy, A_Index)), 4L), xy, A_Index);
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

                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginX", 0L);
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginY", 0L);
                guip = Invoke(gettheme, "Call", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Theme")) ? Keysharp.Scripting.Script.Index(usropts, "Theme") : "Info");
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "BackColor", Keysharp.Scripting.Script.Index(guip, "WindowColor"));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Index(guip, "HICON")))
                {
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x0 y0 w", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Index(xy, 4L), 2L), 32L)), " h"), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Index(xy, 4L), 2L), 32L)), " vLineL Background"), Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Picture", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "xp+", Keysharp.Scripting.Script.Index(xy, 4L)), " yp+"), Keysharp.Scripting.Script.Index(xy, 4L)), " w32 h32 BackgroundTrans vIcon"), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "HICON:", Keysharp.Scripting.Script.Index(guip, "HICON")));
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x+", Keysharp.Scripting.Script.Index(xy, 4L)), " w0 h0"));
                }
                else
                {
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x0 y0   w1 h1 vLineL Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Picture", "xp yp w0 h0 vIcon");
                }

                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x+0 ym w10 h1 Section vLineT Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "MainT")))
                {
                    value = Keysharp.Scripting.Script.Index(usropts, "MainT");
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "MainT");
                }
                else
                {
                    value = Keysharp.Scripting.Script.Index(new Keysharp.Core.Array("Warning", "Information", "Success", "Error", A_ScriptName), Keysharp.Core.Strings.InStr("WISEU", Keysharp.Scripting.Script.Index(guip, "T")));
                }

                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginX", Keysharp.Scripting.Script.Index(xy, 1L));
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginY", Keysharp.Scripting.Script.Index(xy, 2L));
                font = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "FontM")) ? Keysharp.Core.Strings.StrSplit(Keysharp.Scripting.Script.Index(usropts, "FontM"), ",", " ") : new Keysharp.Core.Array("s10 Bold", "Segoe UI");
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "SetFont", [.. Keysharp.Scripting.Script.FlattenParam(font)]);
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "xp+", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "MarginX")), " y+m vMainText BackgroundTrans c"), Keysharp.Scripting.Script.Index(guip, "TextColor"));
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, txtp, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "TextW")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, " w", Keysharp.Scripting.Script.Index(usropts, "TextW")) : "");
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, txtp, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "MainA")) ? Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(usropts, "MainA"), 0L)) ? " Center" : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(usropts, "MainA"), 1L)) ? " Right" : "" : "");
                if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Main")) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(usropts, "Main"), 0L))).ParseObject()))
                {
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "vMainText w0 h0 y+0 xp+", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "MarginX")));
                }
                else
                {
                    Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", txtp, value);
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "SubTe")))
                {
                    value = Keysharp.Scripting.Script.Index(usropts, "SubTe");
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "SubTe");
                }
                else
                {
                    value = "";
                }

                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginY", Keysharp.Scripting.Script.Index(xy, 3L));
                font = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "FontS")) ? Keysharp.Core.Strings.StrSplit(Keysharp.Scripting.Script.Index(usropts, "FontS"), ",", " ") : new Keysharp.Core.Array("s10 Norm", "Segoe UI");
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "SetFont", [.. Keysharp.Scripting.Script.FlattenParam(font)]);
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "xp y+m vSubText BackgroundTrans c", Keysharp.Scripting.Script.Index(guip, "TextColor"));
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, txtp, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "TextW")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, " w", Keysharp.Scripting.Script.Index(usropts, "TextW")) : "");
                txtp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, txtp, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "SubAl")) ? Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(usropts, "SubAl"), 0L)) ? " Center" : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(usropts, "SubAl"), 1L)) ? " Right" : "" : "");
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", txtp, value);
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginY", Keysharp.Scripting.Script.Index(xy, 2L));
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x+0 yp  w1  hp vLineR Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "xs  y+m w10 h1 vLineB Background", Keysharp.Scripting.Script.Index(guip, "BorderColor")));
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginX", Keysharp.Scripting.Script.Index(xy, 1L));
                Keysharp.Scripting.Script.SetPropertyValue(((VarRef)mygui).__Value, "MarginY", 0L);
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Show", "AutoSize Hide");
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "GetPos", new VarRef(() => x, (value) => x = value), new VarRef(() => y, (value) => y = value), new VarRef(() => w, (value) => w = value), new VarRef(() => h, (value) => h = value));
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineL"), "Move", null, null, null, h);
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineR"), "Move", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, w, 1L), 0L, null, h);
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineT"), "GetPos", new VarRef(() => lx, (value) => lx = value));
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineT"), "Move", null, null, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, w, lx));
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "LineB"), "Move", null, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, h, 1L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, w, lx));
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "MainText"), "GetPos", null, null, new VarRef(() => mw, (value) => mw = value));
                Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "SubText"), "GetPos", null, null, new VarRef(() => sw, (value) => sw = value));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.GreaterThan, mw, sw)))
                {
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "SubText"), "Move", null, null, mw);
                }
                else
                {
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "MainText"), "Move", null, null, sw);
                }

                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Add", "Text", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "x0 y0 w", w), " h"), h), " vClick BackgroundTrans"));
                if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Shado")) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Index(usropts, "Shado"))).ParseObject()))
                {
                    dwoldlong = Invoke(setclasslong, "Call", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 26L));
                    Invoke(setclasslong, "Call", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 26L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.BitwiseOr, dwoldlong, 131072L));
                }

                Invoke(onclose, "Call", new VarRef(() => ((VarRef)mygui).__Value, (value) => ((VarRef)mygui).__Value = value));
                Invoke(applysettings, "Call", new VarRef(() => ((VarRef)mygui).__Value, (value) => ((VarRef)mygui).__Value = value), true);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Show")))
                {
                    Invoke(animatewindow, "Call", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), Keysharp.Scripting.Script.Index(usropts, "Show"));
                }

                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "Show", "NA");
                if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Shado")) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Index(usropts, "Shado"))).ParseObject()))
                {
                    Invoke(setclasslong, "Call", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 26L), dwoldlong);
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Has", "Timer")))
                {
                    period = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, 0L, Keysharp.Core.Maths.Min(20000L, Keysharp.Core.Maths.Max(200L, Keysharp.Core.Strings.Format("{:d}", Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Timer")))));
                    Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "MyTimer"), period);
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "Timer");
                }

                return "";
            }

            creategui = Keysharp.Core.Functions.Func((Delegate)Creategui);
            object Applysettings(VarRef mygui, [Optional, DefaultParameterValue(0L)] object create)
            {
                object usropts = null;
                object trans = null;
                object dhw = null;
                usropts = Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "WMP")))
                {
                    Invoke(playsound, "Call", name, Keysharp.Scripting.Script.Index(usropts, "WMP"));
                    Keysharp.Core.Flow.Sleep(100L);
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "WMP");
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Move")))
                {
                    Invoke(guisetpos, "Call", [Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), .. Keysharp.Scripting.Script.FlattenParam(Keysharp.Core.Strings.StrSplit(Keysharp.Scripting.Script.Index(usropts, "Move"), ",", A_Space, 3L))]);
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Delete", "Move");
                }
                else
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, create, true)))
                    {
                        Invoke(guisetpos, "Call", Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"), "-10", "-10");
                    }
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Has", "Trans")))
                {
                    trans = Keysharp.Core.Maths.Min(255L, Keysharp.Core.Maths.Max(64L, Keysharp.Core.Strings.Format("{:d}", Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts"), "Trans"))));
                    dhw = A_DetectHiddenWindows;
                    Keysharp.Core.WindowX.DetectHiddenWindows(true);
                    Keysharp.Core.WindowX.WinSetTransparent(trans, Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "Hwnd"));
                    Keysharp.Core.WindowX.DetectHiddenWindows(dhw);
                }

                return "";
            }

            applysettings = Keysharp.Core.Functions.Func((Delegate)Applysettings);
            object Gettheme(object theme)
            {
                object arr = null;
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Strings.InStr(theme, ","))))
                {
                    theme = Keysharp.Core.Strings.Format("{:U}", Keysharp.Core.Strings.SubStr(theme, 1L, 1L));
                    return Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "W")) ? Keysharp.Core.Collections.Map("T", "W", "TextColor", "856442", "WindowColor", "FFFFF0", "BorderColor", "EBB800", "HICON", Invoke(geticon, "Call", theme)) : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "I")) ? Keysharp.Core.Collections.Map("T", "I", "TextColor", "194499", "WindowColor", "F0F8FF", "BorderColor", "3399FF", "HICON", Invoke(geticon, "Call", theme)) : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "S")) ? Keysharp.Core.Collections.Map("T", "S", "TextColor", "155724", "WindowColor", "F0FFE9", "BorderColor", "429300", "HICON", Invoke(geticon, "Call", theme)) : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "E")) ? Keysharp.Core.Collections.Map("T", "E", "TextColor", "721C24", "WindowColor", "FFF4F4", "BorderColor", "E40000", "HICON", Invoke(geticon, "Call", theme)) : Keysharp.Core.Collections.Map("T", "I", "TextColor", "194499", "WindowColor", "F0F8FF", "BorderColor", "3399FF", "HICON", Invoke(geticon, "Call", "I"));
                }

                arr = Keysharp.Core.Strings.StrSplit(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, theme, ",,,,"), ",", A_Space, 5L);
                return Keysharp.Core.Collections.Map("T", "U", "TextColor", Keysharp.Scripting.Script.IfTest(Keysharp.Core.Strings.StrLen(Keysharp.Scripting.Script.Index(arr, 1L))) ? Keysharp.Core.Strings.Format("{:06X}", Keysharp.Scripting.Script.Index(arr, 1L)) : Invoke(getsyscolor, "Call", 8L), "WindowColor", Keysharp.Scripting.Script.IfTest(Keysharp.Core.Strings.StrLen(Keysharp.Scripting.Script.Index(arr, 2L))) ? Keysharp.Core.Strings.Format("{:06X}", Keysharp.Scripting.Script.Index(arr, 2L)) : Invoke(getsyscolor, "Call", 15L), "BorderColor", Keysharp.Scripting.Script.IfTest(Keysharp.Core.Strings.StrLen(Keysharp.Scripting.Script.Index(arr, 3L))) ? Keysharp.Core.Strings.Format("{:06X}", Keysharp.Scripting.Script.Index(arr, 3L)) : Invoke(getsyscolor, "Call", 6L), "HICON", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Strings.StrLen(Keysharp.Scripting.Script.Index(arr, 4L)), 0L)) ? Invoke(geticon, "Call", "") : Keysharp.Scripting.Script.Index(arr, 4L));
            }

            gettheme = Keysharp.Core.Functions.Func((Delegate)Gettheme);
            object Getsyscolor(object nindex)
            {
                return Invoke(Keysharp.Core.Functions.Func(Keysharp.Core.Strings.Format), "Call", ["{5:}{6:}{3:}{4:}{1:}{2:}", .. Keysharp.Scripting.Script.FlattenParam(Keysharp.Core.Strings.StrSplit(Keysharp.Core.Strings.Format("{:06X}", Keysharp.Core.Dll.DllCall("User32.dll\\GetSysColor", "Int", nindex))))]);
            }

            getsyscolor = Keysharp.Core.Functions.Func((Delegate)Getsyscolor);
            object Geticon(object theme, [Optional, DefaultParameterValue(0L)] object w, [Optional, DefaultParameterValue(0L)] object h)
            {
                object b64 = null;
                object b64len = null;
                object nbytes = null;
                object bin = null;
                b64 = Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "W")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAXVBMVEXruAD//+KFZEL13HH+/NiPclLsuwr9+c789cH887v46Jj45pP02GbtvhPEt5", "ruwh/67ar67amii22Zf2Dyz0rxz0nb1LfUyq7MwaSzooTuwiD499rx7tHn4sWrl3l1GphJAAABHUlEQVRYw+2WybLCIBBF3wUyCZmMUfMc/v8zLXeaC0IXW86+T9FAD3+FQiH"), "Gv7NTa0w7WTfKo3vd4YNO96LwWhvsMLpOjz818NCsqfEaAXTa8Q8IckhJ4zOeDdLzy7M4IcIauYDv+38qpa67t6glCVQkiCTRGxY8dgLT/zwAC+6Se+xYUGFP96P+wIIFRLg2"), "nUewgXBBgfUIBhA2KJg8giuIOShoPYIziDYoMB7BA4QRCW4CAadwVBUkKUwsWCC5RMuCIxib/pGeagPj0r/yZbiAGZOLyU8na4iMTm8oZ6WWO7ihJB9hUZ5i0oKmWnkETS1o6"), "5viYlolg+U20DPq3NGWPVwzx3vugpG74uQuWWJGZ+f3mje/17xCoRDhBWODDIHAQPWnAAAAAElFTkSuQmCC") : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "I")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "iVBORw0KGgoAAAANSUhEUgAAAEAAAABAC", "AMAAACdt4HsAAAAVFBMVEUzmf/w+P8ZRJmSyf/o9P/c5vOks9Pg8P/V6v/P6P/B4f+y2f+u1v+IxP9wuP9Npv9Eof88nv9tg7jH0ue7yOA6nf80VqFKZqmRosrR3O18kL9cdb"), "Au8FSxAAABHElEQVRYw+2WyZaEIAxFO4JhKMSpyp7+/z9bdzYRk2xqxd2/e+AkIXw0Gg2OcUjRW+tjGkZ9ejIBTgQzqeKzsVBgzSzPZwcXuCzNG6hgRPHXA6o8XgLBvzwxKM/"), "f/3bdortFhjNf3c4TzmSmfo4TuFlTANzzm1WUYir7BxH7sqMm+QH03RAkgnAzfyCiPpuDTDBUBQkKfkgVD1JVEKFguxTEqsBDQbfzTQS+KrBXgp4IrFjQHwJQCPyF4BMUV4gy"), "QbwpIxUsVJDEjbQSAdNIo0wwMsPECQIzzqzAMA8KJ7C3S86QKmy67TI70onrE5E8qnUymYWimbJqsSxHfENyAfFq6xFxFa82/XLVr/d3fTD0Xxz9J0v/zWs0Ggx/07kMr8YqP"), "fYAAAAASUVORK5CYII=") : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "S")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAY1BMVEVCkwDw/+kVVySZyXXo+t/c7dbh9tTS7MHF5K+325yz2", "ZiQxGl6tktRnBRKmAvG28Jrj24yZzujvaHm9uBaoiDR5MzX78fW78avx62WspWJp4lbg19IdU5IlwlboiG70bd7m3xysmd2AAABTUlEQVRYw+2W226EIBCGOwMqKyqeD6tr+/"), "5PWeO2oSjEsdzyJV5o8v+ZgzDzEQgErlhlKnrOe5HK9r46Zwn8IWH5LXnBOBzgrKDrsxgsxBlVz8ABI8lfD3DyeBEMtN7m4BE/MYsMLsgu+heDFTWVXz+9KP6TwIiIMyWJnIO"), "NqEKsInjD8/sBlIhYk+qYOBMo9Wvi1q9go6kQMQKN+2xKsFEbCWxIp0FqreCmX4wvqdNAgIUnIn4aX4TToHcEUILB4DTgjgAUGHCSQaQDeIIJJ6TQLVh1vy2IwGQgFHFCfDtU"), "5wBAENrYzIg4NzDqClDaKMF0mKDULaD8SOvhBGK9PSMcaWmHSeHOctIn1OM87QY1HGHkC2VBo4f6QqGGoNBWQnbjUu2Uak4DrvC91n0Hi+9o8x6uvuPdf8HwX3H8lyw6rUzFw"), "Pmwr3mBQOCCb5bODse1hzUgAAAAAElFTkSuQmCC") : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, theme, "E")) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAS1BMVEXkAAD/7u1yHCTyd3eziovFo", "6T+5OPlCgqfbW/92dj7y8v6xcT4s7LOr6/2m5rwa2voISHmFRT2oJ/tTU3tTUy8l5ipfH6UXWD2oaDGo2xeAAABGUlEQVRYw+2W3W7DIAyFdyCB8VMIdN32/k+6SZ3SZBSbyr"), "d8d1F0TmxiG79NJhOOLafojfEx5e11ddEBB4IuL8mtNviH0XZcf3V4gvsY1Wt00GPhV3SpI2l8g6AK4h/M4goG5iStA4OzfAKCJMqjfpYLjqzrXlFlKIBFqctRr9Q6EkI4CP4"), "c2sdA9B923pV6fPSmfln2d/3ezHjqsJz1yF2DhNah1SN1DSJah1aP2DXwaB2+Gj1818DgzKe6c8MJM2yARd3TGDXwtAGfQqRT4A8xjR1ioguJ/42ZLmW+kDaymfhSDlQ7t/rW"), "QZMDhW9nU8iRxg8UzQxVbqQ5Kx3r4ouFpYKgii9X4fUuXTCkK45wyRKueZPJhOEHEsMMXLgJ8a8AAAAASUVORK5CYII=") : Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYA", "AAAfFcSJAAAADUlEQVQImWOor69nBgAEfwGBGWiMoAAAAABJRU5ErkJggg==");
                nbytes = Keysharp.Core.Maths.Floor(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Divide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, b64len = Keysharp.Core.Strings.StrLen(b64 = Keysharp.Core.Strings.RTrim(b64, "=")), 3L), 4L));
                bin = Keysharp.Core.Collections.Buffer(nbytes);
                Keysharp.Core.Dll.DllCall("Crypt32.dll\\CryptStringToBinary", "str", b64, "int", b64len, "int", 1L, "ptr", bin, "uintp", nbytes, "Int", 0L, "Int", 0L);
                return Keysharp.Core.Dll.DllCall("User32.dll\\CreateIconFromResourceEx", "ptr", bin, "int", nbytes, "int", 1L, "int", 196608L, "Int", w, "Int", h, "Int", 0L, "ptr");
            }

            geticon = Keysharp.Core.Functions.Func((Delegate)Geticon);
            object Playsound(object name, [Optional, DefaultParameterValue("")] object filename, params object[] args)
            {
                object checksound = null;
                object Checksound(object _name)
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Scripting.Script.Invoke(PLAYSOUND_names, "Has", _name))))
                    {
                        return Keysharp.Core.Flow.SetTimer(null, 0L);
                    }

                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueInequality, Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(PLAYSOUND_names, _name), "playState"), 3L)))
                    {
                        {
                            Keysharp.Core.Flow.SetTimer(null, 0L);
                            Keysharp.Scripting.Script.Invoke(PLAYSOUND_names, "Delete", _name);
                        }
                    }

                    return "";
                }

                checksound = Keysharp.Core.Functions.Func((Delegate)Checksound);
                Keysharp.Scripting.Script.InitStaticVariable(ref PLAYSOUND_names, "Program_PLAYSOUND_names", () => Keysharp.Core.Collections.Map());
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, filename, "")))
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(PLAYSOUND_names, "Has", name)))
                    {
                        {
                            Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(PLAYSOUND_names, name), "Controls"), "Stop");
                            Keysharp.Scripting.Script.Invoke(PLAYSOUND_names, "Delete", name);
                        }
                    }

                    return "";
                }

                {
                    Keysharp.Scripting.Script.SetObject(Keysharp.Core.COM.Com.ComObject("WMPlayer.OCX"), PLAYSOUND_names, name);
                    Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.Index(PLAYSOUND_names, name), "Url", filename);
                }

                {
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(PLAYSOUND_names, "Has", name)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(PLAYSOUND_names, name), "controls"), "isAvailable", "stop"))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.Index(PLAYSOUND_names, name), "controls"), "currentPosition")))).ParseObject()));)
                        {
                            Keysharp.Core.Loops.Inc();
                            Keysharp.Core.Flow.Sleep(10L);
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

                try
                {
                    Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.Invoke(checksound, "Bind", name), 2000L);
                }
                catch (Keysharp.Core.Error _ks_ex_1_0)
                {
                }

                Keysharp.Scripting.Script.MultiStatement(checksound);
                return "";
            }

            playsound = Keysharp.Core.Functions.Func((Delegate)Playsound);
            object Setclasslong(object hwnd, object nindex, [Optional, DefaultParameterValue("")] object dwnewlong)
            {
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Types.IsInteger(dwnewlong))))
                {
                    return Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, A_PtrSize, 8L)) ? Keysharp.Core.Dll.DllCall("User32.dll\\GetClassLongPtr", "ptr", hwnd, "int", nindex, "uint") : Keysharp.Core.Dll.DllCall("User32.dll\\GetClassLong", "ptr", hwnd, "int", nindex, "uint");
                }
                else
                {
                    return Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, A_PtrSize, 8L)) ? Keysharp.Core.Dll.DllCall("User32.dll\\SetClassLongPtr", "ptr", hwnd, "int", nindex, "ptr", dwnewlong, "uint") : Keysharp.Core.Dll.DllCall("User32.dll\\SetClassLong", "ptr", hwnd, "int", nindex, "ptr", dwnewlong, "uint");
                }

                return "";
            }

            setclasslong = Keysharp.Core.Functions.Func((Delegate)Setclasslong);
            object Guisetpos(object hwnd, [Optional, DefaultParameterValue("")] object x, [Optional, DefaultParameterValue("")] object y, [Optional, DefaultParameterValue("")] object m)
            {
                object dpi_awareness_context_unaware = null;
                object dpi_awareness_context = null;
                object err = null;
                object mleft = null;
                object mtop = null;
                object mright = null;
                object mbottom = null;
                object rect = null;
                object w = null;
                object h = null;
                dpi_awareness_context = 0L;
                try
                {
                    dpi_awareness_context = Keysharp.Core.Dll.DllCall("User32.dll\\GetThreadDpiAwarenessContext", "ptr");
                    dpi_awareness_context_unaware = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, dpi_awareness_context, 1L);
                    Keysharp.Core.Dll.DllCall("User32.dll\\SetThreadDpiAwarenessContext", "ptr", dpi_awareness_context_unaware, "ptr");
                }
                catch (Keysharp.Core.Error _ks_ex_1_0)
                {
                }

                try
                {
                    m = Keysharp.Core.Monitor.MonitorGetWorkArea(m, new VarRef(() => mleft, (value) => mleft = value), new VarRef(() => mtop, (value) => mtop = value), new VarRef(() => mright, (value) => mright = value), new VarRef(() => mbottom, (value) => mbottom = value));
                }
                catch (Keysharp.Core.Error _ks_ex_1_1)
                {
                    err = _ks_ex_1_1;
                    m = Keysharp.Core.Monitor.MonitorGetWorkArea(null, new VarRef(() => mleft, (value) => mleft = value), new VarRef(() => mtop, (value) => mtop = value), new VarRef(() => mright, (value) => mright = value), new VarRef(() => mbottom, (value) => mbottom = value));
                }

                rect = Keysharp.Core.Collections.Buffer(16L, 0L);
                Keysharp.Core.Dll.DllCall("User32.dll\\GetWindowRect", "ptr", hwnd, "ptr", rect);
                w = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Core.External.NumGet(rect, 8L, "int"), Keysharp.Core.External.NumGet(rect, 0L, "int"));
                h = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Core.External.NumGet(rect, 12L, "int"), Keysharp.Core.External.NumGet(rect, 4L, "int"));
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, mleft, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Strings.StrLen(x), 0L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.FloorDivide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, mright, mleft), 2L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.FloorDivide, w, 2L)) : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.LessThan, x, 0L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, mright, mleft), w), 1L), x) : x);
                y = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, mtop, Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Strings.StrLen(y), 0L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.FloorDivide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, mbottom, mtop), 2L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.FloorDivide, h, 2L)) : Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.LessThan, y, 0L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, mbottom, mtop), h), 1L), y) : y);
                Keysharp.Core.Dll.DllCall("User32.dll\\MoveWindow", "ptr", hwnd, "int", x, "int", y, "int", w, "int", h, "int", 1L);
                if (Keysharp.Scripting.Script.IfTest(dpi_awareness_context))
                {
                    Keysharp.Core.Dll.DllCall("User32.dll\\SetThreadDpiAwarenessContext", "ptr", dpi_awareness_context);
                }

                return "";
            }

            guisetpos = Keysharp.Core.Functions.Func((Delegate)Guisetpos);
            object Animatewindow(object hwnd, object p)
            {
                object flags = null;
                object _ks_temp1 = null;
                object _ks_temp2 = null;
                flags = Keysharp.Core.Collections.Map();
                Keysharp.Scripting.Script.SetPropertyValue(flags, "CaseSense", "Off");
                Keysharp.Scripting.Script.Invoke(flags, "Set", "Fade", "0x00080000", "Zoom", "0x00000010", "SlideEast", "0x00040001", "SlideNorthEast", "0x00040009", "SlideNorth", "0x00040008", "SlideNorthWest", "0x0004000A", "SlideWest", "0x00040002", "SlideSouthWest", "0x00040006", "SlideSouth", "0x00040004", "SlideSouthEast", "0x00040005", "RollEast", "0x00000001", "RollNorthEast", "0x00000009", "RollNorth", "0x00000008", "RollNorthWest", "0x0000000A", "RollWest", "0x00000002", "RollSouthWest", "0x00000006", "RollSouth", "0x00000004", "RollSouthEast", "0x00000005");
                p = Keysharp.Core.Strings.StrSplit(p, new Keysharp.Core.Array("@", "ms"), "- ");
                Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(flags, "Has", Keysharp.Scripting.Script.Index(p, 1L))) ? Keysharp.Scripting.Script.Index(flags, Keysharp.Scripting.Script.Index(p, 1L)) : Keysharp.Scripting.Script.Index(flags, "Fade"), p, 1L);
                Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = p, _ks_temp2 = 1L, Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.BitwiseOr, Keysharp.Scripting.Script.Index(_ks_temp1, _ks_temp2), Keysharp.Scripting.Script.IfTest(Keysharp.Core.Dll.DllCall("User32.dll\\IsWindowVisible", "ptr", hwnd)) ? 65536L : 0L), _ks_temp1, _ks_temp2));
                Keysharp.Scripting.Script.SetObject(Keysharp.Core.Maths.Min(2500L, Keysharp.Core.Maths.Max(25L, Keysharp.Core.Strings.Format("{:d}", Keysharp.Scripting.Script.Index(p, 2L)))), p, 2L);
                Keysharp.Core.Dll.DllCall("User32.dll\\AnimateWindow", "ptr", hwnd, "int", Keysharp.Scripting.Script.Index(p, 2L), "int", Keysharp.Scripting.Script.Index(p, 1L));
                return "";
            }

            animatewindow = Keysharp.Core.Functions.Func((Delegate)Animatewindow);
            object Onclose(VarRef mygui)
            {
                object usropts = null;
                object _ks_anonlambda_1 = null;
                object _ks_anonlambda_2 = null;
                object _ks_AnonLambda_1(params object[] _ks_args)
                {
                    object args = new Array(_ks_args);
                    return Invoke(guiclose, "Call", ((VarRef)mygui).__Value, "*");
                }

                _ks_anonlambda_1 = Keysharp.Core.Functions.Func((Delegate)_ks_AnonLambda_1);
                object _ks_AnonLambda_2(params object[] _ks_args)
                {
                    object args = new Array(_ks_args);
                    return Invoke(guiclose, "Call", ((VarRef)mygui).__Value, "-1");
                }

                _ks_anonlambda_2 = Keysharp.Core.Functions.Func((Delegate)_ks_AnonLambda_2);
                usropts = Keysharp.Scripting.Script.GetPropertyValue(((VarRef)mygui).__Value, "UsrOpts");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(usropts, "Has", "Close")))
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Index(usropts, "Close")))
                    {
                        Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "Click"), "OnEvent", "Click", Keysharp.Scripting.Script.Invoke(guiclose, "Bind", ((VarRef)mygui).__Value, true));
                    }
                }
                else
                {
                    Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(((VarRef)mygui).__Value, "Click"), "OnEvent", "Click", Keysharp.Scripting.Script.Invoke(guiclose, "Bind", ((VarRef)mygui).__Value, true));
                    Keysharp.Scripting.Script.SetObject(1L, usropts, "Close");
                }

                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "OnEvent", "Close", _ks_anonlambda_1);
                Keysharp.Scripting.Script.Invoke(((VarRef)mygui).__Value, "OnEvent", "ContextMenu", _ks_anonlambda_2);
                return "";
            }

            onclose = Keysharp.Core.Functions.Func((Delegate)Onclose);
            object Guiclose(object thisgui, [Optional, DefaultParameterValue("0")] object clicked, params object[] _ks_args)
            {
                object err = null;
                object args = new Array(_ks_args);
                Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "MyTimer"), 0L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Close"))))
                {
                    Keysharp.Core.Flow.Sleep(0L);
                    return 1L;
                }

                try
                {
                    Keysharp.Scripting.Script.SetPropertyValue(thisgui, "Title", "");
                }
                catch (Keysharp.Core.Error _ks_ex_1_1)
                {
                    err = _ks_ex_1_1;
                    return "";
                }

                Keysharp.Scripting.Script.Invoke(WISEGUI_names, "Delete", Keysharp.Scripting.Script.GetPropertyValue(thisgui, "Name"));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Has", "Hide")))
                {
                    Invoke(animatewindow, "Call", Keysharp.Scripting.Script.GetPropertyValue(thisgui, "Hwnd"), Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Hide"));
                }

                Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.Invoke(playsound, "Bind", Keysharp.Scripting.Script.GetPropertyValue(thisgui, "Name"), ""), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Has", "Close")))
                {
                    if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Close"), "Trigger")))
                    {
                        if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Has", "Trigger")))
                        {
                            Keysharp.Core.Flow.SetTimer(Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(thisgui, "UsrOpts"), "Trigger"), "Bind", clicked), Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L));
                        }
                    }
                }

                Keysharp.Scripting.Script.Invoke(thisgui, "Destroy");
                return "";
            }

            guiclose = Keysharp.Core.Functions.Func((Delegate)Guiclose);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Strings.StrLen(name), 0L)))
            {
                return "";
            }

            Keysharp.Scripting.Script.InitStaticVariable(ref WISEGUI_names, "Program_WISEGUI_names", () => Keysharp.Core.Collections.Map());
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Scripting.Script.GetPropertyValue(options, "Length"), 0L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(WISEGUI_names, "Has", name))).ParseObject()))
            {
                return Invoke(guiclose, "Call", Keysharp.Scripting.Script.Index(WISEGUI_names, name));
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Invoke(WISEGUI_names, "Has", name)))
            {
                mygui = Keysharp.Scripting.Script.Index(WISEGUI_names, name);
                Invoke(updategui, "Call", new VarRef(() => mygui, (value) => mygui = value));
            }
            else
            {
                Invoke(creategui, "Call", new VarRef(() => mygui, (value) => mygui = value));
            }

            Keysharp.Scripting.Script.MultiStatement(parseoptions);
            Keysharp.Scripting.Script.MultiStatement(updategui);
            Keysharp.Scripting.Script.MultiStatement(creategui);
            Keysharp.Scripting.Script.MultiStatement(applysettings);
            Keysharp.Scripting.Script.MultiStatement(gettheme);
            Keysharp.Scripting.Script.MultiStatement(getsyscolor);
            Keysharp.Scripting.Script.MultiStatement(geticon);
            Keysharp.Scripting.Script.MultiStatement(playsound);
            Keysharp.Scripting.Script.MultiStatement(setclasslong);
            Keysharp.Scripting.Script.MultiStatement(guisetpos);
            Keysharp.Scripting.Script.MultiStatement(animatewindow);
            Keysharp.Scripting.Script.MultiStatement(onclose);
            Keysharp.Scripting.Script.MultiStatement(guiclose);
            return "";
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\Source\Repos\Keysharp_clone\bin\debug\net9.0-windows\*";
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
            Invoke(wisegui, "Call", "Test", "Theme: Warning");
            Keysharp.Core.Flow.Sleep(2000L);
            Invoke(wisegui, "Call", "Test");
            Invoke(wisegui, "Call", "Test", "Theme: Info");
            Keysharp.Core.Flow.Sleep(2000L);
            Invoke(wisegui, "Call", "Test");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}