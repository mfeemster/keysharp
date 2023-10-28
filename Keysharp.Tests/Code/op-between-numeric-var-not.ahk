;#Include %A_ScriptDir%/header.ahk

x = 1
y = 0
z = 2
a = 2
b = 3
c = 0.9
d = 1.1
e = 0.5
f = 0.8
g = -1
h = 2
i = -3
j = -2
k = -0.9
l = -0.5
m = -0.8

If x not between %y% and %z%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x not between %z% and %y%
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If x not between %a% and %b%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x not between %c% and %d%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x not between %d% and %c%
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If x not between %e% and %f%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x not between %g% and %h%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x not between %h% and %g%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x not between %i% and %j%
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If x not between %j% and %i%
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If x not between %k% and %d%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x not between %d% and %k%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x not between %l% and %m%
	FileAppend, pass, *
else
	FileAppend, fail, *