str := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
)"

if (FileExist("./continuation.txt"))
	FileDelete("./continuation.txt")

FileAppend(str, "./continuation.txt")

if (FileExist("./continuation.txt"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

text := FileRead("./continuation.txt")

if (text == "A line of text.`nBy default, the hard carriage return (Enter) between the previous line and this one will be stored.")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (FileExist("./continuation.txt"))
	FileDelete("./continuation.txt")

str := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
	This line is indented with a tab; by default, that tab will also be stored.
Additionally, "quote marks" are automatically escaped when appropriate.
)"

if (FileExist("./continuation.txt"))
	FileDelete("./continuation.txt")

FileAppend(str, "./continuation.txt")

if (FileExist("./continuation.txt"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

text := FileRead("./continuation.txt")

if (text == "A line of text.`nBy default, the hard carriage return (Enter) between the previous line and this one will be stored.`n`tThis line is indented with a tab; by default, that tab will also be stored.`nAdditionally, `"quote marks`" are automatically escaped when appropriate.")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (FileExist("./continuation.txt"))
	FileDelete("./continuation.txt")