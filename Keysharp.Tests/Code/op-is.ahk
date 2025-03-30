x := 0

if (x is integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is Integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 123

if (x is integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is Integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -123

if (x is integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is Integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0.0

if (x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (not x is Integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 123.0

if (x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (not x is Integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -123.0

if (x is float)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is float)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is integer)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (not x is Integer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is number)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (not x is number)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x is string)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (not x is string)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := {}

if (x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := []

if (x is array)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is object)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := (*) => 1

if (x is not Closure)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is Func)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

f() => (x := 1, (*) => x)
x := f()

if (x is Closure)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is Func)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"