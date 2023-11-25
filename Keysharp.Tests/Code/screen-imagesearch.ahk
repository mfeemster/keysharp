;#Include %A_ScriptDir%/header.ahk

x :=
y := 
CoordMode("Mouse", "Screen")
GetScreenClip(10, 10, 500, 500, "./imagesearch.bmp")
ImageSearch(&x, &y, 0, 0, 1920, 1080, "./imagesearch.bmp")

if (x == 10 && y == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"