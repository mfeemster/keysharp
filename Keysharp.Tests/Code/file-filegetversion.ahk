; #Include %A_ScriptDir%/header.ahk

dir := "./Keysharp.Core.dll"
ver := FileGetVersion(dir)
split := StrSplit(ver, ".")
len := split.Length()

if (len == 4)
 	FileAppend, pass, *
else
  	FileAppend, fail, *