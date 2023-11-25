;#Include %A_ScriptDir%/header.ahk

compfunc(x, y, z)
{
	return StrCompare(x, y)
}

x := "Z,X,Y,F,D,B,C,A,E"
y := Sort(x, "D,")

if ("A,B,C,D,E,F,X,Y,Z" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := Sort(x, "D,", "compfunc")

if ("A,B,C,D,E,F,X,Y,Z" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

thefunc := Func("compfunc")
y := Sort(x, "D,", thefunc)

if ("A,B,C,D,E,F,X,Y,Z" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := Sort(x, "D, r")

if ("Z,Y,X,F,E,D,C,B,A" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "Z,X,Y,F,D,B,C,A,E,a,b,c,d,e"
y := Sort(x, "D,")

if ("A,a,B,b,C,c,D,d,E,e,F,X,Y,Z" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := Sort(x, "D, r")

if ("Z,Y,X,F,e,E,d,D,c,C,b,B,a,A" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := Sort(x, "D, c")

if ("A,B,C,D,E,F,X,Y,Z,a,b,c,d,e" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := Sort(x, "D, c r")

if ("e,d,c,b,a,Z,Y,X,F,E,D,C,B,A" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	

x := "200,100,300,500,600,111,222,1010"
y := Sort(x, "D, n")

if ("100,111,200,222,300,500,600,1010" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

Loop 10
{
	z := Sort(x, "D, n random")

	if (z != y)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
 	y := z
}

y := Sort(x, "D, n r")

if ("1010,600,500,300,222,200,111,100" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "RED`nGREEN`nBLUE`n"
y := Sort(x)

if ("BLUE`nGREEN`nRED" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := Sort(x, "z")

if ("`nBLUE`nGREEN`nRED" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "C:\AAA\BBB.txt,C:\BBB\AAA.txt"
y := Sort(x, "D, \")

if ("C:\BBB\AAA.txt,C:\AAA\BBB.txt" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "/usr/bin/AAA/BBB.txt,/usr/bin/BBB/AAA.txt"
y := Sort(x, "D, /")

if ("/usr/bin/BBB/AAA.txt,/usr/bin/AAA/BBB.txt" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "co-op,comp,coop"
y := Sort(x, "D, CL")

if ("comp,co-op,coop" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "Ä,Ü,A,a,B,b,u,U"
y := Sort(x, "D, CL")

if ("A,a,Ä,B,b,u,U,Ü" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "AZB,BYX,CWM,LMN"
y := Sort(x, "D, P2")

if ("LMN,CWM,BYX,AZB" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"