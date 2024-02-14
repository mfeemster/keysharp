

x := 1
z := ""

switch x
{
	case 3:
		z := 3
	case 2:
		z := 2
	case 1:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ""

switch x
{
	case 3:
		z := 3
	case 2:
		z := 2
	default:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ""

switch x
{
	case 3:
	case 2:
	case 1:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ""

switch x
{
	case 3, 2, 1:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "Tester"
z := ""

switch x, 0
{
	case "mismatch":
		z := 3
	case "notthis":
		z := 2
	case "tester":
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "Tester"
z := ""

switch x, 1
{
	case "mismatch":
		z := 3
	case "notthis":
		z := 2
	case "tester":
		z := 0
	case "Tester":
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "Tester"
z := ""

switch x, 1
{
	case "mismatch", "notthis", "tester":
		z := 2
	case "Tester":
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
z := ""

switch
{
	case x == 3:
		z := 3
	case x == 2:
		z := 2
	case x == 1:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
z := ""

switch
{
	case x > 5:
		z := 3
	case x > 0 && x < 4:
		z := 1
	default:
		z := 2
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
z := ""
y := ""

switch
{
	case "":
		z := 3
	case y:
		z := 2
	case 123:
		z := 1
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 123
z := ""

switch x, 1 ; this is a comment
{
	case "mismatch": ; another comment
		mism:
		z := 3
	case "notthis":
		z := 2
	case 123:
		goto mism ; last comment
	case "Tester":
		z := 1
}

if (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
z := 0

switch z
{
	case 10:
		x += 100
	case 20:
		x += 100
	case 30:
		x += 100
}

if (x == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 3
y := 4
z := 0

func(m, n)
{
	return m * n
}

switch func(x, y)
{   
    case 1:  z := 1
    case 2:  z := 2
    case 12: z := 3
	default: z := 4
}

if (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 3
y := 4
z := 0

switch func(x, y)
{   
    case 1, 2, func(3, 4): z := 1
	default: z := 2
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myclass
{
	func(m, n)
	{
		return m * n
	}
}

myclassobj := myclass()

x := 3
y := 4
z := 0

switch myclassobj.func(x, y)
{   
    case 1, 2, myclassobj.func(3, 4): z := 1
	default: z := 2
}

if (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"