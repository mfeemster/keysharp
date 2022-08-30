GuiBGColor := "BackgroundFF9A9A"
;BGColor2 := "0xFFFFAA"

; ┌───────────┐
; │  Globals  │
; └───────────┘

global gb3Hwnd
winpos := ""

; ┌────────────────┐
; │  Tab One Menu  │
; └────────────────┘

FileMenu := Menu()
FileMenu.Add("System", "MenuHandler")
FileMenu.Add("Script Icon", "MenuHandler")
FileMenu.Add("Suspend Icon", "MenuHandler")
FileMenu.Add("Pause Icon", "MenuHandler")
FileMenu.SetIcon("System", "Shell32.dll", 174) ; 2nd icon group from the file
FileMenu.SetIcon("Script Icon", A_KeysharpCorePath, "Keysharp.ico")
FileMenu.SetIcon("Suspend Icon", A_KeysharpCorePath, "Keysharp_s.ico")
FileMenu.SetIcon("Pause Icon", A_KeysharpCorePath, "Keysharp_p.ico")

ImgSrchMenu := Menu()
ImgSrchMenu.Add("Image Search Test", "ImgSrch")


MyMenuBar := MenuBar()
MyMenuBar.Add("&Menu Icon Test", FileMenu)
MyMenuBar.Add("Image Search", ImgSrchMenu)

MyGui := Gui(, "KEYSHARP TESTS")

; ┌───────────────────┐
; │  Add Menu to GUI  │
; └───────────────────┘

MyGui.MenuBar := MyMenuBar

; ┌─────────────┐
; │  Start TAB  │
; └─────────────┘

Tab := MyGui.Add("Tab3", , ["First","Second","Third", "GroupBoxes", "ControlZoo"])

Tab.UseTab("First")

; ┌──────────────────────┐
; │  Create the window:  │
; └──────────────────────┘

MyGui.SetFont("cBlack s8", "Arial")
TEST_HEADER := MyGui.Add("Text", "s20 w1200","Keysharp GUI Tests")

; ┌────────────────────────────────────┐
; │  Add button to change header font  │
; └────────────────────────────────────┘
headerBtn := MyGui.Add("Button", "s8 x10 y+10", "Make header font larger Comic Sans MS")
headerBtn.OnEvent("Click", "ChangeFont")
; ┌──────────────────────────────┐
; │  Add button to restore font  │
; └──────────────────────────────┘
headerBtn2 := MyGui.Add("Button", "s8 x+10 yp", "Restore header font")
headerBtn2.OnEvent("Click", "ChangeFontBack")
; ┌───────────────────────────────────┐
; │  Add button to change background  │
; └───────────────────────────────────┘
bgBtn := MyGui.Add("Button", "s8 x+10 yp", "Change GUI Backgroud")
bgBtn.OnEvent("Click", "ChangeBG")
; ┌────────────────────────────────────┐
; │  Add button to restore background  │
; └────────────────────────────────────┘
bgBtn2 := MyGui.Add("Button", "s8 x+10 yp", "Restore GUI Backgroud")
bgBtn2.OnEvent("Click", "RestoreBG")
; ┌─────────────────┐
; │  GroupBox test  │
; └─────────────────┘

gb1_TabOne := MyGui.Add("GroupBox", "x10 y+10 w325 h800", "Tab One - Group One") ; 

; ┌──────────────────────────────────┐
; │  Listview testing                │
; │  Double-click activates tooltip  │
; └──────────────────────────────────┘
LV_Label := MyGui.Add("Text", "w400 x10 y+10","Create listview with tooltip - double-click row")
LV_Label.SetFont("cBlue s10")
; Create the ListView with two columns, Name and Size:
LV := MyGui.Add("ListView", "r15 w300 x10 y+5 BackgroundTeal", ["Name","Size (KB)"])

; ┌────────────────────────────────────────────────────────────┐
; │  Notify the script whenever the user double clicks a row:  │
; └────────────────────────────────────────────────────────────┘
LV.OnEvent("DoubleClick", "LV_DoubleClick")

; ┌─────────────────────────────────────────────────────────────────────────────┐
; │  Gather a list of file names from a folder and put them into the ListView:  │
; └─────────────────────────────────────────────────────────────────────────────┘
Loop Files, A_MyDocuments "\*.*"
  LV.Add(, A_LoopFileName, A_LoopFileSizeKB)
; ┌─────────────────────────────────────────────┐
; │  Show an input box and retrieve the result  │
; └─────────────────────────────────────────────┘
InputBtn := MyGui.Add("Button", "s8 x10 y+10", "Input Test")
InputBtn.OnEvent("Click", "InputTest")

;LV.ModifyCol()  ; Auto-size each column to fit its contents.
LV.ModifyCol(2, "Integer")  ; For sorting purposes, indicate that column 2 is an integer.
; ┌─────────────────────┐
; │  Add a radio group  │
; └─────────────────────┘

RadioText := MyGui.Add("Text", "w200", "Radio group tests")
RadioText.SetFont("cBlue s10")
RadioOne := MyGui.Add("Radio", "vMyRadioGroup", "Change header font (alternate).")
RadioOne.OnEvent("Click", "ChangeFont")
RadioTwo := MyGui.Add("Radio", "vMyRadioGroup", "Restore header font (alternate)")
RadioTwo.OnEvent("Click", "ChangeFontBack")
RadioThree := MyGui.Add("Radio", "vMyRadioGroup", "Please click me.")
RadioThree.OnEvent("Click", "RadioThreeClicked")

; ┌──────────────────┐
; │  Add checkboxes  │
; └──────────────────┘

CheckBoxText := MyGui.Add("Text", "w200", "Checkbox test")
CheckBoxText.SetFont("cBlue s10")
CheckBoxOne := MyGui.Add("CheckBox", "w200 x10 yp+20", "If this text is long, it will wrap automatically")
CheckBoxOne.OnEvent("Click", "CheckBoxOneClicked")

; ┌────────────────────────────────┐
; │  Notify User about Popup Menu  │
; └────────────────────────────────┘

Menu_Label := MyGui.Add("Text", "w400 x10 y+10","Press Win-Z to see popup menu")
Menu_Label.SetFont("cBlue s14")

MyGui.UseGroup()
Tab.UseTab("First")
gb2_TabOne := MyGui.Add("GroupBox", "x350 yp w325 h800", "Tab One - Group Two") ; 

; ┌───────────────────────────────┐
; │  Tab One, Group Two controls  │
; └───────────────────────────────┘

g2Label1 := MyGui.Add("Text", "w200 cBlue S10", "Click buttons to set and reset style")
g2Label2 := MyGui.Add("Text", "x10", "Keep an eye on the title bar!")

g2Btn1 := MyGui.Add("Button", "x10 y+10", "Set")
g2Btn1.SetFont("s10 cBlue")
g2Btn2 := MyGui.Add("Button", "x100 yp", "Reset")
g2Btn2.SetFont("s10 cBlue")

g2Btn1.OnEvent("Click", "Set_Style")
g2Btn2.OnEvent("Click", "Reset_Style")

g2Label3 := MyGui.Add("Text", "x10 w200 cBlue S10", "Click buttons to alter Edit style")
g2Label4 := MyGui.Add("Text", "x10", "Uppercase - restrict or reset")

MyEdit2 := MyGui.Add("Edit", "x10 w300 h100")
HwndMyEdit := MyEdit2.Hwnd


g2Btn3 := MyGui.Add("Button", "x10 y+10", "Uppercase")
g2Btn3.SetFont("s8 cBlue")
g2Btn4 := MyGui.Add("Button", "x100 yp", "Unrestrict")
g2Btn4.SetFont("s8 cBlue")

g2Btn3.OnEvent("Click", "Set_Edit_Style")
g2Btn4.OnEvent("Click", "Reset_Edit_Style")

iniLabel := MyGui.Add("Text", "xp y+5 cRed", "Click to read kstests.ini`nKey = PRIMATE2`nValue = BONOBO")
iniBtn1 := MyGui.Add("Button", "x220 yp", "Read INI")
iniBtn1.OnEvent("Click", "ReadINI")
iniText := MyGui.Add("Text", "w100 x150 yp", "")
iniEdit := MyGui.Add("Edit", "x10 y+10 w300 h180")

ReadINI() {
    Val := IniRead(".\kstests.ini", "section2", "PRIMATE2")
    iniText.SetFont("s10 cBlue")
    IniFileText := FileRead(".\kstests.ini")
    ControlSetText(Val, iniText)
    ControlSetText(IniFileText, iniEdit)
}

iniWriteBtn := MyGui.Add("Button", "x10 y+10", "Write INI")
iniWriteBtn.OnEvent("Click", "WriteINI")
writeLabel := MyGui.Add("Text", "x100 yp cGreen", "Write and Re-Write`nChange case`nThen change back")


iniWriteEdit := MyGui.Add("Edit", "x10 y+10 w300 h180")

WriteINI() {
    IniWrite("BonoboBozo has been captured", ".\kstests.ini", "SECTION42", "PRIMATEZ_ON_LOOSE")
    IniFileText2 := FileRead(".\kstests.ini")
    ControlSetText(IniFileText2, iniWriteEdit)
    Sleep(2000)
    IniWrite("BONOBOBOZO has escaped", ".\kstests.ini", "SECTION42", "PRIMATEZ_ON_LOOSE")
    IniFileText2 := FileRead(".\kstests.ini")
    ControlSetText(IniFileText2, iniWriteEdit)
}


;;;;;;;;;
; ┌──────────────────────┐
; │  Second Tab section  │
; └──────────────────────┘
Tab.UseTab("Second")

; ┌─────────────────────────────┐
; │  Add group boxes - 8/23/22  │
; └─────────────────────────────┘
gb1_TabTwo := MyGui.Add("GroupBox", "x10 y10 w325 h800", "Tab Two - Group One") ; 
;gb2_TabTwo := MyGui.Add("GroupBox", "x350 yp w325 h800", "Tab Two - Group Two")
;MyGui.UseGroup()
MyGui.UseGroup(gb1_TabTwo) 
; ┌────────┐
; │  Edit  │
; └────────┘
SecondEdit := MyGui.Add("Edit", "w300 h200")
SecondEditText := MyGui.Add("Text", "cBlue s10 w200", "ControlSetText Test")
HwndSecondEdit := SecondEdit.Hwnd
EditBtn1 := MyGui.Add("Button", "s8 xp y+10", "Text -> Edit")
EditBtn1.OnEvent("Click", "SendTextToEdit")
EditBtn2 := MyGui.Add("Button", "s8 x80 yp", "Clear Edit")
EditBtn2.OnEvent("Click", "ClearEdit")
EditHwndBtn := MyGui.Add("Button", "s8 x160 yp", "Show Edit Hwnd")
EditHwndBtn.OnEvent("Click", "ShowEditHwnd")
; ┌────────────┐
; │  RichEdit  │
; └────────────┘
SecondRichEdit := MyGui.Add("RichEdit", "x10 w250 h150", "Try pasting rich text and/or images here!")
SecondRichEditText := MyGui.Add("Text", "cBlue s10 w200", "ControlSetText Test (RichEdit)")
RichEditBtn1 := MyGui.Add("Button", "s8 x10 y+10", "Send Text to RichEdit")
RichEditBtn1.OnEvent("Click", "SendTextToRichEdit")
RichEditBtn2 := MyGui.Add("Button", "s8 x150 yp", "Clear RichEdit")
RichEditBtn2.OnEvent("Click", "ClearRichEdit")

; ┌────────────┐
; │  TreeView  │
; └────────────┘
TreeViewText := MyGui.Add("Text", "x10 cBlue s10 w200", "TreeView Test")
TV := MyGui.Add("TreeView", "xp w200 y+5 -ReadOnly") ; Need to work on -ReadOnly
TV.OnEvent("ItemEdit", "MyTreeView_Edit")
P1 := TV.Add("First parent")
P1C1 := TV.Add("Parent 1's first child", P1)  ; Specify P1 to be this item's parent.
P2 := TV.Add("Second parent")
P2C1 := TV.Add("Parent 2's first child", P2)
P2C2 := TV.Add("Parent 2's second child", P2)
P2C2C1 := TV.Add("Child 2's first child", P2C2)

; ┌──────────────────────────┐
; │  Text to show Mouse Pos  │
; └──────────────────────────┘
MousePosText := MyGui.Add("Text", "x10 y+10 cBlue s10 w200", "Uses SetTimer to show mouse position")
CoordText := MyGui.Add("Text", "x10 y+10 cLime", "")
;CoordText.SetFont("s20") 
SetTimer("UpdateOSD", 200)
UpdateOSD()  ; Make the first update immediate rather than waiting for the timer.

; ┌──────────────────────────────┐
; │  Insert Image for searching  │
; └──────────────────────────────┘

SrchPic := MyGui.Add("Picture", "x10 y+10 h-1", A_ScriptDir "\killbill.png")
SrchPicText := MyGui.Add("Text", "x10 y+15 w200", "^ Use top menu to find me!")
SrchPicText.SetFont("s10 cBlue")
; ┌────────────────────────┐
; │  End of Tab 2 Group 1  │
; └────────────────────────┘
MyGui.UseGroup()
Tab.UseTab("Second")
gb2_TabTwo := MyGui.Add("GroupBox", "x350 y10 w325 h550", "Tab Two - Group Two")
MyGui.UseGroup(gb2_TabTwo)

; ┌─────────┐
; │  Edits  │
; └─────────┘
t2g2t1 := MyGui.Add("Text", "xp y+10 w200 cBlue", "Password entry")
t2g2t1.SetFont("s10")
e1 := MyGui.Add("Edit", "w200 xp y+10 +0x20")
t2g2t2 := MyGui.Add("Text", "xp y+10 w250 cBlue s10", "Alternate password entry (*)")
t2g2t2.SetFont("s10")
e2 := MyGui.Add("Edit", "w200 xp y+10 Password*")
t2g2t3 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - ControlSetStyle")
t2g2t3.SetFont("s10")
e3 := MyGui.Add("Edit", "w200 xp y+10 h50", "Edit 3")
MyGui.UseGroup(gb2_TabTwo)
ControlSetStyle("+0x8", e3)
t2g2t4 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - +0x8")
t2g2t4.SetFont("s10")
e4 := MyGui.Add("Edit", "w200 xp y+10 h50 +0x8", "Edit 4")
e3Btn := MyGui.Add("Button", "xp y+10", "Toggle ControlSetStyle Edit")
e3Btn.OnEvent("Click", "ShowE3Hwnd")

ShowE3Hwnd() {
    ControlSetStyle("^0x8", e3)
    ControlFocus(e3.Hwnd)
}

; ┌────────────┐
; │  Move GUI  │
; └────────────┘

MoveButton := MyGui.Add("Button", , "Move GUI")
MoveButton.OnEvent("Focus", "ChangeMoveBtnColor")
MoveButton.OnEvent("Click", "MoveGui")
MoveButtonBack := MyGui.Add("Button", "x120 yp", "Move GUI Back")
MoveButtonBack.OnEvent("Focus", "ChangeMoveBtnBackColor")
MoveButtonBack.OnEvent("Click", "MoveGuiBack")

ChangeMoveBtnColor() {
    MoveButton.SetFont("cRed")
    MoveButtonBack.SetFont("cBlack")
}

ChangeMoveBtnBackColor() {
    MoveButton.SetFont("cBlack")
    MoveButtonBack.SetFont("cRed")
}

; ┌───────────────────────┐
; │  SendMessage Section  │
; └───────────────────────┘

TitleInfo := MyGui.Add("Text", "x10 y+10", "Buttons below will alter GUI title with SendMessage")
TitleInfo.SetFont("cBlue s8")
SendBtn1 := MyGui.Add("Button", "x10 y+10", "Change Title")
SendBtn1.OnEvent("Click", "ChangeTitle")
SendBtn2 := MyGui.Add("Button", "x120 yp", "Restore Title")
SendBtn2.OnEvent("Click", "RestoreTitle")


ChangeTitle() {
    Title := "KEYSHARP'S BRAND SPANKING NEW TITLE"
    SendMessage(0x000C, 0, Title)  ; 0X000C is WM_SETTEXT
}

RestoreTitle() {
    Title := "KEYSHARP TESTS"
    SendMessage(0x000C, 0, Title)  ; 0X000C is WM_SETTEXT
}


; ┌───────────────────────┐
; │  PostMessage Section  │
; └───────────────────────┘
/*
PostInfo := MyGui.Add("Text", "x10 y+10", "Run Notepad - Use PostMessage to show 'About'")
PostInfo.SetFont("cBlue s8")
PostBtn1 := MyGui.Add("Button", "x10 y+10", "Show Notepad 'About'")
PostBtn1.OnEvent("Click", "AboutNotepad")

AboutNotepad() {
    SetTitleMatchMode(2)
    Run("Notepad.exe")
    Sleep(1000)
    PostMessage(0x0111, 65, 0, , "Untitled - Notepad")
    Sleep(2000)
    WinKill("ahk_exe Notepad.exe")
}
*/

MyGui.UseGroup()
Tab.UseTab("Second")

; ┌───────────────┐
; │  Add Picture  │
; └───────────────┘
MyPictureBtn := MyGui.Add("Button", "cBlue s10 x400 y600", "Display a Picture")
MyPictureBtn.OnEvent("Click", "LoadPic")
;MyPic := MyGui.Add("Picture", "xp y+20 w100 h-1", A_ScriptDir "\monkey.ico")
SlugLine := MyGui.Add("Text", "cBlue s10 w200 xp y800", "Picture will display above")



;;;;;;;;;;
; ┌─────────────────────┐
; │  Third Tab section  │
; └─────────────────────┘

Tab.UseTab("Third")
; ┌──────────────────┐
; │  Add a groupbox  │
; └──────────────────┘

gb1_TabThree := MyGui.Add("GroupBox", "x10 y10 w325 h875", "Tab Three - Group One")

    ;Placeholder ThirdText1
ThirdText1 := MyGui.Add("Text", "cBlue s10", "ListBox Test")
; ┌────────────────┐
; │  ListBox test  │
; └────────────────┘
MyListBox := MyGui.Add("ListBox", "r5 w110", ["Red","Green","Blue","Black","White"])
MyListBox.OnEvent("Change", "ListBoxClicked")
MyLbBtn1 := MyGui.Add("Button", "x+10 yp", "Delete White")
MyLbBtn1.OnEvent("Click", "DeleteWhite")
MyLbBtn2 := MyGui.Add("Button", "x+10 yp", "Add White")
MyLbBtn2.OnEvent("Click", "AddWhite")

DeleteWhite() {
    ControlDeleteItem(5, MyListBox)
}

AddWhite() {
    ControlAddItem("White", MyListBox)
}

; ┌────────────────┐
; │  Multi-select  │
; └────────────────┘
ThirdText2 := MyGui.Add("Text", "x10 y+10 cBlue s10", "ListBox Test (Multi-Select)")
MyMultiLB := MyGui.Add("ListBox", "+Multi r5 w110 x10 y+10", ["Reactionary Red","Garish Green","Beastly Blue","Banal Black","Washed-out White"])
MyMultiLB.OnEvent("Change", "MultiLBClicked")
; ┌─────────────┐
; │  Drop-Down  │
; └─────────────┘
ThirdText3 := MyGui.Add("Text", "x10 y+10 cBlue s10", "Drop-down List")
MyDDL := MyGui.Add("DropDownList", "x10 y+10", ["Orange","Purple","Fuchsia","Lime","Aqua"])
MyDDL.OnEvent("Change", "DDLClicked")
; ┌─────────────┐
; │  Combo Box  │
; └─────────────┘
ThirdText4 := MyGui.Add("Text", "x10 cBlue s10", "ComboBox")
MyCB := MyGui.Add("ComboBox", "x10 y+10 r5 Limit", ["Orange","Purple","Fuchsia","Lime","Aqua"])
CB_Button := MyGui.Add("Button", "s8 x10 y+10", "Show CB Selection")
CB_AddBtn := MyGui.Add("Button", "s8 x140 yp", "Add 'Yellow' to CB")
CB_Button.OnEvent("Click", "CB_ButtonClicked")
CB_AddBtn.OnEvent("Click", "AddYellow")

AddYellow() {
    ControlAddItem("Yellow", MyCB)
}


; ┌──────────┐
; │  Slider  │
; └──────────┘

ThirdText5 := MyGui.Add("Text", "x10 cBlue s10", "Moving slider shows position below")
MySlider := MyGui.Add("Slider", "x10 y+10 +AltSubmit TickInterval10 Page10", 100)
MySlider.OnEvent("Change", "SliderPos")
MySliderPos := MyGui.Add("Text", "x10 y+5","")

; ┌───────────────────┐
; │  Slider Callback  │
; └───────────────────┘

SliderPos() {
    ControlSetText("Slider value is " MySlider.Value, MySliderPos.Hwnd)
}

; ┌────────────────┐
; │  Progress Bar  │
; └────────────────┘

ThirdText6 := MyGui.Add("Text", "x10 cBlue s10", "Progress bar - click buttons to move")
MyProgress := MyGui.Add("Progress", "x10 y+10", 50)
Pbtn1 := MyGui.Add("Button", "s8 x10 y+5", "Lower")
Pbtn2 := MyGui.Add("Button", "s8 x100 yp", "Higher")
Pbtn1.OnEvent("Click", "Pbtn1Clicked")
Pbtn2.OnEvent("Click", "Pbtn2Clicked")
; ┌──────────────┐
; │  Status Bar  │
; └──────────────┘
MySB := MyGui.Add("StatusBar",, "                       ")
; ┌─────────────┐
; │  Date Time  │
; └─────────────┘
ThirdText7 := MyGui.Add("Text", "x10 y+5 cBlue s10", "DateTime Test")
MyDateTime := MyGui.Add("DateTime", "s8 x10 y+5 w200", "LongDate")
; ┌────────────┐
; │  MonthCal  │
; └────────────┘
ThirdText8 := MyGui.Add("Text", "x10 y+5 cBlue s10", "MonthCal Test")
MyMonthCal := MyGui.Add("MonthCal")
MC_Btn := MyGui.Add("Button", "s8 x10 y+5", "Change Cal Colors (not implemented)")
MC_Btn.OnEvent("Click", "MC_Colors")

; ┌───────────────────────────┐
; │  End Tab Three Group One  │
; └───────────────────────────┘

MyGui.UseGroup()
Tab.UseTab("Third")
gb2_TabThree := MyGui.Add("GroupBox", "x350 y10 w325 h875", "Tab Three - Group Two")

; ┌────────────────┐
; │  Sliding text  │
; └────────────────┘

InfoText3 := MyGui.Add("Text", "x10 y+10 w200", "Sliding text. Move Slider.")
InfoText3.SetFont("cBlue s8")
MyText := MyGui.Add("Text", "x10 y+10 w340 h30")
MyText.SetFont("cTeal Consolas Bold")
HwndMyText := MyText.Hwnd

MySlider2 := MyGui.Add("Slider", "Range0-80 +AltSubmit TickInterval10 Page10 ToolTip", 10)
MySlider2.Value := 10
mybtn := MyGui.Add("Button", "w100 s8 cBlue", "Sliding Test")
mybtn.OnEvent("Click", "STest")
FakeSep := MyGui.Add("Text", "x10 y+10", "__________________________________________________")
FakeSep.SetFont("cTeal Bold")


STest() {
    Loop(MySlider2.Value) {
        padding := A_Index
        s := Format("| {1,-" padding "} |`r`n| {2," padding "} |`r`n", "Left  ", "Right")
        ControlSetText(s, HwndMyText)
        Sleep(5) ; Need time to update Text

    }
        Loop(MySlider2.Value) {
        padding := MySlider2.Value-A_Index
        s := Format("| {1,-" padding "} |`r`n| {2," padding "} |`r`n", "Left  ", "Right")
        ControlSetText(s, HwndMyText)
        Sleep(5)
    }
}

MyGui.Add("Text", "x+5 y+5", "_____________________________")

; ┌────────────────────────┐
; │  Groupbox Tab Section  │
; └────────────────────────┘

Tab.UseTab("GroupBoxes")

; ┌────────────────────────────────────────────────────┐
; │  Note that you must use 'Tab.UseTab("GroupBoxes")  │
; │  after a call to MyGui.UseGroup()                  │
; │  if you use GroupBoxes with Tabs.                  │
; └────────────────────────────────────────────────────┘

gb1 := MyGui.Add("GroupBox", "x10 y10 w330 h400", "Group One")
MyGui.UseGroup(gb1)
MyGui.Add("Text", , "Testing gb1")
MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
gb2 := MyGui.Add("GroupBox", "x10 y+10 w330 h400", "Group Two")
MyGui.UseGroup(gb2)
MyGui.Add("Button", "cBlue s8", "Testing gb2")
MyGui.Add("Text", , "Testing placement")
MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
MyGui.AddText("s14 cBlue", "This should be below.")
gb3 := MyGui.Add("GroupBox", "x+10 y10 w330 h400", "Group Three")
MyGui.UseGroup(gb3)
MyGui.Add("Text", "xp w400", "Testing gb3")
gb3Edit := MyGui.Add("Edit", "w300 h300")
gb3Hwnd := gb3Edit.Hwnd
gb3Edit.OnEvent("Focus", "StartEditTooltip")
gb3Edit.OnEvent("LoseFocus", "StopToolTip")
;gb3Btn1 := MyGui.Add("Button", "s14 cLime", "Send to GB3")
;gb3Btn1.OnEvent("Click", "SendToGB3")
MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
gb4 := MyGui.Add("GroupBox", "xp y420 w330 h400", "Group Four")
MyGui.UseGroup(gb4)
MyGui.Add("Text", "xp", "Testing gb4")
gb4Btn1 := MyGui.Add("Button", "s14 cLime", "Send to GB3")
gb4Btn1.OnEvent("Click", "SendToGB3")
gb4Btn2 := MyGui.Add("Button", "s14 cLime x+5 yp", "Clear GB3")
gb4Btn2.OnEvent("Click", "ClearGB3")
MyGui.UseGroup()

MyGui.Opt("+Autosize")
MyGui.Show()

; ┌────────────────┐
; │  MENU SECTION  │
; └────────────────┘

MyMenu := Menu()
MyMenu.Add("Item 1", "MenuHandler")
MyMenu.Add("Item 2", "MenuHandler")
MyMenu.Add()  ; Add a separator line.

; Create another menu destined to become a submenu of the above menu.
Submenu1 := Menu()
Submenu1.Add("Item A", "MenuHandler")
Submenu1.Add("Item B", "MenuHandler")

; Create a submenu in the first menu (a right-arrow indicator). When the user selects it, the second menu is displayed.
MyMenu.Add("My Submenu", Submenu1)

MyMenu.Add()  ; Add a separator line below the submenu.
MyMenu.Add("Item 3", "MenuHandler")  ; Add another menu item beneath the submenu.

MenuHandler(Item, *) {
    MsgBox("You selected " Item, "ITEM SELECTED")
}


#z::MyMenu.Show()  ; i.e. press the Win-Z hotkey to show the menu.
;#z::Run("Notepad.exe")

; ┌──────────────────┐
; │  ControlZoo Tab  │
; └──────────────────┘

MyGui.UseGroup()
Tab.UseTab("ControlZoo")
gb1_CZ := MyGui.Add("GroupBox", "x10 y10 w325 h875", "ControlZoo - Group One")

CZ_Text1 := MyGui.Add("Text", , "Control Functions testing")
CZ_Text1.SetFont("s10 CBlue")
CZ_Text2 := MyGui.Add("Text", "x10 y+10 w300", "For the controls on this tab, we'll add, delete, click, focus and perform other control functions.")
CZ_Text2.SetFont("CTeal")

CZ_ListBox := MyGui.Add("ListBox", "x10 h500 w160", ["Red","Green","Blue","Black","White", "Maroon"
    , "Purple", "Color de gos com fuig", "Weiß", "Amarillo", "красный"
    , "朱红"])

CZ_LbBtn1 := MyGui.Add("Button", "x180 w120 yp", "Add fuchsia")
CZ_LbBtn1.OnEvent("Click", "AddFuchsia")
CZ_LbBtn2 := MyGui.Add("Button", "x180 w120 yp+40", "Delete Fuchsia")
CZ_LbBtn2.OnEvent("Click", "DeleteFuchsia")
CZ_LbBtn2.OnEvent("Focus", "FuchsiaDeleteTrayTip")
CZ_LbBtn3 := MyGui.Add("Button", "x180 w120 yp+80", "Purple (Index)")
CZ_LbBtn3.OnEvent("Click", "ChooseIndex")
CZ_LbBtn4 := MyGui.Add("Button", "x180 w120 yp+120", "красный (String)")
CZ_LbBtn4.OnEvent("Click", "ChooseString")
CZ_LbBtn5 := MyGui.Add("Button", "x180 w120 yp+160", "ControlGetChoice")
CZ_LbBtn5.OnEvent("Click", "GetChoice")
CZ_LbBtn6 := MyGui.Add("Button", "x180 w120 yp+200", "ControlGetClassNN")
CZ_LbBtn6.OnEvent("Click", "GetClassNN")

CZ_LbBtn7 := MyGui.Add("Button", "w120 x180 yp+240", "ControlGetEnabled")
CZ_LbBtn7.OnEvent("Click", "GetEnabled")
CZ_LbBtn8 := MyGui.Add("Button", "w120 x180 yp+280", "Disabled!")
CZ_LbBtn8.Enabled := False


; ┌────────────────────────┐
; │  ControlZoo Functions  │
; └────────────────────────┘

AddFuchsia() {
    ControlAddItem("Fuchsia", CZ_ListBox)
}

DeleteFuchsia() {
    Try 
    {
        FuchsiaIndex := ControlFindItem("Fuchsia", CZ_ListBox)
    }
    Catch as e  ; Handles the first error thrown by the block above.
    {
        MsgBox("An error was thrown!`nSpecifically: " e.Message, "ERROR!")
        Return
    }
    
    ;MsgBox(FuchsiaIndex)
    ControlDeleteItem(FuchsiaIndex, CZ_ListBox)
}

FuchsiaDeleteTrayTip() {
    TrayTip("Also tests ControlFindItem")
}

ChooseIndex() {
    ControlChooseIndex(7, CZ_ListBox)
}

ChooseString() {
    ControlChooseString("красный", CZ_ListBox)
}

GetChoice() {
    Try
    {
    Choice := ControlGetChoice(CZ_ListBox, MyGui)
    MsgBox(Choice, "Choice")
    }
        Catch as e  ; Handles the first error thrown by the block above.
    {
        ;MsgBox("An error was thrown!`nSpecifically: " e.Message, "ERROR!")
        MsgBox("You must select an item first.", "ERROR!")
        Return
    }
}

GetClassNN() {
    ClassNN := ControlGetClassNN(CZ_ListBox, MyGui)
    MsgBox(ClassNN, "ClassNN")
}

GetEnabled() {
    Result := ControlGetEnabled(CZ_LbBtn8, MyGui)
    MsgBox(Result, "'Disabled' Button State (1: enabled 0: disabled)")
    Result2 := ControlGetEnabled(CZ_LbBtn6, MyGui)
    MsgBox(Result2, "ClassNN Button State (1: enabled 0: disabled)")
}

; ┌───────────────────────────┐
; │  FUNCTIONS AND CALLBACKS  │
; └───────────────────────────┘

LV_DoubleClick(LV, RowNumber)
{
    RowText := LV.GetText(RowNumber, 1)  ; Get the text from the row's first field.
    ColumnText := LV.GetText(RowNumber, 2)
    ToolTip("You double-clicked row number " RowNumber ". File '" RowText "' has size " ColumnText "kb.")
}
; ┌──────────────────────┐
; │  Change header font  │
; └──────────────────────┘
ChangeFont()
{
global TEST_HEADER
TEST_HEADER.SetFont("cBlue s14", "Comic Sans MS")
}
; ┌────────────────┐
; │  Restore font  │
; └────────────────┘
ChangeFontBack()
{
TEST_HEADER.SetFont("cBlack s8", "Arial")
MsgBox("Done", "Restoring Font")
}
; ┌───────────────────────────┐
; │  Change background color  │
; └───────────────────────────┘
ChangeBG()
{
MsgBox(MyGui.BackColor, "Background color:")
MyGui.BackColor := GuiBGColor
}
; ┌───────────────────────────────┐
; │  Restore background function  │
; └───────────────────────────────┘
RestoreBG()
{
global MyGui
MyGui.BackColor := 0xF0F0F0
}

; ┌───────────────────────┐
; │  Input test function  │
; └───────────────────────┘
InputTest() {
    OutputVar := InputBox("What is your first name?", "Question 1").Value
    if (OutputVar = "Bill")
        MsgBox("That's an awesome name, " OutputVar ".", "What a great name ...")

    OutputVar2 := InputBox("Do you like AutoHotkey?", "Question 2").Value
    if (OutputVar2 = "yes")
        MsgBox("Thank you for answering " OutputVar2 ", " OutputVar "! We will become great friends.", "You are in good company")
    else
        MsgBox(OutputVar ", That makes me sad.", "Sorry to hear it")
}

; ┌───────────────────────┐
; │  RadioThree callback  │
; └───────────────────────┘

RadioThreeClicked() {
MsgBox("You clicked the last radio button.", "Radio 3 Clicked")
}

; ┌─────────────────────┐
; │  Checkbox callback  │
; └─────────────────────┘

CheckBoxOneClicked() {
IsChecked := ControlGetChecked(CheckBoxOne, "KEYSHARP TESTS")
MsgBox("1 is checked - 0 is unchecked`nTests 'ControlGetChecked' also`n`nValue is: " IsChecked, "Checkbox Test")
TrayTip("TrayTipTest", "I will see myself out, thanks!")
Sleep(1000)
;HideTrayTip()
}

; ┌─────────────────┐
; │  TreeView Edit  │
; └─────────────────┘
MyTreeView_Edit(TV, Item) {
    ;MsgBox("Sort Not Implemented", "Men at Work")
    TV.Modify(TV.GetParent(Item), "Sort")  ; This works even if the item has no parent.
    ;return
}


; ┌───────────────────────────────────────────────────────────────────────────┐
; │  https://www.autohotkey.com/board/topic/69784-different-tab-backgrounds/  │
; └───────────────────────────────────────────────────────────────────────────┘

; ┌────────────────┐
; │  Hide TrayTip  │
; └────────────────┘

; Copy this function into your script to use it.
HideTrayTip() {  
    TrayTip()  ; Attempt to hide it the normal way.
    if SubStr(A_OSVersion,1,3) = "10." {
        A_IconHidden := True
        Sleep(200) ; It may be necessary to adjust this sleep.
        A_IconHidden := False
    }
}


; ┌──────────────────────────────┐
; │  Send Text to Edit Callback  │
; └──────────────────────────────┘

SendTextToEdit() {
EditVar := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
	This line is indented with a tab; by default, that tab will also be stored.
"Quote marks" are now automatically escaped when appropriate - not yet implemented.
)"
    ;MsgBox(EditVar)
    ControlSetText(EditVar, SecondEdit)
}
; ┌───────────────────────┐
; │  Clear Edit Callback  │
; └───────────────────────┘
ClearEdit() {
    ControlSetText(, SecondEdit)
}

; ┌──────────────────────┐
; │  RichEdit Callbacks  │
; └──────────────────────┘

SendTextToRichEdit() {
RichEditVar := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
	This line is indented with a tab; by default, that tab will also be stored.
"Quote marks" are now automatically escaped when appropriate.
)"
    ;MsgBox(EditVar)
    ControlSetText(RichEditVar, SecondRichEdit)
}
; ┌───────────────────────┐
; │  Clear Edit Callback  │
; └───────────────────────┘
ClearRichEdit() {
    ControlSetText(, SecondRichEdit)
}

; ┌───────────┐
; │  LoadPic  │
; └───────────┘


LoadPic() {
    Tab.UseTab("Second")
    MyPic := MyGui.Add("Picture", "x400 y650 w100 h-1", A_ScriptDir "\monkey.ico")
    ; MyGui.Opts("+Redraw")
}

; ┌────────────────────┐
; │  Listbox Callback  │
; └────────────────────┘

ListBoxClicked() {
    MsgBox(MyListBox.Text, "ListBox")
    ;MySB.SetIcon("Shell32.dll", 2)
    MySB.SetIcon(A_KeysharpCorePath, "Keysharp.ico")
    MySB.SetText(MyListBox.Text . " selected in ListBox")
}
; ┌─────────────────────┐
; │  Multi LB Callback  │
; └─────────────────────┘
MultiLBClicked() {
    For Index, Field in MyMultiLB.Text
        {
            MsgBox("Selection number " Index " is " Field, "Multi ListBox")
        }
}
; ┌─────────────────────┐
; │  DropDown Callback  │
; └─────────────────────┘
DDLClicked() {
    MsgBox(MyDDL.Text, "Drop Down List")
}
; ┌─────────────────────┐
; │  ComboBox Callback  │
; └─────────────────────┘
CB_ButtonClicked() {
    MsgBox(MyCB.Text, "CB Selection")
}

; ┌─────────────────────────────┐
; │  Progress Button Callbacks  │
; └─────────────────────────────┘

Pbtn1Clicked() {
    ;MsgBox(MyProgress.Value)
    MyProgress.Value -= 10
}

Pbtn2Clicked() {
    MyProgress.Value += 10
}

MC_Colors() {
    MsgBox("Not implemented.", "Future feature")
}

; ┌─────────────────────┐
; │  Test GuiCtrl.Hwnd  │
; └─────────────────────┘

ShowEditHwnd() {
    MsgBox(HwndSecondEdit, "Test 'GuiCtrl.Hwnd'")
}

; ┌──────────────┐
; │  Update OSD  │
; └──────────────┘


; Not used, as SetTimer is causing crashes.
UpdateOSD()
{
    Result := MouseGetPos()
    CoordText.SetFont("bold s20")
    CoordText.Text := ("X: " Result["X"] " Y: " Result["Y"])
}

; ┌────────────────────────────┐
; │  GroupBox Tab - Functions  │
; └────────────────────────────┘


SendToGB3() {
GB3Text := "
(
This uses 'ControlSetText' from a button in GroupBox 4 to populate this edit.
The first message box shows the HWND of this Edit.
The second message box shows '1' (True) if GuiCtrlFromHwnd created an
object from the Hwnd.
Finally, ControlSetText operates on the Object created from the Hwnd.
)"
    MsgBox(gb3Hwnd, "Hwnd of Groupbox 3 Edit")
    obj := GuiCtrlFromHwnd(gb3Hwnd)
    Result := IsObject(obj)
    MsgBox(Result, "If '1', is Object")
    ControlSetText(GB3Text, obj)
}

ClearGB3() {
    ControlSetText(, gb3Edit)
}

StartEditToolTip() {
ToolTipText := "
(
This uses 'ControlSetText' from a button in GroupBox 4 to populate this edit.
The first message box shows the HWND of this Edit.
The second message box shows '1' (True) if GuiCtrlFromHwnd created an
object from the Hwnd.
Finally, ControlSetText operates on the Object created from the Hwnd.
)"
    ToolTip(ToolTipText)
}

StopToolTip() {
    ToolTip()
}

; ┌───────────────────────────────┐
; │  Tab One Group Two functions  │
; └───────────────────────────────┘
Set_Style() {
    WinSetStyle("-0xC00000", "A")
}

Reset_Style() {
    WinSetStyle("+0xC00000", "A")
}

Set_Edit_Style() 
{
    ;MsgBox(HwndMyEdit, "This is the ID")
    ControlSetStyle("+0x8", HwndMyEdit)
    ControlFocus(HwndMyEdit)
}

Reset_Edit_Style() {
    Str := ControlGetStyle(HwndMyEdit)
    MsgBox(Format("0x{1:x}", Str), "Style of Edit1 Before Reset")

    ControlSetStyle("-0x8", HwndMyEdit)
    ControlFocus(HwndMyEdit)
    
    Str := ControlGetStyle(HwndMyEdit)
    MsgBox(Format("0x{1:x}", Str), "Style of Edit1 After Reset")
}

; ┌──────────────────────┐
; │  Move Gui functions  │
; └──────────────────────┘

MoveGui() {
    Tab.UseTab("Second")
    MyGui.UseGroup(gb2_TabTwo)
    global winpos
    winpos := WinGetPos(MyGui)
    MyGui.Move(100, 100, 200, 200)

}

MoveGuiBack() {
    Tab.UseTab("Second")
    MyGui.UseGroup(gb2_TabTwo)
    MyGui.Move(winpos["X"], winpos["Y"], winpos["Width"], winpos["Height"])
}

; ┌──────────────────────────┐
; │  Image Search functions  │
; └──────────────────────────┘

ImgSrch() {
CoordMode("Pixel", )  ; Interprets the coordinates below as relative to the screen rather than the active window.

    try 
    {
    result := ImageSearch(0, 0, A_ScreenWidth, A_ScreenHeight, "killbill.png")

    If (result["X"] != "")
        MsgBox("Found at x: " result["X"] " y: " result["Y"], "Image Search")
    Else
        MsgBox("Image not found!", "FAILURE")
    }

    catch as e  ; Handles the first error thrown by the block above.
    {
        MsgBox("An error was thrown!`nSpecifically: " e.Message)
        Exit
    }
}