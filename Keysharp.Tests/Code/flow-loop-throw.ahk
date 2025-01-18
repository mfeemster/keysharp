i := 0

Loop 5 {
	i++
try
{
	f1()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f1() {
	Loop {
		A_Index := 0 ; test premature exit from loop to ensure Pop() is still called.
		throw 1
	}
}

i := 0

Loop 5 {
	i++
try
{
	f2()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f2() {
	Loop {
		Loop {
			A_Index := 0
			throw 1
		}
	}
}

i := 0

Loop 5 {
	i++
try
{
	f3()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

f3()
{
	arr := [10, 20, 30]

	for (a in arr)
	{
		A_Index := 0
		throw 1
	}
}

i := 0

Loop 5 {
	i++
try
{
	f4()
}

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
		{
			A_Index := 0
			throw 1
		}
}

i := 0

while i < 5
{
	i++
try
{
	f1()
}

try
{
	f2()
}

try
{
	f3()
}

try
{
	f4()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

i := 0

Loop 5 {
	i++
try
{
	tw1()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

tw1() {
	while true {
		A_Index := 0
		throw 1
	}
}

i := 0

Loop 5 {
	i++
try
{
	tw2()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

tw2() {
	while true {
		while true {
			A_Index := 0
			throw 1
		}
	}
}

i := 0

Loop 5 {
	i++
	ftc1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ftc1() {
	Loop 2 {
		A_Index := 0
		try
		{
			throw 1
		}
		break
	}
}

i := 0

Loop 5 {
	i++
	ftc2()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ftc2() {
	Loop 2 {
		Loop 2 {
			A_Index := 0
			try
			{
				throw 1
			}
			break
		}
	}
}

i := 0

Loop 5 {
	i++
	ftc3()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ftc3()
{
	arr := [10, 20, 30]

	for (a in arr)
	{
		A_Index := 0
		try
		{
			throw 1
		}
	}
}

i := 0

Loop 5 {
	i++
	ftc4()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ftc4()
{
	arr := [10, 20, 30]

	for (a in arr)
		for (b in arr)
		{
			A_Index := 0
			try
			{
				throw 1
			}
		}
}

i := 0

Loop 5 {
	i++
	wtc1()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

wtc1() {
	while true {
		A_Index := 0
		try
		{
			throw 1
		}
		break
	}
}

i := 0

Loop 5 {
	i++
	wtc2()

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

wtc2() {
	while true {
		while true {
			A_Index := 0
			try
			{
				throw 1
			}
			break
		}
		break
	}
}

i := 0

Loop 5 {
	i++
try
{
	flut1()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

flut1() {
	Loop {
		A_Index := 0
		throw 1
	}
	until false
}

i := 0

Loop 5 {
	i++
try
{
	fwut1()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

fwut1() {
	while true {
		A_Index := 0
		throw 1
	}
	until false
}

i := 0

Loop 5 {
	i++
try
{
	ffu1()
}

	if (i == A_Index)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

ffu1()
{
	arr := [10, 20, 30]

	for (a in arr)
	{
		A_Index := 0
		throw 1
	}
	until false
}
