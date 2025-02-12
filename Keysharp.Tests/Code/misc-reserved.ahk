char := 123

if (char == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

resfunc(short, float, double)
{
	return short + float + double
}

int := resfunc(1, 2, 3)

if (int == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myclass
{
	char := 1
	ushort := 2
	ulong := 3

	__New(double, float, string)
	{
		global
		char := double
		ushort := float
		ulong := string
	}

	GetSum()
	{
		global
		return char + ushort + ulong
	}
}

mc := myclass(4, 5, 6)
sbyte := mc.GetSum()

if (sbyte == 15)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

resfunc2()
{
	string := 123
	return string
}

xx := resfunc2()

if (xx == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
