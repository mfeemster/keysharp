MyGui := Gui()

LV_Label := MyGui.Add("Text", "w400 x10 y10","ListView testing")
LV_Label.SetFont("cBlue s10")
; Create the ListView with two columns, Name and Size:
LV := MyGui.Add("ListView", "r15 w350 x10 y+5", ["Name","Size (KB)"])

Loop Files, A_MyDocuments "\*.*"
  LV.Add(, A_LoopFileName, A_LoopFileSizeKB)

LV_Btn1 := MyGui.Add("Button", "x10 y+10", "Count Columns")
LV_Btn1.OnEvent("Click", LV_CountCol)

MyGui.Show()

LV_CountCol(*) {
    List := ListViewGetContent("Count Col", LV, MyGui)
    MsgBox(List, "Count columns")
    List := ""
}

