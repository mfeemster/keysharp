; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy/file1.txt"
text := FileRead(dir)

if (text == "this is file 1")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

text := FileRead(dir, "m4")

if (text == "this")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

text := FileRead(dir, "m4 utf-8")

if (text == "this")
 	FileAppend, pass, *
else
  	FileAppend, fail, *

buf := FileRead(dir, "m4 raw")
buf2 := [ 116, 104, 105, 115 ]

if (buf = buf2)
 	FileAppend, pass, *
else
  	FileAppend, fail, *