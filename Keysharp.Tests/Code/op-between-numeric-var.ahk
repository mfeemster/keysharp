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

If x between %y% and %z%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x between %z% and %y%
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If x between %a% and %b%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x between %c% and %d%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x between %d% and %c%
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If x between %e% and %f%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x between %g% and %h%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x between %h% and %g%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x between %i% and %j%
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If x between %j% and %i%
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If x between %k% and %d%
	FileAppend, pass, *
else
	FileAppend, fail, *

If x between %d% and %k%
	FileAppend, fail, *
else
	FileAppend, pass, *

If x between %l% and %m%
	FileAppend, fail, *
else
	FileAppend, pass, *