x := 0

Loop 10
{
	x++
	If (A_Index == 5)
		goto l1
}

l1:

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop 10
{
	x++
	If (A_Index == 5)
		break 1
}

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

looplabel1:
Loop 10
{
	x++
	If (A_Index == 5)
		break looplabel1
}

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

outerlooplabel1:
Loop 10
{
	x++
	If (A_Index == 5)
		break outerlooplabel1
}

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

outerlooplabel2:
Loop 10
{
	innerlooplabel2:
	Loop 10
	{
		x++
		If (A_Index == 5)
			break innerlooplabel2
	}
	
	x := 999
}

if (x == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

outerlooplabel3:
Loop 10
{
	innerlooplabel3:
	Loop 10
	{
		x++
		If (A_Index == 5)
			break outerlooplabel3
	}
	
	x := 999
}

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			break 2
	}
	
	x := 999
}

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			goto l4
	}
	
	x := 999
}
l4:

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			goto l5
	}
l5:
	x := 999
}

if (x == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 123

Loop 10
{
	x++
	switch y
	{
		case 3:
			z := 3
		case 2:
			z := 2
		case 123:
			break
	}
}

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 123

Loop 10
{
	Loop 10
	{
		x++
		switch y
		{
			case 3:
				z := 3
			case 2:
				z := 2
			case 123:
				break 2
		}
	}
}

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 123

outerswitchlooplabel1:
Loop 10
{
	innerswitchlooplabel1:
	Loop 10
	{
		x++
		switch y
		{
			case 3:
				z := 3
			case 2:
				z := 2
			case 123:
				break outerswitchlooplabel1
		}
	}
	
	x := 999
}

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_Index == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"