buf := Buffer(100, 0)
NumPut("int", 1, buf)
b1 := NumGet(buf, 0, "int")

if (b1 == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("int", -1, buf)
b1 := NumGet(buf, 0, "int")

if (b1 == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("uint", 0xFFFFFFFF, buf)
b1 := NumGet(buf, 0, "uint")

if (b1 == 0xFFFFFFFF)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("char", 123, buf)
b1 := NumGet(buf, 0, "char")

if (b1 == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("char", -123, buf)
b1 := NumGet(buf, 0, "char")

if (b1 == -123)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
NumPut("uchar", 255, buf)
b1 := NumGet(buf, 0, "uchar")

if (b1 == 255)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("uchar", 256, buf)
b1 := NumGet(buf, 0, "uchar")

if (b1 == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("short", 10000, buf)
b1 := NumGet(buf, 0, "short")

if (b1 == 10000)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("short", -10000, buf)
b1 := NumGet(buf, 0, "short")

if (b1 == -10000)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("ushort", 50000, buf)
b1 := NumGet(buf, 0, "ushort")

if (b1 == 50000)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("ushort", 65536, buf)
b1 := NumGet(buf, 0, "ushort")

if (b1 == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("int64", 0xFFFFFFFFFFFFFFFF, buf)
b1 := NumGet(buf, 0, "int64")

if (b1 == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
NumPut("double", 1.2345, buf)
b1 := NumGet(buf, 0, "double")

if (b1 == 1.2345)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("double", -1.2345, buf)
b1 := NumGet(buf, 0, "double")

if (b1 == -1.2345)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("float", 1.2345, buf)
b1 := NumGet(buf, 0, "float")

if (b1 == 1.2345)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("float", -1.2345, buf)
b1 := NumGet(buf, 0, "float")

if (b1 == -1.2345)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("char", 1, buf)
NumPut("char", 2, buf, 1)
NumPut("char", 3, buf, 2)
NumPut("char", 4, buf, 3)

b1 := NumGet(buf, 0, "uint")

if (b1 == 0x04030201)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("int", 1234, "short", 10000, "double", 1.2345, buf)
b1 := NumGet(buf, 0, "int")

if (b1 == 1234)
	FileAppend, pass, *
else
	FileAppend, fail, *

b1 := NumGet(buf, 4, "short")

if (b1 == 10000)
	FileAppend, pass, *
else
	FileAppend, fail, *

b1 := NumGet(buf, 6, "double")

if (b1 == 1.2345)
	FileAppend, pass, *
else
	FileAppend, fail, *

NumPut("int", 0, buf)
NumPut("int", 0x01020304, buf, 2)
b1 := NumGet(buf, 0, "int")

if (b1 == 0x03040000)
	FileAppend, pass, *
else
	FileAppend, fail, *

buf := Buffer(4, 0)
NumPut("int", 123, buf)
val := false

try
{
	NumPut("int", 123, buf, 2)
}
catch
{
	val := true
}

if (val == true)
	FileAppend, pass, *
else
	FileAppend, fail, *