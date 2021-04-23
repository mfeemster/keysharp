;#Include %A_ScriptDir%/header.ahk

if (0.36787944117144233 == Exp(-1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0.6065306597126334 == Exp(-0.5))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1 == Exp(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1 == Exp(-0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1.6487212707001282 == Exp(0.5))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (2.718281828459045 == Exp(1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1.9640329759698474 == Exp(0.675))
	FileAppend, pass, *
else
	FileAppend, fail, *
