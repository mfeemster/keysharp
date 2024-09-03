filename := "./asciiart.txt"

attr := FileExist(filename)

if (attr == "N" || attr == "A")
	FileDelete(filename)

Download("http://textfiles.com/art/asciiart.txt", filename)
attr := FileExist(filename)

if ("A" == attr || "N" == attr)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
size := FileGetSize(filename)

if (16048 == size)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

attr := FileExist(filename)

if ("A" == attr || "N" == attr)
	FileDelete(filename)