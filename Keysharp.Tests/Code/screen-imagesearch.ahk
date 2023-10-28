;#Include %A_ScriptDir%/header.ahk

CoordMode("Mouse", "Screen")
GetScreenClip(1000, 900, 100, 100, "./imagesearch.bmp")
found := ImageSearch(0, 0, 1500, 1080, "./imagesearch.bmp")

; Need to eventually make this work with ownprops like monget.Left

if (found["X"] == 1000 && found["Y"] == 900)
	FileAppend, pass, *
else
	FileAppend, fail, *