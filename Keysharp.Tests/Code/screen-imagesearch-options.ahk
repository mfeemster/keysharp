#ErrorStdOut
#Warn All, StdOut
#Include <assert>

for option in ["*Dir", "*Dir0", "*Dir10", "*Dir-1", "*Dir1.5", "*Dir2bad", "*DirTopLeft", "*Dir2 *Dir0"] {
    try {
        ImageSearch(, , 0, 0, 0, 0, option " missing-image.png")
        Assert(false, A_LineNumber)
    } catch ValueError as caught {
        Assert(InStr(caught.Message, "*Dir requires"), A_LineNumber)
    }
}

FileAppend "pass", "*"
