;#Include %A_ScriptDir%/header.ahk

CoordMode("Mouse", "Screen")
GetScreenClip(1000, 900, 100, 100, "./imagesearch.bmp")
found := ImageSearch(0, 0, 1500, 1080, "./imagesearch.bmp")

if (found.OutputVarX == 1000 && found.OutputVarY == 900)
	FileAppend, pass, *
else
	FileAppend, fail, *