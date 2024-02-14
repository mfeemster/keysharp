;#Include %A_ScriptDir%/header.ahk

x = 1

If x = 2
	FileAppend, "fail", "*"

	
ELSE if x    =1
	FileAppend, "pass", "*"
else FileAppend, "fail", "*"

if x = 0
{
	FileAppend, "fail", "*"
}
else
{
	FileAppend, "pass", "*"
}

if x = 1 {
	FileAppend, "pass", "*"
	if x = 2
		{
			FileAppend, "fail", "*"
}
} else if x = 2
{	FileAppend, "fail", "*"
} else { FileAppend, "pass", "*"
}

x := 123

if (!x) {
} else if (x == 123) { FileAppend, "pass", "*"
}

x := 1

if (x == 1) {
	FileAppend, "pass", "*"
} else {
	FileAppend, "fail", "*"
}

if (x == 1) {
	FileAppend, "pass", "*"
} else
	FileAppend, "fail", "*"

; Ensure else blocks are attached to the proper parent if block.
x := 1

if x = 1
{
	if (x = 2)
	{
		FileAppend, "fail", "*"
	}
	else
	{
		FileAppend, "pass", "*"
	}
}
else
{
	FileAppend, "fail", "*"
}

if x = 1
{
	if (x = 2)
	{
		FileAppend, "fail", "*"
	}
	else if (x = 1)
	{
		FileAppend, "pass", "*"
	}
}
else
{
	FileAppend, "fail", "*"
}

if x =
	FileAppend, "fail", "*"
else if x !=
	FileAppend, "pass", "*"
	
if x ==
	FileAppend, "fail", "*"
else if x !=
	FileAppend, "pass", "*"

if (x =)
	FileAppend, "fail", "*"
else if (x != )
	FileAppend, "pass", "*"
	
if (x ==)
	FileAppend, "fail", "*"
else if (x !=)
	FileAppend, "pass", "*"

x := ""
b := true
c := false

If b
	x := 123

if (c)
	b := 123
else
	b := 456
	
if (x == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (b == 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [123, 456, 789]

if (arr) ; Objects are always considered true.
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"