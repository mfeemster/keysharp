; #Include %A_ScriptDir%/header.ahk

FileEncoding("utf-8")
fe := A_FileEncoding

if (fe == "utf-8")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("utf-8-raw")
fe := A_FileEncoding

if (fe == "utf-8-raw")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("utf-16")
fe := A_FileEncoding

if (fe == "utf-16")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("unicode")
fe := A_FileEncoding

if (fe == "utf-16")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("utf-16-raw")
fe := A_FileEncoding

if (fe == "utf-16-raw")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("ascii")
fe := A_FileEncoding

if (fe == "us-ascii")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileEncoding("us-ascii")
fe := A_FileEncoding

if (fe == "us-ascii")
 	FileAppend, pass, *
else
  	FileAppend, fail, *