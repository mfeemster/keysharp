; #Include %A_ScriptDir%/header.ahk

if (FileExist("./testini2.ini"))
	FileDelete("./testini2.ini")

dir := "../../../Keysharp.Tests/Code/testini.ini"
FileCopy(dir, "./testini2.ini", true)

if (FileExist("./testini2.ini"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

val := IniRead("./testini2.ini", "sectionone", "keyval")

if ("theval" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey1=groupval1" . A_NewLine . "groupkey2=groupval2" . A_NewLine . "groupkey3=groupval3" . A_NewLine == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

val := IniRead("./testini2.ini")

if ("sectionone" . A_NewLine . "sectiontwo" . A_NewLine . "sectionthree" . A_NewLine == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniWrite("thevalnew", "./testini2.ini", "sectionone", "keyval")
val := IniRead("./testini2.ini", "sectionone", "keyval")

if ("thevalnew" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

str := "groupkey11=groupval11" . A_NewLine . "groupkey12=groupval12" . A_NewLine . "groupkey13=groupval13" . A_NewLine
IniWrite(str, "./testini2.ini", "sectiontwo")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey11=groupval11" . A_NewLine . "groupkey12=groupval12" . A_NewLine . "groupkey13=groupval13" . A_NewLine == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniDelete("./testini2.ini", "sectiontwo", "groupkey11")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey12=groupval12" . A_NewLine . "groupkey13=groupval13" . A_NewLine == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

b := false

try
{
    val := IniRead("./testini2.ini", "sectiontwo", "thiskeydoesntexist")
}
catch
{
    b := true
}

if (b)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"
    
b := false

try
{
    val := IniRead("./testini2.ini", "sectiontwo", "thiskeydoesntexist", 123)
}
catch
{
    b := true
}

if (!b && val == 123)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniDelete("./testini2.ini", "sectiontwo")
val := IniRead("./testini2.ini", "sectiontwo")

if ("" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"
	
if (FileExist("./testini2.ini"))
	FileDelete("./testini2.ini")
