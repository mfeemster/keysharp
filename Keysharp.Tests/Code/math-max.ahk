;#Include %A_ScriptDir%/header.ahk

; Assert.AreEqual(-6, Maths.Max(-6, -6));
; Assert.AreEqual(-5, Maths.Max(-6, -5));
; Assert.AreEqual(-4.2, Maths.Max(-4.2, -5.0));
; Assert.AreEqual(0, Maths.Max(0, 0));
; Assert.AreEqual(1, Maths.Max(0, 1));
; Assert.AreEqual(1, Maths.Max(1, 1));
; Assert.AreEqual(2.3, Maths.Max(1.5, 2.3));
; Assert.AreEqual(1, Maths.Max(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675 }));
; Assert.AreEqual(2, Maths.Max(new object[] { -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 }));
; Assert.AreEqual(string.Empty, Maths.Max(new object[] { -1.0, "asdf" }));
			
if (Max(-6, -6) == -6)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (Max(-6, -5) == -5)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(-4.2, -5.0) == -4.2)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(0, 0) == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(0, 1) == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(1, 1) == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(1.5, 2.3) == 2.3)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(-1.0, "asdf") == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675 ]

if (Max(x) == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 ]

if (Max(x) == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(-1.0, -0.5, 0, 0.5, 1, 0.675) == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Max(-1.0, -0.5, 0, 0.5, 1, 0.675, 2.0) == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *
