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

if ("groupkey1=groupval1`ngroupkey2=groupval2`ngroupkey3=groupval3" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

val := IniRead("./testini2.ini")

if ("sectionone`nsectiontwo`nsectionthree" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniWrite("thevalnew", "./testini2.ini", "sectionone", "keyval")
val := IniRead("./testini2.ini", "sectionone", "keyval")

if ("thevalnew" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

str := "groupkey11=groupval11`ngroupkey12=groupval12`ngroupkey13=groupval13"
IniWrite(str, "./testini2.ini", "sectiontwo")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey11=groupval11`ngroupkey12=groupval12`ngroupkey13=groupval13" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniDelete("./testini2.ini", "sectiontwo", "groupkey11")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey12=groupval12`ngroupkey13=groupval13" == val)
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
val := IniRead("./testini2.ini", "sectiontwo",, "")

if ("" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"
	
if (FileExist("./testini2.ini"))
	FileDelete("./testini2.ini")
