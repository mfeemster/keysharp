match := ""

RegExMatch("abc123abc456", "abc\d+", &match, 1)

if (match[0] == "abc123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "abc123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc123")

RegExMatch("abc123abc456", "456", &match, -1)

if (match[0] == "456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "456")

RegExMatch("abc123abc456", "abc", &match, -1)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc")

RegExMatch("abc123abc456", "abc", &match, -15)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc")

RegExMatch("abc123abc456", "abc", &match, -5)

if (match[0] == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "abc")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc")

RegExMatch("abc123abc456", "abc\d+", &match, 2)

if (match[] == "abc456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc456")

RegExMatch("abc123123", "123$", &match, 1)

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "123")

RegExMatch("xxxabc123xyz", "abc.*xyz", &match)

if (match.Pos() == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc123xyz")

RegExMatch("abc123123", "123$", &match)

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "123")

RegExMatch("abc123", "i)^ABC", &match)

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc")

RegExMatch("abcXYZ123", "abc(.*)123", &match)

if (match[1] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.1 == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos(1) == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "01", "abcXYZ123XYZ")

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

CheckMatches(match, "0testname", "abcXYZ123XYZ")

RegExMatch("C:\Foo\Bar\Baz.txt", "\w+$", &match)

if (match[0] == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.0 == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match.Pos() == 16)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "txt")

RegExMatch("Michiganroad 72", "(.*) (?<nr>\d+)", &match)

if (match.Count == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match[1] == "Michiganroad")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match.1 == "Michiganroad")
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

if (match.2 == "72")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "01nr", "Michiganroad 72Michiganroad72")

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

CheckMatches(match, "0", "abc123")

match := "abc123123" ~= "123$"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "123")

match := "xxxabc123xyz" ~= "abc.*xyz"

if (match.Pos() == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc123xyz")

match := "abc123123" ~= "123$"

if (match.Pos() == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "123")

match := "abc123" ~= "i)^ABC"

if (match.Pos() == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "abc")

match := "abcXYZ123" ~= "abc(.*)123"

if (match[1] == "XYZ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (match.Pos(1) == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
CheckMatches(match, "01", "abcXYZ123XYZ")

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

CheckMatches(match, "0testname", "abcXYZ123XYZ")

match := "C:\Foo\Bar\Baz.txt" ~= "\w+$"

if (match[0] == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (match.Pos() == 16)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CheckMatches(match, "0", "txt")

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

CheckMatches(match, "01nr", "Michiganroad 72Michiganroad72")

CheckMatches(m, nameMatch, valuesMatch)
{
	values := ""

	for v in m
	{
		values .= v
	}

	if (values == valuesMatch)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	names := ""
	values := ""

	for n,v in m
	{
		names .= n
		values .= v
	}

	if (names == nameMatch)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	if (values == valuesMatch)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}