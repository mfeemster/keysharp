# README #

Keysharp is a fork and improvement of the abandoned IronAHK project, which itself was a C# re-write of the C++ AutoHotKey project.

This project is in an extremely early state and should not be used on production systems by anyone.

Some general notes about Keysharp's implementation of the AutoHotKey specification as detailed here: https://www.autohotkey.com/docs/v2/

-The syntax is V2 style. While some remnants of V1 will work, it's unintentional and only V2 is supported.
-The operation of Keysharp is different than AHK. While AHK is an interpreted scripting language, Keysharp actually creates a compiled .NET executable and runs it.
-The process for reading and running a script is:
	-Pass the script to Keysharp.exe which parses it and generates a DOM tree.
	-Have the DOM generate C# code for a single program.
	-Compile the C# program into an in-memory executable.
	-Run the executable in memory as a new process.
	-Optionally output the generated C# code to a .cs file for debugging purposes.
	-Optionally output the generated executable to a file to a .exe file for running standalone in the future.
-Keysharp supports files with the .ahk extension, however installing it will not register it with that extension. Instead, it will register the other extension it supports, .ks.
-The following are not implemented yet:
	-Fat arrow functions.
	-COM.
	-Threads.
-In addition to Keysharp.exe, there is another executable that ships with the installer named Keyview.exe. This program can be used to see the C# code that is generated for the script code.
It gives real-time feedback so you can see immediately when you have a syntax error.
Despite our best efforts to remain compatible with the AHK spec, there are differences. Some of these differences are a reduction in functionality, and others are an increase. Others still are just slight syntax changes.

###Differences:###

####	Behaviors/Functionality:####
		-Keysharp follows the .NET memory model.
		-There is no variable caching with strings vs numbers. All variables are C# objects.
		-AHK says about the inc/dec ++/-- operators on empty variables: "Due to backward compatibility, the operators ++ and -- treat blank variables as zero, but only when they are alone on a line". Keysharp breaks this and will instead create a variable, initialize it to zero, then increment it. Example, a file with nothing but the line x++ in it, will end with a variable named x which has the value of 1.
		-Function objects will need to be created using the name of the function as a string to be used. They are not all created automatically on script load like the documentation says.
			-This is done by calling Func("FunctionName")
		-Exception classes aren't, and can't be, derived from KeysharpObject. That is because for the Exception mechanics to work in C#, all exception objects must be derived from the base System.Exception class, and multiple inheritance is not allowed.
		-CallbackCreate() does not support the CDecl/C option because the program will be run in 64-bit mode.
			-The Fast/F option is enabled by default because threads are not implemented yet.
			-The ParamCount parameter is unused. The callback that gets created supports passing up to 31 parameters and the number that actually gets passed is adjusted internally.
			-Passing string pointers to DllCall() when passing a created callback is strongly recommended against. This is because the string pointer cannot remain pinned, and is likely to crash the program if the pointer gets moved by the GC.
			-Usage of the created callback will be extremely inefficient, so usage of CallbackCreate() is discouraged.
		-Deleting a tab via GuiCtrl.Delete() does not reassociate the controls that it contains with the next tab. Instead, they are all deleted.
		-The size and positioning of some GUI components will be slightly different than AHK because WinForms uses different defaults.
		-The class name for statusbar/statusstrip objects created by Keysharp is "WindowsForms10.Window.8.app.0.2b89eaa_r3_ad1". However, for accessing a statusbar created by another, non .NET program, the class name is still "msctls_statusbar321".
		-Using the class name with ClassNN on .NET controls gives long, version specific names such as "WindowsForms10.Window.8.app.0.2b89eaa_r3_ad1" for a statusbar/statusstrip.
			-This is because a simpler class names can't be specified in code the way they can in AHK with calls to CreatWindowEx().
			-These long names may change from machine to machine, and may change for the same GUI if you edit its code.
			-There is an new NetClassNN property alongside ClassNN.
			-All GUI controls created in Keysharp are prefixed with the string "Keysharp", eg: KeysharpButton, KeysharpEdit etc...
			-NetClassNN will give values like 'KeysharpButton6' (note that the final digit is the same for the ClassNN and the NetClassNN). 
			-This is used internally in the index operator for the Gui class, where if a control with a matching ClassNN is not found, then controls are searched for their NetClassNN values.
		-Tooltips function slightly differently.
			-When specifying a coordinate for a ToolTip, it will attempt to show it relative to the currently focused Keysharp form.
			-If there is no focused form, it will attempt to show it relative to the last form which was created. If none are found, it will use the main form shown when double clicking the tray icon.
			-If the form is minimized, then it will attempt to use the RestoreBounds property of the form. This may not work sometimes, so the ToolTip may never show in that case.
			-Tooltips cannot be used if the script is not persistent (meaning, it has no main window). This is because C# tooltips require a parent control or form.
		-TrayTip() function slightly differently.
			-Muting the sound played by the tip is not supported with the "Mute" option. The sound will be whatever the user has configured in their system settings.
			-The option 4 to use the program's tray icon is not supported. It is always shown in the title of the tip.
			-The option 32 to use the large version of the program's tray icon is not supported. Windows will always show the small version.
		-Sleep() works, but uses Application.DoEvents() under the hood which is not a good programming practice and can lead to hard to solve bugs.
			-For this reason, it's recommended that users use timers for repeated execution rather than a loop with calls to Sleep().
		-The Optimization section of the #HotIf documentation doesn't apply to Keysharp because it uses compiled code, thus the expressions are never re-evaluated.
		-For transparent controls which are intended to be overlaid over other controls, special steps must be taken.
			-Specify BackgroundTrans in the options for the transparent control.
			-Then set the .Parent property of the transparent control to the one it's laid over.
			-This makes the x and y coordinates of the control be relative to its parent, which may be different than the overall form if it's a nested control.
		-The #ErrorStdOut directive will not print to the console unless piping is used. For example:
			.\Keysharp.exe .\test.ahk | more
			.\Keysharp.exe .\test.ahk | more > out.txt
		-AddStandard() detects menu items by string, instead of ID, because WinForms doesn't expose the ID.
		-The built in class methods __Init() and __New are not static. They are instance methods so they can access static and instance member variables.
		-Function objects are much slower than direct function calls due to the need to use reflection. So for repeated function calls, such as those involving math, it's best to use the functions directly.
		
### How do I get set up? ###

####Windows####
	Download Visual Studio 2022
	Open Keysharp.sln
	Build all (building the installer is not necessary)
	CD to bin\release\net7.0-windows
	Run .\Keysharp.exe yourtestfile.ahk
	
	If you run the installer, the install path can be optionally added to the $PATH varible, so you can run it from the command line from anywhere.
	It also registeres Keysharp.exe as the default program to open .ks files. So after installing, double click any .ks file to run it.

### Who do I talk to? ###

	Please make an account here and post a ticket.