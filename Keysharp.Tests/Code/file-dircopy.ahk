if (DirExist("./DirCopy2"))
	DirDelete("./DirCopy2", true)

DirCopy("../../../Keysharp.Tests/Code/DirCopy", "./DirCopy2")
VerifyAndDelete(true)

DirCopy("../../../Keysharp.Tests/Code/DirCopy/DirCopy.zip", "./DirCopy2", true)
VerifyAndDelete(false)

b := false

try
{
    DirCopy("../../../Keysharp.Tests/Code/DirCopy/DirCopy.zip", "./DirCopy2", false)
}
catch
{
    b := true
}

if (b)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

VerifyAndDelete(true)

VerifyAndDelete(del)
{
    if (DirExist("./DirCopy2"))
 	    FileAppend "pass", "*"
    else
  	    FileAppend "fail", "*"

    if (FileExist("./DirCopy2/file1.txt"))
 	    FileAppend "pass", "*"
    else
  	    FileAppend "fail", "*"

    if (FileExist("./DirCopy2/file2.txt"))
 	    FileAppend "pass", "*"
    else
  	    FileAppend "fail", "*"

    if (FileExist("./DirCopy2/file3txt"))
 	    FileAppend "pass", "*"
    else
  	    FileAppend "fail", "*"

    if (del)
    {
        if (DirExist("./DirCopy2"))
	        DirDelete("./DirCopy2", true)

        if (DirExist("./DirCopy2"))
 	        FileAppend "fail", "*"
        else
  	        FileAppend "pass", "*"
    }
}