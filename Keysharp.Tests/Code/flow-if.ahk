;#Include %A_ScriptDir%/header.ahk

x = 1

If x = 2
	FileAppend, fail, *

	
ELSE if x    =1
	FileAppend, pass, *
else FileAppend, fail, *

if x = 0
{
	FileAppend, fail, *
}
else
{
	FileAppend, pass, *
}

if x = 1 {
	FileAppend, pass, *
	if x = 2
		{
			FileAppend, fail, *
}
} else if x = 2
{	FileAppend, fail, *
} else { FileAppend, pass, *
}

if (x == 1) {
	FileAppend, pass, *
} else {
	FileAppend, fail, *
}

arr := [123, 456, 789]

if (arr) ; Objects are always considered true.
	FileAppend, pass, *
else
	FileAppend, fail, *