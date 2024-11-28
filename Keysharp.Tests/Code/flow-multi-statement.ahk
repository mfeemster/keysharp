x := 1
y := 2
z := 3

xx := (1, 2, 3)

If (xx == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := x > 0 ? (x, y, z) : 123

If (xx == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := x > 0 ? 123 : (x, y, z)

If (xx == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

func(a)
{
    return a
}

xx := 0
xx := func((x, y))

If (xx == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := func((x, y, z))

If (xx == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

func3(a, b, c)
{
    return a + b + c
}

xx := 0
xx := func3((1, 2), (3, 4), (5, 6))

If (xx == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := x > 0 ? func3((1, 2), (3, 4), (5, 6)) : 123

If (xx == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := x > 0 ? 123 : func3((1, 2), (3, 4), (5, 6))

If (xx == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := x > 0 ? func3((1, 2), (3, 4), (5, 6)) : func3((1, 2), (3, 4), (5, 6))

If (xx == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := 0
xx := func3((1, 2), (3, 4), (5, 6)) ? func3((1, 2), (3, 4), (5, 6)) : func3((1, 2), (3, 4), (5, 6))

If (xx == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
x := 1, ++x, y := x

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := y := 1
x := 1
, ++x
, y := x

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

Test(x) => (++x, x := x + 3)

y := Test(123)

If (y == 127)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1 ? (a := 1, ++a) : ""

If (a == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a := 0, ++a)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (func((a := 0, ++a)))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (func3((1, 2), (3, 4), (5, 6)) == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 144
ct := 0
while (x -= func3((1, 2), (3, 4), (5, 6)))
{
	ct++
}

if (ct == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := (1, 2, 3) == 3

if (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := (1, 2, 3) == 3

if ((1, 2, 3) == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if ((1, 2, 3))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [(1, 2), (3, 4)]

if (arr.Length == 2 && arr[1] == 2 && arr[2] == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [func3(1, 2, 3), func3(3, 4, 5)]

if (arr.Length == 2 && arr[1] == 6 && arr[2] == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
arr := [func3(1, 2, 3),, func3(3, 4, 5)]

if (arr.Length == 3 && arr[1] == 6 && arr[2] == unset && arr[3] == 12)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [,func3(1, 2, 3),]

if (arr.Length == 3 && arr[1] == unset && arr[2] == 6 && arr[3] == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Dummy test that does nothing but test a very complex ternary.
class Toggle {
    static A := Map()
	
	TestFunc(a, b, c := "")
	{
	}

    __New(F, P, I:=0) => (TestFunc(F, !P ? (A.Has(F) && A.Delete(F))*0 : A.Has(F) && A[F] = P ? !A.Delete(F) : (I && F.Call(), A[F] := P)))
}
