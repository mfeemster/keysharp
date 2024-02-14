;#Include %A_ScriptDir%/header.ahk

match := ""

RegExMatch("abc123abc456", "abc\d+", &match, 1)

if (match[0] == "abc123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("abc123abc456", "456", &match, -1)

if (match[0] == "456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("abc123abc456", "abc", &match, -1)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("abc123abc456", "abc", &match, -15)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
RegExMatch("abc123abc456", "abc", &match, -5)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("abc123abc456", "abc\d+", &match, 2)

if (match[] == "abc456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
RegExMatch("abc123123", "123$", &match, 1)

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("xxxabc123xyz", "abc.*xyz", &match)

if (match.Pos() == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
RegExMatch("abc123123", "123$", &match)

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
RegExMatch("abc123", "i)^ABC", &match)

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("abcXYZ123", "abc(.*)123", &match)

if (match[1] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos(1) == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
RegExMatch("abcXYZ123", "abc(?<testname>.*)123", &match)

if (match["testname"] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos("testname") == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Name("testname") == "testname")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("C:\Foo\Bar\Baz.txt", "\w+$", &match)

if (match[0] == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match.Pos() == 16)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

RegExMatch("Michiganroad 72", "(.*) (?<nr>\d+)", &match)

if (match.Count == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match[1] == "Michiganroad")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Name(2) == "nr")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match[2] == "72")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
; Same, but with ~= operator

match := "abc123abc456" ~= "abc\d+"

if (match[0] == "abc123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
match := "abc123123" ~= "123$"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
match := "xxxabc123xyz" ~= "abc.*xyz"

if (match.Pos() == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := "abc123123" ~= "123$"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
match := "abc123" ~= "i)^ABC"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := "abcXYZ123" ~= "abc(.*)123"

if (match[1] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos(1) == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
match := "abcXYZ123" ~= "abc(?<testname>.*)123"

if (match["testname"] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos("testname") == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Name("testname") == "testname")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

match := "C:\Foo\Bar\Baz.txt" ~= "\w+$"

if (match[0] == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match.Pos() == 16)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
match := "Michiganroad 72" ~= "(.*) (?<nr>\d+)"

if (match.Count == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match[1] == "Michiganroad")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Name(2) == "nr")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match[2] == "72")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"