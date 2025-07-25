
outputVarCount :=
match := RegExReplace("abc123123", "123$", "xyz")

if (match == "abc123xyz")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := RegExReplace("abc123", "i)^ABC")

if (match == "123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := RegExReplace("abcXYZ123", "abc(.*)123", "aaa$1zzz")

if (match == "aaaXYZzzz")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := RegExReplace("abc123abc456", "abc\d+", "", &outputVarCount)

if (match == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (outputVarCount == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := RegExReplace("abc", ".", (m) => m[] == "a" ? 1 : m[] == "b" ? 2 : 3, &outputVarCount:=0)

if (match == "123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (outputVarCount == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"