;#Include %A_ScriptDir%/header.ahk

product := "Prod"
color := "Red"

x := 
(
123
)

If (x = 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
	This line is indented with a tab; by default, that tab will also be stored.
Additionally, "quote marks" are automatically escaped when appropriate.
)"

ProductIsAvailable := ProductIsAvailable := (Color = "Red") ?
	false : ; We don't have any red products, so don't bother calling the function.
	ProductIsAvailableInColor(Product, Color)

If (ProductIsAvailable == false)
	FileAppend, pass, *
else
	FileAppend, fail, *

ProductIsAvailable := (Color = "Green")
	? false  ; We don't have any red products, so don't bother calling the function.
	: ProductIsAvailableInColor(Product, Color)

If (ProductIsAvailable == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Color = "Red" or Color = "Green"  or Color = "Blue"   ; Comment.
	or Color = "Black" or Color = "Gray" or Color = "White"   ; Comment.
	and ProductIsAvailableInColor(Product, Color))   ; Comment.
{
	FileAppend, pass, *
}
else
	FileAppend, fail, *

arr :=  ; The assignment operator causes continuation.
[  ; Brackets enclose the following two lines.
  "item 1",
  "item 2",
]

if (arr[1] = "item 1")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[2] == "item 2")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[3] == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := [
  "item 1",
  "item 2",
]

FileDelete("./multilines.txt")
FileAppend( "
(
Line 1 of the text.
Line 2 of the text. By default, a linefeed (`n) is present between lines.
)", "./multilines.txt")

if (FileExist("./multilines.txt"))
	FileAppend, pass, *
else
	FileAppend, fail, *

teststr := "Line 1 of the text.`nLine 2 of the text. By default, a linefeed (`n) is present between lines."
data2 := FileRead("./multilines.txt")

if (data2 = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

FileDelete("./multilines.txt")

Var := "
(
	A line of text beginning in a tab which should not be removed.
A second line not beginning in a tab.
)"

teststr := "`tA line of text beginning in a tab which should not be removed.`nA second line not beginning in a tab."

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
( LTrim0
	A line of text not ending in a tab.
A second line not ending in a tab.
)"

teststr := "`tA line of text not ending in a tab.`nA second line not ending in a tab."

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
( RTrim0
A line of text ending in a tab.	
A second line ending in a tab.	
)"

teststr := "A line of text ending in a tab.`t`nA second line ending in a tab.`t"

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "   ; Comment.
(
; This is not a comment; it is literal. Include the word Comments in the line above to make it a comment.
)"

teststr := "; This is not a comment; it is literal. Include the word Comments in the line above to make it a comment."

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "   ; Comment.
( Comments
; This is not a comment; it is literal. Include the word Comments in the line above to make it a comment.
)"

teststr := ""

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
( Join|
Line 1
Line 2
Line 3
)"

teststr := "Line 1|Line 2|Line 3"

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
(
`)Escaped closing paren.
)"

teststr := ")Escaped closing paren."

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

Var := "
(comment
this is
; comments
)
(Join
more 
string
)"

teststr := "this is`nmore string"

if (Var = teststr)
	FileAppend, pass, *
else
	FileAppend, fail, *

ProductIsAvailableInColor(a, b)
{
	return 123
}