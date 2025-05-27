//System usings.
global using global::Microsoft.CodeAnalysis;
global using global::Microsoft.CodeAnalysis.CSharp;
global using global::Microsoft.CodeAnalysis.Emit;
global using global::Microsoft.VisualBasic.FileIO;//See if this is cross platform or not. //TODO
global using global::System;
global using global::System.Buffers;
global using global::System.Data;
global using global::System.CodeDom;
global using global::System.CodeDom.Compiler;
global using global::System.Collections;
global using global::System.Collections.Concurrent;
global using global::System.Collections.Frozen;
global using global::System.Collections.Generic;
global using global::System.Collections.Immutable;
global using global::System.Collections.ObjectModel;
global using global::System.Collections.Specialized;
global using global::System.ComponentModel;
global using global::System.Diagnostics;
global using global::System.Drawing;
global using global::System.Drawing.Drawing2D;
global using global::System.Drawing.Imaging;
global using global::System.Globalization;
global using global::System.IO;
global using global::System.IO.Compression;
global using global::System.Linq;
global using global::System.Net;
global using global::System.Net.Http;
global using global::System.Net.Http.Headers;
global using global::System.Net.Mail;
global using global::System.Net.Sockets;
global using global::System.Reflection;
global using global::System.Reflection.Emit;
global using global::System.Runtime.CompilerServices;
global using global::System.Runtime.InteropServices;
global using global::System.Runtime.Loader;
global using global::System.Security;
global using global::System.Security.Cryptography;
global using global::System.Security.Principal;
global using global::System.Text;
global using global::System.Text.Json;
global using global::System.Text.RegularExpressions;
global using global::System.Threading;
global using global::System.Threading.Channels;
global using global::System.Threading.Tasks;
global using global::System.Windows.Forms;


//Our usings.
global using global::Keysharp.Core;
global using global::Keysharp.Core.Common.Containers;
global using global::Keysharp.Core.Common.Cryptography;
global using global::Keysharp.Core.Common.ExtensionMethods;
global using global::Keysharp.Core.Common.File;
global using global::Keysharp.Core.Common.Images;
global using global::Keysharp.Core.Common.Input;
global using global::Keysharp.Core.Common.Invoke;
global using global::Keysharp.Core.Common.Joystick;
global using global::Keysharp.Core.Common.Keyboard;
global using global::Keysharp.Core.Common.Mapper;
global using global::Keysharp.Core.Common.ObjectBase;
global using global::Keysharp.Core.Common.Patterns;
global using global::Keysharp.Core.Common.Platform;
global using global::Keysharp.Core.Common.Strings;
global using global::Keysharp.Core.Common.Threading;
global using global::Keysharp.Core.Common.Window;
global using global::Keysharp.Scripting;
global using global::Semver.Comparers;
global using global::Semver.Utility;
global using global::PCRE;
global using global::BitFaster.Caching.Lfu;
global using global::BitFaster.Caching.Scheduler;

#if WINDOWS
	global using global::Accessibility;
	global using global::Microsoft.Win32;
	global using global::Microsoft.Win32.SafeHandles;
	global using global::System.Management;
	global using global::System.Media;
	global using global::System.Runtime.InteropServices.ComTypes;
	global using global::Keysharp.Core.COM;
	global using global::Keysharp.Core.Windows;
#endif

#if LINUX
	global using global::Keysharp.Core.Linux;
	global using global::Keysharp.Core.Linux.Proxies;
	global using global::Keysharp.Core.Linux.X11;
#endif

//Static
global using static global::Keysharp.Core.Accessors;
global using static global::Keysharp.Core.KeysharpEnhancements;
global using static global::Keysharp.Scripting.Keywords;
global using static global::Keysharp.Scripting.Script;
