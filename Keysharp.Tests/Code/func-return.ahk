func0() {
}

x := func0()

If (x == "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

func1(a)
{
	return a
}

x := func1(123)

If (x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
func2(a) {
	return a * 2
}

x := func2(4)

If (x == 8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
func3()
{
	return [10, 20, 30]
}

x := func3()

If (x = [10, 20, 30])
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
func4()
{
	return { one : 1 }
}

x := func4()

If (x.one == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
func5()
{
	return {
	two : 2
}
}

x := func5()

If (x.two == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"