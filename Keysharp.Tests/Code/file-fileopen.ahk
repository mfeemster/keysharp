; #Include %A_ScriptDir%/header.ahk

path := "./testfileobject1.txt"

if (FileExist(path) != "")
	FileDelete(path)

f := FileOpen(path, "rw") ; Simplest first, read/write.
w := "testing"
count := f.WriteLine(w)
f.Seek(0) ; Test seeking from beginning.
r := f.ReadLine()

if (r == "testing")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (count == 8) ; Add one for the newline.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f.Close()

if (FileExist(path) != "")
	FileDelete(path)

f := FileOpen(path, "rw") ; Read/write integers.
val := 0x01020304
count := f.WriteUInt(val)
f.Seek(0)
r := f.ReadUInt()

if (val == r)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (count == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val2 := -12345678
count := f.WriteInt(val2)
f.Seek(-4, 1) ; Test seeking from current.
r2 := f.ReadInt()

if (val2 == r2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (count == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f.Close()
		
if (FileExist(path) != "")
	FileDelete(path)

f := FileOpen(path, "rw") ; Read/write buffers and arrays.
buf := Buffer(4, 9)
count := f.RawWrite(buf)
f.Seek(0)
buf2 := Buffer(4, 0)
f.RawRead(buf2)

Loop (buf.Size)
{
	p1 := buf[A_Index]
	p2 := buf2[A_Index]

	if (p1 == p2)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f.Seek(0)
arr := Array()

Loop (buf.Size)
{
	arr.Push(A_Index)
}

f.RawRead(arr)

Loop (buf.Size)
{
	p1 := arr[A_Index]
	p2 := buf2[A_Index]

	if (p1 == p2)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f.Close()

if (FileExist(path) != "")
	FileDelete(path)

f := FileOpen(path, "rw", "Unicode") ; Test text encoding.
w := "testing"
count := f.Write(w)
f.Seek(2) ; A unicode file will have a 2 byte long byte order mark.
r := f.ReadLine()

if (r == "testing")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (count == 14) ; Unicode is two bytes per char.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (f.Length == 16) ; BOM plus 2 bytes per char.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
f.Close()

f := FileOpen(path, "rw", "Unicode") ; Ensure reading an existing file with a BOM works.
w := "testing"
r := f.ReadLine()

if (r == "testing")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (w.Length == r.Length)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f.Close()

if (FileExist(path) != "")
	FileDelete(path)
			
A_FileEncoding := "utf-8-raw"
f := FileOpen(path, "rw") ; Test position.
w := "testing"
count := f.Write(w)
pos := f.Pos
len := StrLen(w)

if (len == pos)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
eof := f.AtEOF

if (eof == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
len := f.Length

if (len == 7)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
enc := f.Encoding

if (enc == "utf-8")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f.Close()

; Do not delete here, file is used for appending.
f := FileOpen(path, "a") ; Test append.
w := "testing"
count := f.Write(w)
pos := f.Pos
eof := f.AtEOF

if (eof == 0) ; With append mode, you're never really at the "end" of the file.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
len := f.Length

if (pos == 14)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (len == 14)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f.Close()

if (FileExist(path) != "")
	FileDelete(path)
			
f := FileOpen(path, "w") ; Test write only.
w := "testing"
count := f.Write(w)
f.Close()

f := FileOpen(path, "w") ; Test write only on an existing file, which should clear it.
pos := f.Pos
eof := f.AtEOF
len := f.Length

if (eof == 1) ; Overwrite should cause it to be an empty file.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (pos == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (len == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
			
f.Close()

if (FileExist(path) != "")
	FileDelete(path)

f := FileOpen(path, "w") ; Test write only.
w := "testing"
count := f.Write(w)
f.Close()

f := FileOpen(path, "rw") ; Test read/write on an existing file, which should not clear it.
pos := f.Pos
eof := f.AtEOF
len := f.Length

if (eof == 0) ; At position zero, so not at EOF.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (pos == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (len == 7)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
			
f.Close()

b := false
#if WINDOWS
try
{
	FileOpen(path, "r -r")
	FileOpen(path, "r")
}
catch
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	FileOpen(path, "rw -w")
	FileOpen(path, "rw")
}
catch
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
#endif

b := false

try
{
	f := FileOpen(path, "r -r")
	handle := f.Handle;
	FileOpen(handle, "r h")
}
catch
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (FileExist(path) != "")
	FileDelete(path)

b := false

try
{
	FileOpen(path, "r")
}
catch
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"