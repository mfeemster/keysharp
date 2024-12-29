

x := 1
y := 0
z := 2
a := 2
b := 3
c := 0.9
d := 1.1
e := 0.5
f := 0.8
g := -1
h := 2
i := -3
j := -2
k := -0.9
l := -0.5
m := -0.8

If x > y and x < z
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > z and x < y
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > a and x < b
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > c and x < d
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > d and x < c
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > e and x < f
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > g and x < h
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > h and x < g
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > i and x < j
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > j and x < i
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > k and x < d
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > d and x < k
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > l and x < m
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"