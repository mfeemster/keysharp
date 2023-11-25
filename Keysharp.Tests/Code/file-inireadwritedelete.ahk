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

if ("groupkey1=groupval1`r`ngroupkey2=groupval2`r`ngroupkey3=groupval3`r`n" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

val := IniRead("./testini2.ini")

if ("sectionone`r`nsectiontwo`r`nsectionthree`r`n" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniWrite("thevalnew", "./testini2.ini", "sectionone", "keyval")
val := IniRead("./testini2.ini", "sectionone", "keyval")

if ("thevalnew" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

str := "groupkey11=groupval11`r`ngroupkey12=groupval12`r`ngroupkey13=groupval13`r`n"
IniWrite(str, "./testini2.ini", "sectiontwo")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey11=groupval11`r`ngroupkey12=groupval12`r`ngroupkey13=groupval13`r`n" == val)
  	FileAppend, "pass", "*"
else
   	FileAppend, "fail", "*"

IniDelete("./testini2.ini", "sectiontwo", "groupkey11")
val := IniRead("./testini2.ini", "sectiontwo")

if ("groupkey12=groupval12`r`ngroupkey13=groupval13`r`n" == val)
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