;Simple assign.

x := 123

if (x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;String concat.

x := "this is a string"
. " and another string"

if (x == "this is a string and another string")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Comment at end.

x := 456 ; This is a comment.

if (x == 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Multiline comment at end.

x := 100/*This is an multiline comment at the end.*/

if (x == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Multiline comment in between.

x := /*This is a multiline comment inline.*/200

if (x == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Multiline comment in between multiline expression.

x := 55/*
This
is
a multiline comment
in a multiline expression.
*/+ 2

if (x == 57)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Multiline multi assignment.
x :=
y := 200

if (x == 200 && y == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Hotkey.
#if WINDOWS
d::
{
	global x := 123
}
#endif

;Exclude ++ and -- from continuation.

x := 0

funcincrement1()
{
	global x += 1
}

funcincrement1()

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0

funcincrement2()
{
	global x += 1
}

funcincrement2()

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0

funcdecrement1()
{
	global x -= 1
}

funcdecrement1()

if (x == -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0

funcdecrement2()
{
	global x -= 1
}

funcdecrement2()

if (x == -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0

funcincrementdecrement()
{
	global x
	x++
	x--
	++x
	--x
}

if (x == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a map multiline with a comment inline.

m := { ;This is a comment.
one : 1
}

if (m.one == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a map multiline with each part on a different line.

b := 100
m := {
a
:
b
}

if (m.a == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a map multiline with each part on a different line, including the first brace.

b := 200
m :=
{
a
:
b
}

if (m.a == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a multiline array with each part on a different line.

m := [
1
, 2
, 3
]

if (m[1] == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
;Construct an array with each part on a different line, including the first bracket.

m :=
[
4
, 5
, 6
]

if (m[2] == 5)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct an array on one line with operators inline.

m := [ 1 * 2, 2 * 2, 3 * 2 ]

if (m[1] == 2 && m[2] == 4 && m[3] == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct an array on one line with lambdas with operators inline.

m := [ (a) => a * 1, (a) => a * 2, (a) => a * 3 ]

if (m[1](0) == 0 && m[2](2) == 4 && m[3](3) == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct an empty map on the fly inside of a conditional.

x := 0

if (x == {}.OwnPropCount())
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Function that takes a parameter and returns it.

func1(p1)
{
	return p1
}

if (func1(123) == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Function call on multiple lines with operators inline on the beginning and end of each line.

x := func1(1
+ 2 +
3 +
4)
; MsgBox(x)

if (x == 10)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a map on the fly, and pass as a function argument with a comment inline.

m := func1({ ;This is a comment.
one : 1 /*This is a multiline comment.*/
})

if (m.one == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct an array on the fly, and pass as a function argument with a comment inline.

m := func1([ ;This is a comment.
1/*This is a multiline comment.*/
, 2
, 3
])

if (m[3] == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Construct a map on the fly with a comment inline and pass as a function argument to a function passed to a conditional.

if (func1({ ; continuation
	a
	: "two"
}).OwnPropCount() == 1) {
	FileAppend "pass", "*"
}
else
	FileAppend "fail", "*"

;Combine multiline assignment with operators with .

x := 1
+ 2 +
3
* func1(1
+ 2
+ 3 + 4)

if (x == 33)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Function that takes a 3 parameters and returns their sum.

func2(p1, p2, p3)
{
	return p1 + p2 + p3
}

;Function call with args on separate lines.

x := func2(1
, 2
, 3
)

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;Function call with map defined inline on separate lines passed directly to a conditional with OTB.

if (func2(
1,
2,
3
) == 6) {
	FileAppend "pass", "*"
}
else
	FileAppend "fail", "*"

;OTB function definition.

func3(p1) {
	
	If (p1 != 0) {
		return p1 * 2
	} Else {
		return p1
	}
}

x := func3(0)

if (x == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := func3(2)

if (x == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; Function with loop whose variables get passed to another function.
; Ensure they are not defined as function variables outside of the loop scope.
m := Map("one", 1)

mapfunc()
{
	for k, v in m
	{
		afunc(k, v)
	}
}

afunc(kk, vv)
{
}

; Same, but inside of a class property.
class mylooppropclass
{
	__item[p1*]
	{
		set
		{
			temp := 0

			for n in p1
			{
				temp += n
			}
		}
	
		get
		{
			m := Map("one", 1)
			
			for k,v in m
			{
				afunc(k, v)
			}

			return 1
		}
	}

	afunc(kk, vv)
	{
	}
}

;Test ending a file with a multiline comment.
ExitApp()/*
asdf
asdf
*/