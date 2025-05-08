if 1 == 1.0
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if 1 = "1"
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if 1 == "1"
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if 1 != 2.0
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if "0.10" = 0.1
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if 513 = "0x201"
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if 513.0 == "0x201"
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if "a" = "A"
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if "a" == "A"
    FileAppend "fail", "*"
else
    FileAppend "pass", "*"