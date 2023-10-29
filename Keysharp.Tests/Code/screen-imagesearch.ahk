;#Include %A_ScriptDir%/header.ahk

x :=
y := 
CoordMode("Mouse", "Screen")
GetScreenClip(1000, 1000, 100, 100, "./imagesearch.bmp")
ImageSearch(&x, &y, 0, 0, 1500, 1500, "./imagesearch.bmp")

if (x == 1000 && y == 1000)
	FileAppend, pass, *
else
	FileAppend, fail, *