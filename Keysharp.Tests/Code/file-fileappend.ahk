; #Include %A_ScriptDir%/header.ahk

if (FileExist("./fileappend.txt"))
	FileDelete("./fileappend.txt")
		
if (FileExist("./fileappend2.txt"))
	FileDelete("./fileappend2.txt")

FileAppend("test file text", "./fileappend.txt")

if (FileExist("./fileappend.txt"))
	FileAppend, pass, *
else
	FileAppend, fail, *

FileAppend("test file text", "./fileappend.txt")
text := FileRead("./fileappend.txt")

if (text == "test file texttest file text")
	FileAppend, pass, *
else
	FileAppend, fail, *

data := [ 1, 2, 3, 4]
FileAppend(data, "./fileappend2.txt", "utf-8-raw")

if (FileExist("./fileappend2.txt"))
	FileAppend, pass, *
else
	FileAppend, fail, *

data2 := FileRead("./fileappend2.txt", "raw")

if (Buffer(data) = data2)
	FileAppend, pass, *
else
	FileAppend, fail, *

FileAppend("abcd", "./fileappend2.txt", "utf-16-raw")
data2 := FileRead("./fileappend2.txt", "raw")
data := Buffer([ 1, 2, 3, 4, 97, 0, 98, 0, 99, 0, 100, 0 ])

if (data = data2)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (FileExist("./fileappend.txt"))
	FileDelete("./fileappend.txt")
		
if (FileExist("./fileappend2.txt"))
	FileDelete("./fileappend2.txt")
