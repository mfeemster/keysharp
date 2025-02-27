i := 0

Loop 5 {
	i++
	f1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f1() {
	Loop {
		return A_Index := 0 ; test premature exit from loop to ensure Pop() is still called.
	}
}

i := 0

Loop 5 {
	i++
	f2()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f2() {
	Loop {
		Loop {
			return A_Index := 0
		}
	}
}

i := 0

Loop 5 {
	i++
	f3()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f3()
{
	arr := [10, 20, 30]

	for (a in arr)
		return A_Index := 0
}

i := 0

Loop 5 {
	i++
	f4()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f4()
{
	arr := [10, 20, 30]

	for (a in arr)
		for (b in arr)
			return A_Index := 0
}

i := 0

while i < 5
{
	i++
	f1()
	f2()
	f3()
	f4()
	
	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

i := 0

Loop 5 {
	i++
	w1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

w1() {
	while true {
		return A_Index := 0
	}
}

i := 0

Loop 5 {
	i++
	w2()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

w2() {
	while true {
		while true {
			return A_Index := 0
		}
	}
}

i := 0

Loop 5 {
	i++
	flu1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

flu1() {
	Loop {
		return A_Index := 0
	}
	until false
}

i := 0

Loop 5 {
	i++
	fwu1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

fwu1() {
	while true {
		return A_Index := 0
	}
	until false
}

i := 0

Loop 5 {
	i++
	ffu3()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ffu3()
{
	arr := [10, 20, 30]

	for (a in arr)
		return A_Index := 0
	until false
}

; test this here because this test is run outside of a function. 
arr := [10, 20, 30]
loopvar := 0 ; Test global var having the same name as the loop var. Ensure they are the same variable.

for (loopvar in arr)
{
}

if (loopvar == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

aglobalvar := 0

testglobalvarfunc()
{
	global aglobalvar

	for (aglobalvar in arr)
	{
	}

	if (aglobalvar == 0)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

testglobalvarfunc()
