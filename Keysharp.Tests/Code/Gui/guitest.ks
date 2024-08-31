If (FileExist(A_Desktop . "/MyScreenClip.png"))
	FileDelete(A_Desktop . "/MyScreenClip.png")

GuiBGColor := "BackgroundFF9A9A"
;BGColor2 := "0xFFFFAA"

Gui2 := ""

; Second GUI



; ┌───────────┐
; │  Globals  │
; └───────────┘

global gb3Hwnd, gui2StyleButtonHwnd

winposx := ""
winposy := ""
winposw := ""
winposh := ""
origBackColor := ""

; ┌────────────────┐
; │  Tab One Menu  │
; └────────────────┘
FileMenu := Menu()
FileMenu.Add("System", "MenuHandler")
FileMenu.Add("Script Icon", "MenuHandler")
FileMenu.Add("Suspend Icon", "MenuHandler")
FileMenu.Add("Pause Icon", "MenuHandler")
#if WINDOWS
	FileMenu.SetIcon("System", "Shell32.dll", 174) ; 2nd icon group from the file
#endif
FileMenu.SetIcon("Script Icon", A_KeysharpCorePath, "Keysharp.ico")
FileMenu.SetIcon("Suspend Icon", A_KeysharpCorePath, "Keysharp_s.ico")
FileMenu.SetIcon("Pause Icon", A_KeysharpCorePath, "Keysharp_p.ico")

ImgSrchMenu := Menu()
ImgSrchMenu.Add("Image Search Test", "ImgSrch")

MyMenuBar := MenuBar()
MyMenuBar.Add("&Menu Icon Test", FileMenu)
MyMenuBar.Add("Image Search", ImgSrchMenu)

MyGui := Gui(, "KEYSHARP TESTS")
MyGui.OnEvent("Close", "CloseApp")

#if LINUX
	MyGui.Show()
#endif

CloseApp() {
	ExitApp
}

; ┌───────────────────┐
; │  Add Menu to GUI  │
; └───────────────────┘

MyGui.MenuBar := MyMenuBar

; ┌──────────────┐
; │  Status Bar  │
; └──────────────┘
MySB := MyGui.Add("StatusBar", "h36", "                       ")

; ┌─────────────┐
; │  Start TAB  │
; └─────────────┘

Tab := MyGui.Add("Tab3", , ["First","Second","Third", "GroupBoxes", "ControlZoo", "Send & Hotkey", "Dll & COM", "Sound"])

Tab.UseTab("First")

; ┌──────────────────────┐
; │  Create the window:  │
; └──────────────────────┘

MyGui.SetFont("cBlack s8", "Arial")
TEST_HEADER := MyGui.Add("Text", "s20 w1200","Keysharp GUI Tests")

; ┌────────────────────────────────────┐
; │  Add button to change header font  │
; └────────────────────────────────────┘
headerBtn := MyGui.Add("Button", "s8 x10 y+10 Autosize", "Make header font larger Comic Sans MS")
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
Loop Files, A_MyDocuments . A_DirSeparator . "*.*"
	LV.Add(, A_LoopFileName, A_LoopFileSizeKB)

; ┌─────────────────────────────────────────────┐
; │  Show an input box and retrieve the result  │
; └─────────────────────────────────────────────┘
InputBtn := MyGui.Add("Button", "s8 x10 y+10", "Input Test")
InputBtn.OnEvent("Click", "InputTest")

; GetContentBtn := MyGui.Add("Button", "x100 yp", "Get LV Content")

;LV.ModifyCol()  ; Auto-size each column to fit its contents.
LV.ModifyCol(2, "Integer")  ; For sorting purposes, indicate that column 2 is an integer.

; ┌─────────────────────┐
; │  Add a radio group  │
; └─────────────────────┘

RadioText := MyGui.Add("Text", "w200 x10", "Radio group tests")
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

checkBtn := MyGui.Add("Button", "x10 y+10 Autosize", "ControlSetChecked")
checkBtn.OnEvent("Click", "SetChecked")

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


iniLabel := MyGui.Add("Text", "xm y+5 cRed", "Click to read kstests.ini`nKey = PRIMATE2`nValue = BONOBO")
iniBtn1 := MyGui.Add("Button", "x220 yp", "Read INI")
iniBtn1.OnEvent("Click", "ReadINI")
iniText := MyGui.Add("Text", "w100 x150 y+10", "")
iniEdit := MyGui.Add("Edit", "x10 y+10 w300 h180")

kstests := "." . A_DirSeparator . "kstests.ini"

ReadINI() {
	Val := IniRead(kstests, "section2", "PRIMATE2")
	iniText.SetFont("s10 cBlue")
	IniFileText := FileRead(kstests)
	ControlSetText(Val, iniText)
	;ControlSetText("Testing", iniText)
	ControlSetText(IniFileText, iniEdit)
	;ControlSetText("Still Testing", iniEdit)
}

iniWriteBtn := MyGui.Add("Button", "x10 y+10", "Write INI")
iniWriteBtn.OnEvent("Click", "WriteINI")
writeLabel := MyGui.Add("Text", "x100 yp cGreen", "Write and Re-Write`nChange case`nThen change back")
iniWriteEdit := MyGui.Add("Edit", "x10 y+10 w300 h180")

WriteINI() {
	IniWrite("BonoboBozo has been captured", kstests, "SECTION42", "PRIMATEZ_ON_LOOSE")
	IniFileText2 := FileRead(kstests)
	ControlSetText(IniFileText2, iniWriteEdit)
	Sleep(2000)
	IniWrite("BONOBOBOZO has escaped", kstests, "SECTION42", "PRIMATEZ_ON_LOOSE")
	IniFileText2 := FileRead(kstests)
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
gb1_TabTwo := MyGui.Add("GroupBox", "x10 y10 w325 h850", "Tab Two - Group One") ; 
;MyGui.UseGroup()
MyGui.UseGroup(gb1_TabTwo) 

; ┌────────┐
; │  Edit  │
; └────────┘
SecondEdit := MyGui.Add("Edit", "w300 h200")
SecondEditText := MyGui.Add("Text", "cBlue s10 w200", "ControlSetText Test")
HwndSecondEdit := SecondEdit.Hwnd
EditBtn1 := MyGui.Add("Button", "xp y+10 Autosize", "Text -> Edit")
EditBtn1.OnEvent("Click", "SendTextToEdit")
EditBtn2 := MyGui.Add("Button", "x+5 yp Autosize", "Clear Edit")
EditBtn2.OnEvent("Click", "ClearEdit")
EditHwndBtn := MyGui.Add("Button", "x+5 yp Autosize", "Show Edit Hwnd")
EditHwndBtn.OnEvent("Click", "ShowEditHwnd")

; ┌────────────┐
; │  RichEdit  │
; └────────────┘
SecondRichEdit := MyGui.Add("RichEdit", "x10 w250 h150", "Try pasting rich text and/or images here!")
SecondRichEditText := MyGui.Add("Text", "cBlue s10 w200", "ControlSetText Test (RichEdit)")
RichEditBtn1 := MyGui.Add("Button", "x10 y+10 Autosize", "Send Text to RichEdit")
RichEditBtn1.OnEvent("Click", "SendTextToRichEdit")
RichEditBtn2 := MyGui.Add("Button", "x+5 yp Autosize", "Send Rtf to RichEdit")
RichEditBtn2.OnEvent("Click", "SendRtfToRichEdit")
RichEditBtn3 := MyGui.Add("Button", "x10 y+5 Autosize", "Clear RichEdit")
RichEditBtn3.OnEvent("Click", "ClearRichEdit")
LinesBtn := MyGui.Add("Button", "x+5 yp Autosize", "EditGetLineCount")
LinesBtn.OnEvent("Click", "GetLineCount")

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
CoordText.SetFont("bold s16")
SetTimer("UpdateOSD", 200)
UpdateOSD()  ; Make the first update immediate rather than waiting for the timer.

; ┌──────────────────────────────┐
; │  Insert Image for searching  │
; └──────────────────────────────┘

SrchPic := MyGui.Add("Picture", "x10 y+10 h-1", A_ScriptDir . A_DirSeparator . "killbill.png")
SrchPicText := MyGui.Add("Text", "x10 y+15 w200", "^ Use top menu to find me!")
SrchPicText.SetFont("s10 cBlue")
; ┌────────────────────────┐
; │  End of Tab 2 Group 1  │
; └────────────────────────┘
MyGui.UseGroup()
Tab.UseTab("Second")
gb2_TabTwo := MyGui.Add("GroupBox", "x350 y10 w400 h600", "Tab Two - Group Two")
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

#if WINDOWS
	t2g2t3 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - ControlSetStyle")
#else
	t2g2t3 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - Opt(`"+Uppercase`")")
#endif

t2g2t3.SetFont("s10")
e3 := MyGui.Add("Edit", "w200 xp y+10 h50", "Edit 3")
MyGui.UseGroup(gb2_TabTwo)

#if WINDOWS
	ControlSetStyle("+0x8", e3)
#else
	e3.Opt("+Uppercase")
#endif

#if WINDOWS
	t2g2t4 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - +0x8")
#else
	t2g2t4 := MyGui.Add("Text", "xp y+10 w250 cBlue", "Uppercase - Constructor")
#endif

t2g2t4.SetFont("s10")

#if WINDOWS
	e4 := MyGui.Add("Edit", "w200 xp y+10 h50 +0x8", "Edit 4")
#else
	isUpper := true
	e4 := MyGui.Add("Edit", "w200 xp y+10 h50 Uppercase", "Edit 4")
#endif

e3Btn := MyGui.Add("Button", "xp y+10", "Toggle ControlSetStyle Edit")
e3Btn.OnEvent("Click", "ShowE3Hwnd")

numericText := MyGui.Add("Text", "x10 y+10 Autosize", "The text box below should be numeric only")
numericText.SetFont("s10 cBlue")
numericEdit := MyGui.Add("Edit", "w200 xp y+10 number")

setNumericBtn := MyGui.Add("Button", "x+10 yp", "Num")
setNumericBtn.SetFont("s8 cBlue")
setNumericBtn.OnEvent("Click", "SetNumeric")

resetNumericBtn := MyGui.Add("Button", "x+10 yp", "Unr")
resetNumericBtn.SetFont("s8 cBlue")
resetNumericBtn.OnEvent("Click", "ClearNumeric")

SetNumeric()
{
	numericEdit.Opt("+Number")
}

ClearNumeric()
{
	numericEdit.Opt("-Number")
}

ShowE3Hwnd()
{
#if WINDOWS
	ControlSetStyle("^0x8", e3)
#else
	global isUpper
	if (isUpper)
		e3.Opt("")
	else
		e3.Opt("+Uppercase")
	isUpper := !isUpper
#endif
	ControlFocus(e3.Hwnd)
}

; ┌────────────┐
; │  Move GUI  │
; └────────────┘

MoveText := MyGui.Add("Text", "x10 y+10 Autosize", "Move window with this caption, or blank to move this window")
MoveText.SetFont("s10 cBlue")
MoveEdit := MyGui.Add("Edit", "w200 xp y+10", "")
#if LINUX
	MoveEdit.Text := "About Calculator"
#endif
MoveButton := MyGui.Add("Button", , "Move GUI")
MoveButton.OnEvent("Focus", "ChangeMoveBtnColor")
MoveButton.OnEvent("Click", "MoveGui")
MoveButtonBack := MyGui.Add("Button", "x120 yp Autosize", "Move GUI Back")
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

#if WINDOWS
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
#endif

MyGui.UseGroup()
Tab.UseTab("Second")

; ┌───────────────┐
; │  Add Picture  │
; └───────────────┘
MyPictureBtn := MyGui.Add("Button", "cBlue s10 xp y+10 Autosize", "Display a picture")
MyPictureBtn.OnEvent("Click", "LoadPic")
SlugLine := MyGui.Add("Text", "cBlue s10 w200 xp y800", "Picture will display above")

#if WINDOWS
DestroyPictureBtn := MyGui.Add("Button", "cBlue s10 xp y+10 Autosize", "Destroy picture control")
DestroyPictureBtn.OnEvent("Click", "DestroyPic")
#endif

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

	Try 
	{
		WhiteIndex := ControlFindItem("White", MyListBox)
	}
	Catch as e  ; Handles the first error thrown by the block above.
	{
		MsgBox("An error was thrown!`nSpecifically: " e.Message, "ERROR!")
		Return
	}
	
	ControlDeleteItem(WhiteIndex, MyListBox)
}

AddWhite() {
	ControlAddItem("White", MyListBox)
}

; ┌────────────────┐
; │  Multi-select  │
; └────────────────┘
ThirdText2 := MyGui.Add("Text", "x10 y+65 cBlue s10", "ListBox Test (Multi-Select)")
MyMultiLB := MyGui.Add("ListBox", "+Multi r5 w110 x10 y+10", ["Reactionary Red","Garish Green","Beastly Blue","Banal Black","Washed-out White"])
MyMultiLB.OnEvent("Change", "MultiLBClicked")

; ┌─────────────┐
; │  Drop-Down  │
; └─────────────┘
ThirdText3 := MyGui.Add("Text", "x10 y+10 cBlue s10", "Drop-down List with 4 rows")
MyDDL := MyGui.Add("DropDownList", "x10 y+10 r4", ["Orange","Purple","Fuchsia","Lime","Aqua"])
MyDDL.OnEvent("Change", "DDLClicked")

; ┌─────────────┐
; │  Combo Box  │
; └─────────────┘
ThirdText4 := MyGui.Add("Text", "x10 cBlue s10", "ComboBox with 3 rows")
MyCB := MyGui.Add("ComboBox", "x10 y+10 r3", ["Orange","Purple","Fuchsia","Lime","Aqua"])
CB_Button := MyGui.Add("Button", "h25 w80 x10 y+10", "CB Selection")
CB_AddBtn := MyGui.Add("Button", "h25 w80 x90 yp", "Add Yellow")
CB_Button.OnEvent("Click", "CB_ButtonClicked")
CB_AddBtn.OnEvent("Click", "AddYellow")
CB_DeleteBtn := MyGui.Add("Button", "h25 w80 x170 yp", "Del Yellow")
CB_DeleteBtn.OnEvent("Click", "DeleteYellow")

AddYellow() {
	ControlAddItem("Yellow", MyCB)
}

DeleteYellow() {

	Try 
	{
		YellowIndex := ControlFindItem("Yellow", MyCB)
	}
	Catch as e  ; Handles the first error thrown by the block above.
	{
		MsgBox("An error was thrown!`nSpecifically: " e.Message, "ERROR!")
		Return
	}
	
	ControlDeleteItem(YellowIndex, MyCb)
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
MyProgress := MyGui.Add("Progress", "x10 y+10 cRed BackgroundGreen Smooth", 50)
#if WINDOWS
	MyProgress.GetPos(&px, &py, &pw, &ph)
	MyVertProgress := MyGui.Add("Progress", "cRed BackgroundGreen x+10 yp-" . (pw - ph) . " Vertical w" . ph . " h" . pw, 50) ; Swap width and height.
#endif
Pbtn1 := MyGui.Add("Button", "s8 x10 y+5", "Lower")
Pbtn2 := MyGui.Add("Button", "s8 x100 yp", "Higher")
ProgressStatusText := MyGui.Add("Text", "x+5 yp cBlue s10 Autosize", "Value: ")
Pbtn1.OnEvent("Click", "Pbtn1Clicked")
Pbtn2.OnEvent("Click", "Pbtn2Clicked")

; ┌─────────────┐
; │  Date Time  │
; └─────────────┘
ThirdText7 := MyGui.Add("Text", "x10 y+15 cBlue s10", "DateTime Test")
MyDateTime := MyGui.Add("DateTime", "s8 x10 y+5 w200", "LongDate")

; ┌────────────┐
; │  MonthCal  │
; └────────────┘
ThirdText8 := MyGui.Add("Text", "x10 y+5 cBlue s10", "MonthCal Test")
MyMonthCal := MyGui.Add("MonthCal")
MC_Btn := MyGui.Add("Button", "s8 x10 y+5 Autosize", "Change Cal Colors (not implemented)")
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
MyText := MyGui.Add("Text", "x10 y+10 w300 h30")
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

MyLinkText := MyGui.Add("Text", "x10 y+5", "Link test")
MyLinkText.SetFont("cBlue s8")
MyLink := MyGui.Add("Link", "x10 y+5", 'Click this <a href="https://www.autohotkey.com">link to AHK page</a>')

MyHkInfoText := MyGui.Add("Text", "x10 y+5 w200", "Define Hotkey test`nFocus Edit and click hotkey(s)")
MyHkInfoText.SetFont("cBlue s8")
MyHotkey := MyGui.Add("Hotkey", "x10 y+5")
MyHotkey.OnEvent("Change", "UpdateHK")
MyHkText := MyGui.Add("Text", "x10 y+5 w200" , MyHotkey.Value)
;MyHkText2 := MyGui.Add("Text", "x10 y+5 w200 cRed", "NOTE: Combos w/Win not working.")

UpdateHK() {
	ControlSetText(MyHotkey.Value, MyHkText)
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

; ┌───────────────────────┐
; │  Image copying tests  │
; └───────────────────────┘
gb1 := MyGui.Add("GroupBox", "x10 y10 w330 h400", "Group One")
MyGui.UseGroup(gb1)
CpText := MyGui.Add("Text", , "gb1 - Image copying tests")
CpText.SetFont("s8 cBlue")
MyRE := MyGui.Add("RichEdit", "x10 y+10 w300 h100")
MySecondPic := LoadPicture(A_ScriptDir . A_DirSeparator . "Robin.png")
CopyImageToClipboard("HBITMAP:" MySecondPic)
ShowBtn := MyGui.Add("Button", "x10 y+10", "Paste Pic")
ShowBtn.OnEvent("Click", "PastePic")

PastePic() {
	ControlFocus(MyRE)
#if WINDOWS
	Send("^v")
#endif
}

; ┌──────────────────────────────┐
; │  Now load a pic from a file  │
; └──────────────────────────────┘

LpText := MyGui.Add("Text", "x10 y+10", "Now copy a pic from a file.")
LpText.SetFont("s8 cBlue")
MyRE2 := MyGui.Add("RichEdit", "x10 y+10 w300 h100")
ShowBtn2 := MyGui.Add("Button", "x10 y+10", "Paste from file")
ShowBtn2.OnEvent("Click", "CopyPicFromFile")

CopyPicFromFile() {
	SelectedFile := FileSelect("3", A_AppData . A_DirSeparator . "Pictures")
	
	if (SelectedFile != "")
	{
		CopyImageToClipboard(SelectedFile)
		Sleep(100)
		ControlFocus(MyRE2)
#if WINDOWS
		Send("^v")
#endif
	}
}

MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
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
gb2 := MyGui.Add("GroupBox", "x10 y+10 w330 h400", "Group Two")
MyGui.UseGroup(gb2)
MyGui.Add("Text", "cBlue s8 w200", "Testing various Send() types")
gb2Edit := MyGui.Add("Edit", "x10 y+5 w300 h250")
gb2Btn1 := MyGui.Add("Button", "x10", "Notepad")
gb2Btn1.OnEvent("Click", "SendToApp")
gb2Btn2 := MyGui.Add("Button", "x95 yp", "This Edit")
gb2Btn2.OnEvent("Click", "SendToGui")


;MyGui.Add("Text", , "Testing placement")

SendToApp() {
#if WINDOWS
	Run("Notepad.exe")
	WinWaitActive("ahk_exe Notepad.exe")
	SendInput("Sincerely,{enter}John Smith")
	Send("`n")
	Send("Another line.`n")
	Send("{Raw}``100`%`n")
	Send("{Blind}{Text}You should see '{Blind}{Text}' after the ellipses ... {Blind}{Text}`n")
	; Line above produces [You should see '' after the ellipses ...] 
	Send("{Blind}You should see nothing after the ellipses ... {Blind}")
	Send("`n")
	Send("{Text}You should see the Blind mode syntax in single quotes after the ellipses ... '{Blind}'")
	Sleep(500)
	MsgBox("End of Notepad test", "Test finished", "T2")
	Send("{Alt}Fx")
	Sleep(100)
	Send("{Tab}{Enter}")
#endif
}

SendToGui() {
#if WINDOWS
	WinActivate(MyGui)
	ControlFocus(gb2Edit)
	SendInput("Sincerely,{enter}John Smith")
	Send("`n")
	Send("Another line.`n")
	Send("{Raw}``100`%`n")
	Send("{Blind}{Text}You should see '{Blind}{Text}' after the ellipses ... {Blind}{Text}`n")
	Send("{Blind}You should see nothing after the ellipses ... {Blind}")
	Send("`n")
	Send("{Text}You should see the Blind mode syntax in single quotes after the ellipses ... '{Blind}'")
#endif
}


MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
gb4 := MyGui.Add("GroupBox", "x+10 yp w330 h400", "Group Four")
MyGui.UseGroup(gb4)
MyGui.Add("Text", "xp", "Testing gb4")
gb4Btn1 := MyGui.Add("Button", "s14 cLime", "Send to GB3")
gb4Btn1.OnEvent("Click", "SendToGB3")
gb4Btn2 := MyGui.Add("Button", "s14 cLime x+5 yp", "Clear GB3")
gb4Btn2.OnEvent("Click", "ClearGB3")
MyGui.UseGroup()
Tab.UseTab("GroupBoxes")
MyGui.AddText("s14 xm y+10 cBlue", "This should be below.")

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

#if WINDOWS
#z::MyMenu.Show()  ; i.e. press the Win-Z hotkey to show the menu.
;#z::Run("Notepad.exe")
#endif
; ┌──────────────────┐
; │  ControlZoo Tab  │
; └──────────────────┘

MyGui.UseGroup()
Tab.UseTab("ControlZoo")
gb1_CZ := MyGui.Add("GroupBox", "x10 y10 w380 h875", "ControlZoo - Group One")
CZ_Text1 := MyGui.Add("Text", , "Control Functions testing")
CZ_Text1.SetFont("s10 CBlue")
CZ_Text2 := MyGui.Add("Text", "x10 y+10 w300 h30 Wordwrap Border", "For the controls on this tab, we'll add, delete, click, focus and perform other control functions.")
CZ_Text2.SetFont("CTeal")

CZ_Text2a := MyGui.Add("Text", "x10 y+5", "ListBox control testing")
CZ_Text2a.SetFont("s8 CBlue")

CZ_ListBox := MyGui.Add("ListBox", "x10 h300 w160", ["Red","Green","Blue","Black","White", "Maroon"
	, "Purple", "Color de gos com fuig", "Weiß", "Amarillo", "красный"
	, "朱红"])

CZ_Text3 := MyGui.Add("Text", "x10 y+5", "Edit control testing")
CZ_Text3.SetFont("s8 CBlue")

CZ_Edit1 := MyGui.Add("Edit", "x10 y+5 w160 h100")

; ┌─────────────────────────────────────────────┐
; │  ControlZoo - end of Group One, Column One  │
; └─────────────────────────────────────────────┘


CZ_LbBtn1 := MyGui.Add("Button", "x180 w120 h25 y95", "Add Fuchsia")
CZ_LbBtn1.OnEvent("Click", "AddFuchsia")
CZ_LbBtn2 := MyGui.Add("Button", "x180 w120 h25 y+1", "Delete Fuchsia")
CZ_LbBtn2.OnEvent("Click", "DeleteFuchsia")
CZ_LbBtn2.OnEvent("Focus", "FuchsiaDeleteTrayTip")
CZ_LbBtn3 := MyGui.Add("Button", "x180 w120 h25 y+1", "Purple (Index)")
CZ_LbBtn3.OnEvent("Click", "ChooseIndex")
CZ_LbBtn4 := MyGui.Add("Button", "x180 w120 h25 y+1", "красный (String)")
CZ_LbBtn4.OnEvent("Click", "ChooseString")
CZ_LbBtn5 := MyGui.Add("Button", "x180 w120 h25 y+1", "ControlGetChoice")
CZ_LbBtn5.OnEvent("Click", "GetChoice")
CZ_LbBtn19 := MyGui.Add("Button", "x180 w120 h25 y+1", "ControlGetIndex")
CZ_LbBtn19.OnEvent("Click", "GetIndex")
CZ_LbBtn6 := MyGui.Add("Button", "x180 w120 h25 y+1", "ControlGetClassNN")
CZ_LbBtn6.OnEvent("Click", "GetClassNN")

CZ_LbBtn7 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlGetEnabled")
CZ_LbBtn7.OnEvent("Click", "GetEnabled")
CZ_LbBtn20 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlSetEnabled")
CZ_LbBtn20.OnEvent("Click", "SetEnabled")
CZ_LbBtn8 := MyGui.Add("Button", "w120 x180 h25 y+1", "Disabled!")
CZ_LbBtn8.Enabled := False

CZ_LbBtn9 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlGetHwnd")
CZ_LbBtn9.OnEvent("Click", "GetHwnd")

CZ_LbBtn10 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlGetText")
CZ_LbBtn10.OnEvent("Click", "GetText")

CZ_LbBtn11 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlHide")
CZ_LbBtn11.OnEvent("Click", "HideButton")

CZ_LbBtn12 := MyGui.Add("Button", "w120 x180 h25 y+1", "ControlShow")
CZ_LbBtn12.OnEvent("Click", "ShowButton")

CZ_LbBtn13 := MyGui.Add("Button", "w120 x180 h25 y+1", "Visible?")
CZ_LbBtn13.OnEvent("Click", "IsItHidden")

CZ_LbBtn14 := MyGui.Add("Button", "w120 x180 h25 y+1", "Edit Column #")
CZ_LbBtn14.OnEvent("Click", "GetCol")

CZ_LbBtn15 := MyGui.Add("Button", "w120 x180 h25 y+1", "Edit Line #")
CZ_LbBtn15.OnEvent("Click", "GetLine")

CZ_LbBtn16 := MyGui.Add("Button", "w120 x180 h25 y+1", "Edit Line Text")
CZ_LbBtn16.OnEvent("Click", "GetLineText")

CZ_LbBtn17 := MyGui.Add("Button", "w120 x180 h25 y+1", "Selected text")
CZ_LbBtn17.OnEvent("Click", "GetSelectedText")

CZ_LbBtn18 := MyGui.Add("Button", "w120 x180 h25 y+1", "Edit Paste")
CZ_LbBtn18.OnEvent("Click", "EditPaster")

; ┌──────────────────────────┐
; │  ListView Content Tests  │
; └──────────────────────────┘

CZ_SeparatorText1 := MyGui.Add("Text", "x10 yp+8 w320", "ListView Content Tests")
CZ_SeparatorText1.SetFont("s8 CBlue")

LV2 := MyGui.Add("ListView", "r5 w300 x10 y+5", ["Name","Size (KB)"])

Loop Files, A_MyDocuments . A_DirSeparator . "*.*"
  LV2.Add(, A_LoopFileName, A_LoopFileSizeKB)

LV2_Btn1 := MyGui.Add("Button", "x10 y+5 w72 h25" ,"Selected")
LV2_Btn1.OnEvent("Click", "LV_Selected")

LV2_Btn2 := MyGui.Add("Button", "x85 yp w72 h25" ,"Focused")
LV2_Btn2.OnEvent("Click", "LV_Focused")

LV2_Btn3 := MyGui.Add("Button", "x160 yp wp+10 hp+10", "Column 1")
LV2_Btn3.OnEvent("Click", "LV_Col1")

LV2_Btn4 := MyGui.Add("Button", "x+3 yp wp+10 hp+10", "Count")
LV2_Btn4.OnEvent("Click", "LV_Count")

LV2_Btn5 := MyGui.Add("Button", "x10 yp+45 w100 h25", "Count Selected")
LV2_Btn5.OnEvent("Click", "LV_CountSelected")

LV2_Btn6 := MyGui.Add("Button", "x113 yp w100 h25", "Row Focused")
LV2_Btn6.OnEvent("Click", "LV_CountFocused")

LV2_Btn7 := MyGui.Add("Button", "x216 yp w100 h25", "Count Columns")
LV2_Btn7.OnEvent("Click", "LV_CountCol")

MyGui.UseGroup()
Tab.UseTab("ControlZoo")
gb2_CZ := MyGui.Add("GroupBox", "x+10 y10 w325 h875", "ControlZoo - Group Two")
MyGui.UseGroup(gb2_CZ)

;Reserved4 := MyGui.Add("Text", "x10 y20 w325", "Reserved for Future Testing")
;Reserved4.SetFont("s12 CBlue")
gb2_CZ_Text1 := MyGui.Add("Text", "x10 y20 w325", "ComboBox Control Tests")
gb2_CZ_Text1.SetFont("s8 cBlue")

gb2_CZ_CB := MyGui.Add("ComboBox", "x10 y+10 r5 Limit", ["Orange","Purple","Fuchsia","Lime","Aqua"])
gb2_CZ_Btn1 := MyGui.Add("Button", "x10 y+5 w80 h25", "Add White")
gb2_CZ_Btn1.OnEvent("Click", "AddWhite2")
gb2_CZ_Btn2 := MyGui.Add("Button", "x90 yp w80 h25", "Delete White")
gb2_CZ_Btn2.OnEvent("Click", "DeleteWhite2")
gb2_CZ_Btn3 := MyGui.Add("Button", "x170 yp w80 h25", "-> Purple")
gb2_CZ_Btn3.OnEvent("Click", "ChooseString_CB")

gb2_CZ_Btn4 := MyGui.Add("Button", "x10 y+5 w200 h25", "Click Win+R, show dropdown")
gb2_CZ_Btn4.OnEvent("Click", "Click_CB")

gb2_CZ_Btn5 := MyGui.Add("Button", "x10 y+5 Autosize", "Show ListBox items")
gb2_CZ_Btn5.OnEvent("Click", "Click_CB_Items")

gb2_CZ_Btn6 := MyGui.Add("Button", "x+5 yp Autosize", "Show ComboBox items")
gb2_CZ_Btn6.OnEvent("Click", "Click_LB_Items")

gb2_CZ_Btn7 := MyGui.Add("Button", "x10 y+5 Autosize", "Show ComboBox dropdown")
gb2_CZ_Btn7.OnEvent("Click", "Click_CB_Show_Dropdown")

gb2_CZ_Btn8 := MyGui.Add("Button", "x+5 yp Autosize", "Hide ComboBox dropdown")
gb2_CZ_Btn8.OnEvent("Click", "Click_CB_Hide_Dropdown")

gb2_CZ_Text2 := MyGui.Add("Text", "x10 y+10 w325", "Move mouse to color. Press Ctrl+Alt+9.")
gb2_CZ_Text2.SetFont("s8 cBlue")

MyColorLabel := MyGui.Add("Text", "x10 y+10 w200", "Empty text below:")
MyColorText := MyGui.Add("Text", "w200 x10 y+10", "")

SecondGuiButton := MyGui.Add("Button", "x10 y+35 Autosize", "Control Tests Redux")
SecondGuiButton.OnEvent("Click", "SecondGUI")
FindEdit := MyGui.Add("Button", "x10 y+5 Autosize", "Get Edit Hwnd")
FindEdit.OnEvent("Click", "FindSecondGuiEdit")

ThirdGuiButton := MyGui.Add("Button", "x10 y+5 Autosize", "'Find By' Tests")
ThirdGuiButton.OnEvent("Click", "ThirdGUI")

MouseMoveButton := MyGui.Add("Button", "x10 y+5 Autosize", "Mouse-moving tests")
MouseMoveButton.OnEvent("Click", "MoveTheMouse")

#if WINDOWS
AddMsgMonitorButton := MyGui.Add("Button", "x10 y+5 Autosize", "Add msg mon for clicking in edit ctrl testing")
AddMsgMonitorButton.OnEvent("Click", "AddMsgMonitor")

RemoveMsgMonitorButton := MyGui.Add("Button", "x10 y+5 Autosize", "Remove msg mon")
RemoveMsgMonitorButton.OnEvent("Click", "RemoveMsgMonitor")
#endif

MinimizeAllButton := MyGui.Add("Button", "x10 y+5", "Minimize all")
MinimizeAllButton.OnEvent("Click", "MinimizeAll")
UndoMinimizeAllButton := MyGui.Add("Button", "x10 y+5", "Undo minimize all")
UndoMinimizeAllButton.OnEvent("Click", "UndoMinimizeAll")
MaximizeAllButton := MyGui.Add("Button", "x10 y+5", "Maximize all")
MaximizeAllButton.OnEvent("Click", "MaximizeAll")
MoveAllButton := MyGui.Add("Button", "x10 y+5", "Move me")
MoveAllButton.OnEvent("Click", "MoveButton")

MinimizeAll()
{
	WinMinimizeAll()
}

UndoMinimizeAll()
{
	WinMinimizeAllUndo()
}

MaximizeAll()
{
	WinMaximizeAll()
}

MoveButton()
{
	local x, y, w, h
	
	ControlGetPos(&x, &y, &w, &h, MoveAllButton.Hwnd, MyGui)
	x++
	y++
	ControlMove(x, y, w, h, MoveAllButton.Hwnd, MyGui)
}

#if WINDOWS
^!9:: {
	GetPix()
}

#endif

Gui2 := Gui(,"Testing Child GUI")
Gui2.Opt("+Owner")

Gui2StyleButton := Gui2.Add("Button", ,"Style Button")
Gui2StyleButton.OnEvent("Click", "StyleTest")

Gui2GetControlsButton := Gui2.Add("Button", "x100 yp", "Get Ctrls")
Gui2GetControlsButton.OnEvent("Click", "GetTheControls")

Gui2FindCtrlsButton := Gui2.Add("Button", "x180 yp", "Enum Ctrls")
Gui2FindCtrlsButton.OnEvent("Click", "EnumCtrls")

Gui2CtrlIndexButton := Gui2.Add("Button", "x260 yp Autosize", "Find by _Item")
Gui2CtrlIndexButton.OnEvent("Click", "FindByItem")

Gui2Edit := Gui2.Add("Edit", "x10 y+20 h400 w500 +Multiline")
;MsgBox(Gui2Edit.Hwnd, "Hwnd of Edit")

SecondGUI() {
	Gui2.Show()
	ControlGetPos(&x, &y,,, Gui2Edit.Hwnd)
	Gui2Edit.Text := "Edit position: " . x . " " y
}

GetTheControls() {
	MyWords := Gui2Edit.Hwnd
	MyBtn1 := ControlGetClassNN(Gui2StyleButton.Hwnd)
	TheMsg := "The Style Button's ClassNN is " . MyBtn1 . "`n"
	TheMsg := TheMsg . "`nSecond button's Hwnd is " . Gui2GetControlsButton.Hwnd . "`n"

	TheMsg := TheMsg . "`nThe Main GUI's hwnd is " . MyGui.Hwnd

	ControlGetPos(&x, &y,,, Gui2FindCtrlsButton.Hwnd)
	TheMsg .= "`n`nFind button's position is " . x . ", " . y
	MsgBox(TheMsg, "Testing different methods of finding controls")
	Sleep(2000)
}

FindSecondGuiEdit() {
	; Called by button "Get Edit Hwnd"
	MyWords := Gui2Edit.Hwnd
	StyleBtn := ControlGetClassNN(Gui2StyleButton.Hwnd)
	TheOtherMsg := "The Style Button's ClassNN is " . StyleBtn
	TheOtherMsg := TheOtherMsg . "`nChild GUI Edit's hwnd is " . MyWords

	MsgBox(TheOtherMsg, "More testing of different methods to find controls")

}

EnumCtrls() {
	For GuiCtrlObj in MyGui {
		theNN := ControlGetClassNN(GuiCtrlObj, MyGui)
		theMsg.= "Control #" A_Index " is " theNN "`n"
	}
	Gui2Edit.Value := theMsg
}

StyleTest()  {
	ToolTip("Setting style to -0xC00000`n(Will revert in two seconds to`n+0xC00000)")
	WinSetStyle("-0xC00000", "A")
	Sleep(2000)
	ToolTip
	WinSetStyle("+0xC00000", "A")
}

FindByItem() {
	EditObj := Gui2Edit
	MsgBox(EditObj.Text)
}

; GUI3

Gui3 := Gui(, "KEYSHARP TESTS")
Gui3.Name := "Howard"
ButtonOne := Gui3.Add("Button", "w200", "Find by Text")
ButtonOne.OnEvent("Click", "FindByText")
ButtonTwo := Gui3.Add("Button", "w200", "Find by Hwnd")
ButtonTwo.OnEvent("Click", "FindByHwnd")
;ButtonThree := Gui3.Add("Button", "w200", "Find by ClassNN")
;ButtonThree.OnEvent("Click", "FindByClassNN")
ButtonFour := Gui3.Add("Button", "w200", "Find by NetClassNN")
ButtonFour.OnEvent("Click", "FindByNetClassNN")
ButtonFive := Gui3.Add("Button", "w200", "Find by Name")
ButtonFive.OnEvent("Click", "FindByName")

ButtonDummy := Gui3.Add("Button", "w200", "Test Dummy")
ButtonDummy.Name := "I am a dummy button"
MyEdit3 := Gui3.Add("Edit", "x10 h200 w200")

HwndText := "Test Dummy button hwnd: " . ButtonDummy.Hwnd
MyEdit3.Value := HwndText

;Gui3.Show()

FindByText() {
	theItem := Gui3["Find by Name"]
	MsgBox("I found a button. Text:`n" theItem.Text, "Find by Text")
}

FindByHwnd() {
	theItem := Gui3[ButtonTwo.Hwnd]
	MsgBox("I found a button by its Hwnd. Text:`n" theItem.Text, "Find by Hwnd")
}

;FindByClassNN() {
;    theItem := Gui3["WindowsForms10.Button.app.0.5dbcd3_r3_ad11"]
;    MsgBox(theItem.Text, "Find by ClassNN")
;}

FindByNetClassNN() {
	theItem := Gui3["KeysharpButton5"]
	MsgBox("I found a button by its .NET classname. Text:`n" theItem.Text, "Find by NetClassNN")
}

FindByName() {
	theItem := Gui3[ButtonDummy.Name]
	MsgBox("I found a renamed button by Name.`nIt was renamed to:`n" theItem.Name, "Find by Name")
}

ThirdGUI() {
	Gui3.Show()
}

;MouseMoveTests() {
;    MsgBox("Dead monkey")
;}


MoveTheMouse() {
#if WINDOWS
	mx :=
	my := 0
	CoordMode("Mouse", "Screen")
	MouseGetPos(&mx, &my)
	SendMode("Event")
	MouseMove(100,500,90)
	ToolTip("I'm at X:100, Y:500")
	Sleep(2000)
	MouseMove(1500,500,50)
	ToolTip("I'm here!")
	Sleep(2000)
	ToolTip()
	MouseMove(mx, my, 90)
	ToolTip("I'm back!")
	Sleep(2000)
	ToolTip()
#endif
}

#if WINDOWS
AddMsgMonitor()
{
	OnMessage 0x0201, "WM_LBUTTONDOWN"
}

RemoveMsgMonitor()
{
	OnMessage 0x0201, "WM_LBUTTONDOWN", 0
}
#endif

WM_LBUTTONDOWN(wParam, lParam, msg, hwnd)
{
    X := lParam & 0xFFFF
    Y := lParam >> 16
    Control := ""
    thisGui := GuiFromHwnd(hwnd)
    thisGuiControl := GuiCtrlFromHwnd(hwnd)

    if (thisGuiControl && (thisGuiControl.hwnd == CZ_Edit1.hwnd))
    {
        thisGui := thisGuiControl.Gui
        Control := "`n(in control " . thisGuiControl.ClassNN . ")"
		ToolTip "You left-clicked in Gui window '" thisGui.Title "' at client coordinates " X "x" Y "." Control
		SetTimer(() => ToolTip(), -2500)
    }
}

;ReloaderBtn := MyGui.Add("Button", "w200 h25 x10 y+5", "Reload").OnEvent("Click", "Reload")

;ReloadMe() {
;    Reload()
;}

; ┌────────────────────────┐
; │  ControlZoo Functions  │
; └────────────────────────┘

AddFuchsia() {
	ControlAddItem("Fuchsia", CZ_ListBox)
}

AddWhite2() {
	ControlAddItem("White", gb2_CZ_CB)
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

DeleteWhite2() {
	Try 
	{
		WhiteIndex := ControlFindItem("White", gb2_CZ_CB)
	}
	Catch as e  ; Handles the first error thrown by the block above.
	{
		MsgBox("An error was thrown!`nSpecifically: " e.Message, "ERROR!")
		Return
	}
	
	ControlDeleteItem(WhiteIndex, gb2_CZ_CB)
}

FuchsiaDeleteTrayTip() {
#if LINUX
	TrayTip("Also tests ControlFindItem`nClick into another edit box to dismiss this.")
#else
	TrayTip("Also tests ControlFindItem")
#endif
}

ChooseIndex() {
	ControlChooseIndex(7, CZ_ListBox)
}

ChooseString() {
	ControlChooseString("красный", CZ_ListBox)
}

ChooseString_CB() {
	ControlChooseString("Purple", gb2_CZ_CB)
}

GetChoice() {
	Try
	{
	Choice := ControlGetChoice(CZ_ListBox, MyGui)
	MsgBox(Choice, "Choice")
	}
		Catch as e  ; Handles the first error thrown by the block above.
	{
		MsgBox("You must select an item first.", "ERROR!")
		Return
	}
}

GetIndex() {
	Try
	{
	index := ControlGetIndex(CZ_ListBox, MyGui)
	MsgBox(index, "Index")
	}
		Catch as e  ; Handles the first error thrown by the block above.
	{
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

SetEnabled()
{
	Result := ControlGetEnabled(CZ_LbBtn8, MyGui)
	ControlSetEnabled(!Result, CZ_LbBtn8, MyGui)
}

GetHwnd() {
	Result := ControlGetHwnd(CZ_ListBox, MyGui)
	MsgBox(Result, "Hwnd of ListBox")
}

GetText() {
	Result := ControlGetText(CZ_LbBtn8, MyGui)
	MsgBox(Result, "Text of Target Button")
}

HideButton() {
	ControlHide(CZ_LbBtn8, MyGui)
}

ShowButton() {
	ControlShow(CZ_LbBtn8, MyGui)
}

IsItHidden() {
	Result := ControlGetVisible(Cz_LbBtn8, MyGui)
	If (Result != 0) {
		Result := "Visible"
	} Else {
		Result := "Hidden"
	}
	MsgBox(Result, "Visible or Not?")
}

GetCol() {
	CurrentCol := EditGetCurrentCol(CZ_Edit1, MyGui)
	MsgBox(CurrentCol, "Current Colum No.")
	CurrentCol := ""
}

GetLine() {
	CurrentLine := EditGetCurrentLine(CZ_Edit1, MyGui)
	MsgBox(CurrentLine, "Current Line No.")
	CurrentLine := ""
}

GetLineText() {
	CurrentLine := EditGetCurrentLine(CZ_Edit1, MyGui)
	CurrentLineText := EditGetLine(CurrentLine, CZ_Edit1, MyGui)
	MsgBox(CurrentLineText, "Current Line Text")
	CurrentLineText := "" ; Reset variable
}

GetSelectedText() {
	SelectedText := EditGetSelectedText(CZ_Edit1, MyGui)
	MsgBox(SelectedText, "Selected text in Edit")
	SelectedText := "" ; Reset variable
}

EditPaster() {
	EditPasted := "How now brown cow"
	EditPaste(EditPasted, CZ_Edit1, MyGui)
}

LV_Selected() {
	List := ListViewGetContent("Selected", LV2, MyGui)
	MsgBox(List, "LV Selected")
	List := ""
}

LV_Focused() {
	List := ListViewGetContent("Focused", LV2, MyGui)
	MsgBox(List, "LV Focused")
	List := ""
}

LV_Col1() {
	List := ListViewGetContent("Col1", LV2, MyGui)
	MsgBox(List, "LV Column 1")
	List := ""
}

LV_Count() {
	List := ListViewGetContent("Count", LV2, MyGui)
	MsgBox(List, "LV Row Count")
	List := ""
}

LV_CountSelected() {
	List := ListViewGetContent("Count Selected", LV2, MyGui)
	MsgBox(List, "LV Count Selected")
	List := ""
}

LV_CountFocused() {
	List := ListViewGetContent("Count Focused", LV2, MyGui)
	MsgBox(List, "LV Count Focused")
	List := ""
}

LV_CountCol() {
	List := ListViewGetContent("Count Col", LV2, MyGui)
	MsgBox(List, "LV Column Count")
	List := ""
}


Click_CB() {
#if WINDOWS
	Send("#r")  ; Open the Run dialog.
	WinWaitActive("ahk_class #32770")  ; Wait for the dialog to appear.
	ControlShowDropDown("ComboBox1")  ; Show the drop-down list. The second parameter is omitted so that the last found window is used.
	Sleep(2000)
	ControlHideDropDown("ComboBox1")  ; Hide the drop-down list.
	Sleep(1000)
	Send("{Esc}")  ; Close the Run dialog.
#endif
}

GetPix() {
	mx :=
	my := 0
	MouseGetPos(&mx, &my)
	MyColorText.Text := PixelGetColor(mx, my)
	ColorString := "Bold s12 c" MyColorText.Text
	ColorString := StrReplace(ColorString, "0x", "")
	MyColorText.SetFont(ColorString)
}

Click_CB_Items()
{
	global CZ_ListBox
	items := ControlGetItems(CZ_ListBox)
	MsgBox(items.Join("`n"))
}

Click_LB_Items()
{
	global gb2_CZ_CB
	items := ControlGetItems(gb2_CZ_CB)
	MsgBox(items.Join("`n"))
}

Click_CB_Show_Dropdown()
{
	ControlShowDropDown(gb2_CZ_CB, MyGui)
}

Click_CB_Hide_Dropdown()
{
	ControlHideDropDown(gb2_CZ_CB, MyGui)
}

#if WINDOWS
LoadSC() {
	Tab.UseTab("Send & Hotkey")
	path := A_Desktop . A_DirSeparator . "MyScreenClip.png"
	If (!FileExist(path)) {
		GetScreenClip(100, 100, 200, 200, path)
		Sleep(100)
	}
	MyThirdPic := LoadPicture(path)

	MyLoadedPic := MyGui.Add("Picture", "x450 y700 w170 h170", "HBITMAP:" MyThirdPic)
	Sleep(2000)

	DllCall("DestroyWindow", "Ptr", MyLoadedPic.Hwnd)
	; Tab.UseTab()
	FileDelete(path)
	MyThirdPic := ""
	MyLoadedPic := ""
	MyThirdPic := ""
}

; ┌───────────────────────┐
; │  SEND & HOTKEY TESTS  │
; └───────────────────────┘



MyGui.UseGroup()
Tab.UseTab("Send & Hotkey")
SectionTopText := MyGui.Add("Text", "x10 y10 w600", "This section is for testing the various Send() variants and the Hotkey method.")
SectionTopText.SetFont("cBlue s12")
MySendEdit := MyGui.Add("Edit", "x10 y+10 w700 h250", "The buttons below this Edit will use various Send() variants.`n")
BtnSend := MyGui.Add("Button", "x10 y+10 w80", "Send()")
BtnSendText := MyGui.Add("Button", "xp+85 yp w80", "SendText()")
BtnSendInput := MyGui.Add("Button", "xp+85 yp w80", "SendInput()")
BtnSendPlay := MyGui.Add("Button", "xp+85 yp w80", "SendPlay()")
BtnSendEvent := MyGui.Add("Button", "xp+85 yp w80", "SendEvent()")

; Added 3/24/23, taken from line 871 roughly
MyScLabel := MyGui.Add("Text", "x10 y+10 w300", "Get screenclip at 100, 100, 200, 200`nSave to 'MyScreenClip.png' on Desktop`& display for 2 seconds.")
MyScLabel.SetFont("s8 cBlue")
MyScBtn := MyGui.Add("Button", "w200 h25 x10 y+10", "Press to get screenclip").OnEvent("Click", "LoadSC")
; End of moving it

BtnSend.OnEvent("Click", "BtnSendFunc")
BtnSendText.OnEvent("Click", "BtnSendTextFunc")
BtnSendInput.OnEvent("Click", "BtnSendInputFunc")
BtnSendPlay.OnEvent("Click", "BtnSendPlayFunc")
BtnSendEvent.OnEvent("Click", "BtnSendEventFunc")

; ┌────────────────────────────────────┐
; │  Send and Hotkey button functions  │
; └────────────────────────────────────┘

BtnSendFunc(){   
	TheSendMsg := "
(
From the AHK docs:

"Sends simulated keystrokes and mouse clicks to the active window."
		
When you dismiss this button,
Keysharp will send 'Sincerely, John Smith'
(no quotes) to the Edit, then add a newline.
)"

	MsgBox(TheSendMsg, "Send")
	WinActivate(MyGui)
	ControlFocus(MySendEdit)
	Send("{Ctrl}{End}{Enter}")
	Send("Sincerely, John Smith`n")
}


BtnSendTextFunc(){

	TheSendTextMsg := "
(
From the AHK docs:

SendText: Similar to Send, except that all characters
in Keys are interpreted and sent literally. 
See Text mode for details.

The Text mode can be either enabled with {Text}, SendText or ControlSendText,
which is similar to the Raw mode, except that no attempt is made to translate
characters (other than ``r, ``n, ``t and ``b) to keycodes;
instead, the fallback method is used for all of the remaining characters. 

For SendEvent, SendInput and ControlSend, this improves reliability
because the characters are much less dependent on correct modifier state.

This mode can be combined with the Blind mode to avoid releasing any modifier keys:
		
		Send "{Blind}{Text}your text". 
		
However, some applications require that the modifier keys be released.

``n, ``r and ``r``n are all translated to a single Enter, unlike the default behavior and Raw mode,
which translate ``r``n to two Enter. ``t is translated to Tab and ``b to Backspace,
but all other characters are sent without translation.

Like the Blind mode, the Text mode ignores SetStoreCapsLockMode (that is, the state of CapsLock is not changed)
and does not wait for Win to be released. This is because the Text mode
typically does not depend on the state of CapsLock and cannot trigger the system Win+L hotkey.
However, this only applies when Keys begins with {Text} or {Blind}{Text}.
		
		When you dismiss this button,
		Keysharp will open Notepad, wait a bit and
		then send some text. You should see this:

I want to send some {Blind}{Text} with SendText followed by a newline.

and then a newline.

Then, you should see:

You should see the Blind mode syntax after the ellipses ... '{Blind}'
)"
	
	
	MsgBox(TheSendTextMsg, "SendText")

TheSendText := "I want to send some {Blind}{Text} with SendText followed by a newline.`r`n"
Run("Notepad.exe")
WinWaitActive("ahk_exe Notepad.exe")
;Sleep(500)
SendText(TheSendText)
Sleep(500)
Send("{Text}You should see the Blind mode syntax after the ellipses ... '{Blind}'")
Sleep(2000)
Send("{Alt}fx{Tab}{Enter}")
}

BtnSendInputFunc(){

	TheSendInputMsg := "
(
From the AHK docs:

SendInput is generally the preferred method to send keystrokes and mouse clicks because of its superior speed and reliability. 
Under most conditions, SendInput is nearly instantaneous, even when sending long strings. Since SendInput is so fast, 
it is also more reliable because there is less opportunity for some other window to pop up unexpectedly and intercept the keystrokes. 
Reliability is further improved by the fact that anything the user types during a SendInput is postponed until afterward.

Unlike the other sending modes, the operating system limits SendInput to about 5000 characters
(this may vary depending on the operating system's version and performance settings). 
Characters and events beyond this limit are not sent.

	Note: SendInput ignores SetKeyDelay because the operating system does not support a delay in this mode. 
	However, when SendInput reverts to SendEvent under the conditions described below, it uses SetKeyDelay -1, 0
	(unless SendEvent's KeyDelay is -1,-1, in which case -1,-1 is used). 
	When SendInput reverts to SendPlay, it uses SendPlay's KeyDelay.

If a script other than the one executing SendInput has a low-level keyboard hook installed, SendInput automatically reverts
to SendEvent (or SendPlay if SendMode "InputThenPlay" is in effect). 
This is done because the presence of an external hook disables all of SendInput's advantages,
making it inferior to both SendPlay and SendEvent. However, since SendInput is unable to detect
a low-level hook in programs other than AutoHotkey v1.0.43+,
it will not revert in these cases, making it less reliable than SendPlay/Event.

When SendInput sends mouse clicks by means such as {Click}, and CoordMode "Mouse", "Window"
or CoordMode "Mouse", "Client" is in effect, every click will be relative to the window
that was active at the start of the send. Therefore, if SendInput intentionally activates another window
(by means such as alt-tab), the coordinates of subsequent clicks within the same function
will be wrong if they were intended to be relative to the new window rather than the old one.
		
		When you dismiss this button,
		Keysharp will send some text to the Edit. You should see this:

Now how did this get up here???
The buttons below this Edit will use various Send() variants.

Really, Cheeta, you shouldn't have
Lord Greystoke


Testing newlines with braces syntax

)"

	MsgBox(TheSendInputMsg, "SendInput")
	WinActivate(MyGui)
	ControlFocus(MySendEdit)
	SendInput("{End}{Enter}")
	SendInput("Really, Cheeta, you shouldn't have!{End}{Enter}Lord Greystoke`n")
	Sleep(1000)
	ControlFocus(MySendEdit)
	SendInput("^{Home}")
	SendInput("Now how did this get up here???`n")
	SendInput("^{End}{Enter}")
	SendInput("^{End}{Enter}")
	SendInput("Testing newlines with braces syntax")

}

BtnSendPlayFunc(){
	TheSendPlayMsg := "
(
Warning: SendPlay may have no effect at all if UAC is enabled, even if the script is running as an administrator. For more information, refer to the FAQ.

SendPlay's biggest advantage is its ability to "play back" keystrokes and mouse clicks in a broader variety of games than the other modes. For example, a particular game may accept hotstrings only when they have the SendPlay option.

Of the three sending modes, SendPlay is the most unusual because it does not simulate keystrokes and mouse clicks per se. Instead, it creates a series of events (messages) that flow directly to the active window (similar to ControlSend, but at a lower level). Consequently, SendPlay does not trigger hotkeys or hotstrings.

Like SendInput, SendPlay's keystrokes do not get interspersed with keystrokes typed by the user. Thus, if the user happens to type something during a SendPlay, those keystrokes are postponed until afterward.

Although SendPlay is considerably slower than SendInput, it is usually faster than the traditional SendEvent mode (even when KeyDelay is -1).

Both Win (LWin and RWin) are automatically blocked during a SendPlay if the keyboard hook is installed. This prevents the Start Menu from appearing if the user accidentally presses Win during the send. By contrast, keys other than LWin and RWin do not need to be blocked because the operating system automatically postpones them until after the SendPlay (via buffering).

SendPlay does not use the standard settings of SetKeyDelay and SetMouseDelay. Instead, it defaults to no delay at all, which can be changed as shown in the following examples:

SetKeyDelay 0, 10, "Play"  ; Note that both 0 and -1 are the same in SendPlay mode.
SetMouseDelay 10, "Play"

SendPlay is unable to turn on or off CapsLock, NumLock, or ScrollLock. Similarly, it is unable to change a key's state as seen by GetKeyState unless the keystrokes are sent to one of the script's own windows. Even then, any changes to the left/right modifier keys (e.g. RControl) can be detected only via their neutral counterparts (e.g. Control). Also, SendPlay has other limitations described on the SendMode page.

Unlike SendInput and SendEvent, the user may interrupt a SendPlay by pressing Ctrl+Alt+Del or Ctrl+Esc. When this happens, the remaining keystrokes are not sent but the script continues executing as though the SendPlay had completed normally.

Although SendPlay can send LWin and RWin events, they are sent directly to the active window rather than performing their native operating system function. To work around this, use SendEvent. For example, SendEvent "#r" would show the Start Menu's Run dialog.
)"
	MsgBox(TheSendPlayMsg, "SendPlay")
	SendPlay("#r")
	MsgBox("Just sent '#r' with SendPlay, which should not work.`nNow I'll use SendEvent(), which should.`nI'll wait five seconds, then send Alt-F4 to kill the run dialog.", "SendPlay Testing", "T5")
	SendEvent("#r")
	Sleep(5000)
	SendEvent("!{F4}")
}

BtnSendEventFunc(){
	TheSendEventMsg := "
(
From the AHK docs:

"SendEvent: SendEvent sends keystrokes using the Windows keybd_event function.
(search MSDN for details)
The rate at which keystrokes are sent is determined by SetKeyDelay. 
SendMode can be used to make Send synonymous with SendEvent or SendPlay."
		
When you dismiss this button,
Keysharp will send Win-R.
The 'Run' dialog will open.
)"
	MsgBox(TheSendEventMsg, "SendEvent button")
	SendEvent("#r")
}

; ┌─────────────────────────┐
; │  HOTKEY() TEST SECTION  │
; └─────────────────────────┘

MyGui.Add("Text", "x0 y+20 w700", "_____________________________________________________________________________________________________________")
HotkeySectionTopText := MyGui.Add("Text", "x10 y+5 w600", "HOTKEY TESTS`nHold F1 to slow mouse, release to restore.")
HotkeySectionTopText.SetFont("cBlue s14")
FuncBtnOne := MyGui.Add("Button", "x10 y+5", "FuncObj")
FuncBtnOne.OnEvent("Click", "DoTricks")

FuncBtnTwo := MyGui.Add("Button", "x90 yp", "RCtrl and RShift -> AltTab")
FuncBtnTwo.OnEvent("Click", "StupidTrickTwo")

FuncBtnThree := MyGui.Add("Button", "x250 yp", "Hotkey Off")
FuncBtnThree.OnEvent("Click", "StupidTrickThree")

FuncBtnFour := MyGui.Add("Button", "x340 yp", "RCtrl and RShift -> AltTab`nwith FuncObj")
FuncBtnFour.OnEvent("Click", "FuncObjTest")

FuncBtnFive := MyGui.Add("Button", "x10 y+30 w150", "Toggle AltTab Hotkey`nOn or Off")
FuncBtnFive.OnEvent("Click", "ToggleHotkey")

FuncBtnSix := MyGui.Add("Button", "x170 yp w150", "From .INI`nRCtrl+LShift = AltTab")
FuncBtnSix.OnEvent("Click", "GrabFromIni")

FuncBtnSeven := MyGui.Add("Button", "x340 yp w150", "Toggle Hotkey from .INI")
FuncBtnSeven.OnEvent("Click", "ToggleFromIni")

; ┌────────────────────┐
; │  Hotkey functions  │
; └────────────────────┘

RealFn(a, b, c:="c") {
	MsgBox(a ", " b, "A bound function test")
}
	
DoTricks() {
	RealFn := FuncObj("RealFn")

	fn := RealFn.Bind(1)  ; Bind first parameter only
	fn(2)      ; Shows "1, 2"
	fn.Call(3) ; Shows "1, 3"

	fn := RealFn.Bind( , 1)  ; Bind second parameter only
	fn(2)      ; Shows "2, 1"
	fn.Call(3) ; Shows "3, 1"
	;fn(, 4)    ; Error: 'a' was omitted
}

StupidTrickTwo() {
	Hotkey("RCtrl & RShift", "AltTab")
}

StupidTrickThree() {
	Try 
	{
			Hotkey("RCtrl & RShift", "Off")
			MsgBox("Hotkey RCtrl & RShift -> AltTab is Off", "Hotkey Off", "T2")
	}
	Catch 
	{
		MsgBox("Set the Hotkey first!")
	}
}

RealFn2(TheMessage) {
	Hotkey("RCtrl & RShift", TheMessage)
	MsgBox(TheMessage)
}

FuncObjTest() {
	RealFn2 := FuncObj("RealFn2")
	fn2 := RealFn2.Bind("AltTab")
	fn2()
}

ToggleHotkey() {
	Try 
	{
		Hotkey("RCtrl & RShift", "Toggle")
	}
	Catch
	{
		MsgBox("Set the AltTab hotkeyfirst!", "ERROR", "T2")
	}

}

GrabFromIni() {
	HotkeyVal := IniRead("hotkeyini_1.ini", "HotkeyToRead", "Key")
	Hotkey(HotkeyVal, "AltTab")
}

ToggleFromIni() {
	Try 
	{
		Hotkey("RCtrl & LShift", "Toggle")
	}
	Catch
	{
		MsgBox("Set the .INI hotkeyfirst!", "ERROR", "T2")
	}
}
#endif
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
	global origBackColor := MyGui.BackColor
	MsgBox(MyGui.BackColor, "Background color:")
	MyGui.BackColor := GuiBGColor
}
; ┌───────────────────────────────┐
; │  Restore background function  │
; └───────────────────────────────┘

RestoreBG()
{
	global MyGui, origBackColor
	MyGui.BackColor := origBackColor
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
	TrayTip("TrayTipTest", "I will see myself out, thanks!", "Icon!")
	Sleep(1000)
	HideTrayTip()
}

SetChecked()
{
	ControlSetChecked(true, RadioThree, MyGui)
	ControlSetChecked(true, CheckBoxOne, MyGui)
}

; ┌─────────────────┐
; │  TreeView Edit  │
; └─────────────────┘
MyTreeView_Edit(TV, Item) {
	;MsgBox("Sort Not Implemented", "Men at Work")
	TV.Modify(TV.GetParent(Item), "Sort")  ; This works even if the item has no parent.
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

EditVar := "
(
A line of text.
By default, the hard carriage return (Enter) between the previous line and this one will be stored.
	This line is indented with a tab; by default, that tab will also be stored.
"Quote marks" are now automatically escaped when appropriate.
)"

SendTextToEdit() {
	;MsgBox(EditVar)
	global EditVar
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
	;MsgBox(EditVar)
	; ControlSetText(EditVar, SecondRichEdit)
	global EditVar
	SecondRichEdit.Value := EditVar
}

SendRtfToRichEdit()
{
	RawRichEditVar := "
(
{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081\deflangfe3081{\fonttbl{\f0\fswiss\fprq2\fcharset0 Calibri;}}
{\colortbl ;\red0\green0\blue255;\red5\green99\blue193;}
{\*\generator Riched20 10.0.19041}{\*\mmathPr\mnaryLim0\mdispDef1\mwrapIndent1440 }\viewkind4\uc1 
\pard\widctlpar\sa160\sl252\slmult1\qc {\f0\fs22\lang2057{\field{\*\fldinst{HYPERLINK "https://github.com/dotnet/winforms/issues/146" }}{\fldrslt{\ul\cf1\cf2\ul Example}}}}\f0\fs22\lang2057  Document\par
\par

\pard\widctlpar\fi-360\li360\sa160\sl252\slmult1 1.\tab Section Title {{\field{\*\fldinst{HYPERLINK "http://www.google.com" }}{\fldrslt{\ul\cf1\cf2\ul www.google.com}}}}\f0\fs22  \par

\pard\widctlpar\fi-432\li792\sa160\sl252\slmult1 1.1.\tab  Some stuff\par

\pard\widctlpar\fi-504\li1224\sa160\sl252\slmult1 1.1.1.\tab  Some stuff\rquote s thing\par

\pard\widctlpar\fi-432\li792\sa160\sl252\slmult1 1.2.\tab  More stuff\par

\pard\widctlpar\fi-360\li360\sa160\sl252\slmult1 2.\tab Next Section\par

\pard\widctlpar\fi-432\li792\sa160\sl252\slmult1 2.1.\tab Other stuff\par

\pard\widctlpar\li720\sa160\sl252\slmult1\par
}
)"
SecondRichEdit.RichText := RawRichEditVar
}

; ┌───────────────────────┐
; │  Clear Edit Callback  │
; └───────────────────────┘
ClearRichEdit() {
	ControlSetText(, SecondRichEdit)
}

GetLineCount()
{
	MsgBox("Regular edit line count: " . EditGetLineCount(SecondEdit, MyGui) . "`nRich edit line count: " . EditGetLineCount(SecondRichEdit, MyGui))
}

; ┌───────────┐
; │  LoadPic  │
; └───────────┘

MyFirstPic := ""
Monkey := A_ScriptDir . A_DirSeparator . "monkey.ico"

LoadPic() {
	global
	local x, y, w, h
	Tab.UseTab("Second")
	
	if (MyFirstPic = "")
		MyFirstPic := MyGui.Add("Picture", "x400 y650 w100 h-1 border", Monkey)
	else
		MyFirstPic.Value := Monkey
	
	Sleep(2000)
	MyFirstPic.Value := ""
	Tab.UseTab()
	; MyGui.Opts("+Redraw")
}

#if WINDOWS
DestroyPic()
{
	DllCall("DestroyWindow", "Ptr", MyFirstPic.Hwnd)
}
#endif


; ┌────────────────────┐
; │  Listbox Callback  │
; └────────────────────┘

ListBoxClicked() {
	; MsgBox(MyListBox.Text, "ListBox")
	;MySB.SetIcon("Shell32.dll", 2)
	; MsgBox("Icon lives at " . A_KeysharpCorePath)
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
#if WINDOWS
	MyVertProgress.Value -= 10
	ProgressStatusText.Value := "Values: " . MyProgress.Value . " " . MyVertProgress.Value
#else
	ProgressStatusText.Value := "Value: " . MyProgress.Value
#endif
}

Pbtn2Clicked() {
	MyProgress.Value += 10
#if WINDOWS
	MyVertProgress.Value += 10
	ProgressStatusText.Value := "Values: " . MyProgress.Value . " " . MyVertProgress.Value
#else
	ProgressStatusText.Value := "Values: " . MyProgress.Value
#endif
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

UpdateOSD()
{
	mx :=
	my := 0
	msx := 0
	msy :=
	prevMode := A_CoordModeMouse
	MouseGetPos(&mx, &my)
	CoordMode("Mouse", "Screen")
	MouseGetPos(&msx, &msy)
	CoordMode("Mouse", prevMode)
	CoordText.Text := ("X: " mx " Y: " my . " (" . msx . ", " . msy . ")")
}

; ┌────────────────────────────┐
; │  GroupBox Tab - Functions  │
; └────────────────────────────┘

SendToGB3() {
GB3Text := "
(
This uses 'ControlSetText' from a button in GroupBox 4 to populate this edit.
The first message box shows the Hwnd of this Edit.
The second message box shows '1' (True) if GuiCtrlFromHwnd created an
object from the Hwnd.
Finally, ControlSetText operates on the Object created from the Hwnd.
)"
	MsgBox(gb3Hwnd, "Hwnd of Groupbox 3 Edit")
	obj := GuiCtrlFromHwnd(gb3Hwnd)
	Result := IsObject(obj)
	MsgBox(Result, "If '1', the control is an Object")
	ControlSetText(GB3Text, obj)
}

ClearGB3() {
	ControlSetText(, gb3Edit)
}

StartEditToolTip() {
ToolTipText := "
(
This uses 'ControlSetText' from a button in GroupBox 4 to populate this edit.
The first message box shows the Hwnd of this Edit.
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
#if WINDOWS
 	ControlSetStyle("+0x8", HwndMyEdit)
#else
	MyEdit2.Opt("+Uppercase")
	HwndMyEdit := MyEdit2.Hwnd
#endif
	ControlFocus(HwndMyEdit)
}

Reset_Edit_Style()
{
#if WINDOWS
	Str := ControlGetStyle(HwndMyEdit)
	MsgBox(Format("0x{1:x}", Str), "Style of Edit1 Before Reset")

	ControlSetStyle("-0x8", HwndMyEdit)
	ControlFocus(HwndMyEdit)
	
	Str := ControlGetStyle(HwndMyEdit)
	MsgBox(Format("0x{1:x}", Str), "Style of Edit1 After Reset")
#else
	MyEdit2.Opt("-Uppercase")
	HwndMyEdit := MyEdit2.Hwnd
	ControlFocus(HwndMyEdit)
#endif
}

; ┌──────────────────────┐
; │  Move Gui functions  │
; └──────────────────────┘

MoveGui() {
	global winposx, winposy, winposw, winposh, gb2_TabTwo
	Tab.UseTab("Second")
	MyGui.UseGroup(gb2_TabTwo)
	guiToUse := MoveEdit.Text != "" ? MoveEdit.Text : MyGui ; Either search for the text the user entered, or just use this Gui directly.
	WinGetPos(&winposx, &winposy, &winposw, &winposh, guiToUse)
	WinMove(100, 100, 200, 200, guiToUse)
	; MyGui.Move(100, 100, 200, 200)
}

MoveGuiBack() {
	global winposx, winposy, winposw, winposh, gb2_TabTwo
	Tab.UseTab("Second")
	MyGui.UseGroup(gb2_TabTwo)
	guiToUse := MoveEdit.Text != "" ? MoveEdit.Text : MyGui
	WinMove(winposx, winposy, winposw, winposh, guiToUse)
	; MyGui.Move(winposx, winposy, winposw, winposh)
}

; ┌──────────────────────────┐
; │  Image Search functions  │
; └──────────────────────────┘

ImgSrch() {
CoordMode("Pixel", )  ; Interprets the coordinates below as relative to the screen rather than the active window.

	try 
	{
	resultX :=
	resultY := 0
	ImageSearch(&resultX, &resultY, 0, 0, A_ScreenWidth, A_ScreenHeight, "killbill.png")

	If (resultX != "")
		MsgBox("Found at x: " resultX " y: " resultY, "Image Search")
	Else
		MsgBox("Image not found!", "FAILURE")
	}

	catch as e  ; Handles the first error thrown by the block above.
	{
		MsgBox("An error was thrown!`nSpecifically: " e.Message)
		Exit
	}
}

#if WINDOWS
; ┌──────────────────────────┐
; │  Hotkeys with DllCall()  │
; └──────────────────────────┘

F1::
F1 up::
{
	static SPI_GETMOUSESPEED := 0x70
	static SPI_SETMOUSESPEED := 0x71
	static OrigMouseSpeed := 0

    switch ThisHotkey
    {
    case "F1":
        ; Retrieve the current speed so that it can be restored later:
        DllCall("SystemParametersInfo", "UInt", SPI_GETMOUSESPEED, "UInt", 0, "Ptr*", &OrigMouseSpeed, "UInt", 0)
        ; Now set the mouse to the slower speed specified in the next-to-last parameter (the range is 1-20, 10 is default):
        DllCall("SystemParametersInfo", "UInt", SPI_SETMOUSESPEED, "UInt", 0, "Ptr", 3, "UInt", 0)
        KeyWait("F1") ; This prevents keyboard auto-repeat from doing the DllCall repeatedly.
        
    case "F1 up":
        DllCall("SystemParametersInfo", "UInt", SPI_SETMOUSESPEED, "UInt", 0, "Ptr", OrigMouseSpeed, "UInt", 0)  ; Restore the original speed.
    }
}

; ┌──────────────────┐
; │  Dll & COM Tab   │
; └──────────────────┘

MyGui.UseGroup()
Tab.UseTab("Dll & COM")

hideCursorDllLabel := MyGui.Add("Text", "w400 x10 y+10 cBlue S10","Press Win+C to hide the cursor, and press again to restore it.")

dllMsgBoxBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll MsgBox()")
dllMsgBoxBtn.OnEvent("Click", "DllMsgBox")

dllMsgBoxBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll IsWindowVisible() (run notepad then click this)")
dllMsgBoxBtn.OnEvent("Click", "DllIsWindowVisible")

dllWsprintfBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll wsprintf()")
dllWsprintfBtn.OnEvent("Click", "DllWsprintf")

dllPerformanceCounterBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll QueryPerformanceCounter()")
dllPerformanceCounterBtn.OnEvent("Click", "DllPerformanceCounter")

dllDllGetWindowRectBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll GetWindowRect()")
dllDllGetWindowRectBtn.OnEvent("Click", "DllGetWindowRect")

dllDllFillRectBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll FillRect()")
dllDllFillRectBtn.OnEvent("Click", "DllFillRect")

dllDllRemoveFromTaskbarBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "Dll DeleteFromTaskbar() (clear for 3 seconds, then re-add)")
dllDllRemoveFromTaskbarBtn.OnEvent("Click", "DllDeleteFromTaskbar")

comDllRemoveFromTaskbarBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "COM DeleteFromTaskbar() (clear for 3 seconds, then re-add)")
comDllRemoveFromTaskbarBtn.OnEvent("Click", "ComDeleteFromTaskbar")

comDllRunWordBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "COM run MS Word")
comDllRunWordBtn.OnEvent("Click", "ComRunWord")

comDllRunWordListenerBtn := MyGui.Add("Button", "x10 y+10 AutoSize", "COM run MS Word with event listener")
comDllRunWordListenerBtn.OnEvent("Click", "ComRunWordEventListener")

DllMsgBox()
{
	WhichButton := DllCall("MessageBox", "Int", 0, "Str", "Press Yes or No", "Str", "Title of box", "Int", 4)
	MsgBox "You pressed button #" WhichButton
}

DllIsWindowVisible()
{
	DetectHiddenWindows True
	if not DllCall("IsWindowVisible", "Ptr", WinExist("Untitled - Notepad"))  ; WinExist returns an Hwnd.
		MsgBox "Notepad is not visible."
	else
		MsgBox "Notepad is visible."
	DetectHiddenWindows False
}

DllWsprintf()
{
	ZeroPaddedNumber := Buffer(20)  ; Ensure the buffer is large enough to accept the new string.
	DllCall("wsprintf", "Ptr", ZeroPaddedNumber, "Str", "%010d", "Int", 432, "Cdecl")  ; Requires the Cdecl calling convention.
	strfmt := Format("{1:0000000000}", 432)
	str := "Value from wsprintf(): " . StrGet(ZeroPaddedNumber) . "`n" . "Value from Format(): " . strfmt . "`n" . "Reference value: 0000000432"
	MsgBox(str)
}

DllPerformanceCounter()
{
	freq := 0
	CounterBefore := 0
	CounterAfter := 0

	DllCall("QueryPerformanceFrequency", "Int64*", freq)
	DllCall("QueryPerformanceCounter", "Int64*", &CounterBefore)
	Sleep(1000)
	DllCall("QueryPerformanceCounter", "Int64*", &CounterAfter)
	elapsed := (CounterAfter - CounterBefore) / freq * 1000
	MsgBox("This value should be near 1000ms: " . elapsed)
}

DllGetWindowRect()
{
	Run "Notepad"
	WinWait "Untitled - Notepad"  ; This also sets the "last found window" for use with WinExist below.
	Rect := Buffer(16)  ; A RECT is a struct consisting of four 32-bit integers (i.e. 4*4=16).
	win := WinExist()
	DllCall("GetWindowRect", "Ptr", win, "Ptr", Rect)  ; WinExist returns an Hwnd.
	L := NumGet(Rect, 0, "Int"), T := NumGet(Rect, 4, "Int")
	R := NumGet(Rect, 8, "Int"), B := NumGet(Rect, 12, "Int")
	MsgBox Format("Left: {1} Top: {2} Right: {3} Bottom: {4}", L, T, R, B)
	WinClose(win)
}

vtable(ptr, n) {
    ; NumGet(ptr, "ptr") returns the address of the object's virtual function
    ; table (vtable for short). The remainder of the expression retrieves
    ; the address of the nth function's address from the vtable.
    return NumGet(NumGet(ptr, "ptr"), n*A_PtrSize, "ptr")
}

DllFillRect()
{
	Rect := Buffer(16)  ; Set capacity to hold four 4-byte integers.
	NumPut( "Int", 0                  ; left
		  , "Int", 0                  ; top
		  , "Int", A_ScreenWidth//2   ; right
		  , "Int", A_ScreenHeight//2  ; bottom
		  , Rect)
	hDC := DllCall("GetDC", "Ptr", 0, "Ptr")  ; Pass zero to get the desktop's device context.
	hBrush := DllCall("CreateSolidBrush", "UInt", 0x0000FF, "Ptr")  ; Create a red brush (0x0000FF is in BGR format).
	DllCall("FillRect", "Ptr", hDC, "Ptr", Rect, "Ptr", hBrush)  ; Fill the specified rectangle using the brush above.
	DllCall("ReleaseDC", "Ptr", 0, "Ptr", hDC)  ; Clean-up.
	DllCall("DeleteObject", "Ptr", hBrush)  ; Clean-up.
}

DllDeleteFromTaskbar()
{
	IID_ITaskbarList  := "{56FDF342-FD6D-11d0-958A-006097C9A090}"
	CLSID_TaskbarList := "{56FDF344-FD6D-11d0-958A-006097C9A090}"

	; Create the TaskbarList object.
	tbl := ComObject(CLSID_TaskbarList, IID_ITaskbarList)

	activeHwnd := WinExist("A")

	DllCall(vtable(tbl.ptr,3), "ptr", tbl)                     ; tbl.HrInit()
	DllCall(vtable(tbl.ptr,5), "ptr", tbl, "ptr", activeHwnd)  ; tbl.DeleteTab(activeHwnd)
	Sleep 3000
	DllCall(vtable(tbl.ptr,4), "ptr", tbl, "ptr", activeHwnd)  ; tbl.AddTab(activeHwnd)

	; Non-wrapped interface pointers must be manually freed.
	ObjRelease(tbl.ptr)
}

ComDeleteFromTaskbar()
{
	IID_ITaskbarList  := "{56FDF342-FD6D-11d0-958A-006097C9A090}"
	CLSID_TaskbarList := "{56FDF344-FD6D-11d0-958A-006097C9A090}"

	; Create the TaskbarList object.
	tbl := ComObject(CLSID_TaskbarList, IID_ITaskbarList)

	activeHwnd := WinExist("A")

	ComCall(3, tbl)                     ; tbl.HrInit()
	ComCall(5, tbl, "ptr", activeHwnd)  ; tbl.DeleteTab(activeHwnd)
	Sleep 3000
	ComCall(4, tbl, "ptr", activeHwnd)  ; tbl.AddTab(activeHwnd)

	; When finished with the object, simply replace any references with
	; some other value (or if its a local variable, just return):
	tbl := ""
}

ComRunWord()
{
	wd := ComObject("Word.Application")
	wd.Visible := "True"
	WinMaximize("ahk_exe winword.exe")
	doc := wd.Documents.Add()
	wd.Selection.TypeText("Hi Keysharp!")
}

handlerobj := ""

ComRunWordEventListener()
{
	global handlerobj := mycomhandler()
	wd := ComObject("Word.Application")
	ComObjConnect(wd, handlerobj, true)
	wd.Visible := "True"
	WinMaximize("ahk_exe winword.exe")
	doc := wd.Documents.Add()
	wd.Selection.TypeText("Keysharp should receive events from this.")
}

class mycomhandler
{
    WindowActivate(obj1, obj2, comobj)
    {
		OutputDebug("`tReceived WindowActivate event.")
		ShowDebug()
    }

    WindowDeactivate(obj1, obj2, comobj)
    {
		OutputDebug("`tReceived WindowDeactivate event.")
		ShowDebug()
    }

	NewDocument(obj1, comobj)
	{
		OutputDebug("`tReceived NewDocument event.")
		ShowDebug()
	}
	
    DocumentChange(comobj)
    {
		OutputDebug("`tReceived DocumentChange event.")
		ShowDebug()
    }

    WindowSize(obj1, obj2, comobj)
    {
		OutputDebug("`tReceived WindowSize event.")
		ShowDebug()
    }

    DocumentBeforeClose(obj1, obj2, comobj)
    {
		OutputDebug("`tReceived DocumentBeforeClose event.")
		ShowDebug()
    }

    Quit(comobj)
    {
		OutputDebug("`tReceived Quit event.")
		ShowDebug()
    }
}

OnExit (*) => SystemCursor("Show")  ; Ensure the cursor is made visible when the script exits.

#c::SystemCursor("Toggle")  ; Win+C hotkey to toggle the cursor on and off.

SystemCursor(cmd)  ; cmd = "Show|Hide|Toggle|Reload"
{
    static visible := true, c := Map()
    static sys_cursors := [32512, 32513, 32514, 32515, 32516, 32642
                         , 32643, 32644, 32645, 32646, 32648, 32649, 32650]
    if (cmd = "Reload" or !c.Count)  ; Reload when requested or at first call.
    {
        for i, id in sys_cursors
        {
            h_cursor  := DllCall("LoadCursor", "Ptr", 0, "Ptr", id)
            h_default := DllCall("CopyImage", "Ptr", h_cursor, "UInt", 2
                , "Int", 0, "Int", 0, "UInt", 0)
            h_blank   := DllCall("CreateCursor", "Ptr", 0, "Int", 0, "Int", 0
                , "Int", 32, "Int", 32
                , "Ptr", Buffer(32*4, 0xFF)
                , "Ptr", Buffer(32*4, 0))
            c[id] := {def: h_default, blank: h_blank}
        }
    }
    switch cmd
    {
    case "Show": visible := true
    case "Hide": visible := false
    case "Toggle": visible := !visible
    default: return
    }
    for id, handles in c
    {
        h_cursor := DllCall("CopyImage"
            , "Ptr", visible ? handles.def : handles.blank
            , "UInt", 2, "Int", 0, "Int", 0, "UInt", 0)
        DllCall("SetSystemCursor", "Ptr", h_cursor, "UInt", id)
    }
}
#endif

; ┌──────────────────┐
; │  Sound Tab       │
; └──────────────────┘

MyGui.UseGroup()
Tab.UseTab("Sound")

#if WINDOWS
	audioMeter := SoundGetInterface("{C02216F6-8C67-4B5B-9D00-D008E73E0064}")
#endif

txtMasterName := MyGui.Add("Text", "xp y+10 w400", "Master: " . SoundGetName())

txtMasterVol := MyGui.Add("Text", "xp y+10 w200", "Volume: " . SoundGetVolume())
txtMasterMute := MyGui.Add("Text", "xp y+10 w200", "Muted: " . SoundGetMute())
#if WINDOWS
	txtMasterPeak := MyGui.Add("Text", "xp y+10 w200", "Peak: " . MasterPeak())
#endif
btnMasterMute := MyGui.Add("Button", "xp y+10", "Mute")
btnMasterUnmute := MyGui.Add("Button", "xp y+10", "Unmute")
btnMasterRefresh := MyGui.Add("Button", "xp y+10", "Refresh")

btnMasterMute.OnEvent("Click", "MasterMute")
MasterMute()
{
	SoundSetMute(true)
}

btnMasterUnmute.OnEvent("Click", "MasterUnmute")
MasterUnmute()
{
	SoundSetMute(false)
}

btnMasterRefresh.OnEvent("Click", "RefreshSound")
RefreshSound()
{
	txtMasterName.Text := "Master: " . SoundGetName()
	txtMasterVol.Text := "Volume: " . SoundGetVolume()
	txtMasterMute.Text := "Muted: " . SoundGetMute()
#if WINDOWS
	txtMasterPeak.Text := "Peak: " . MasterPeak()
#endif
}

txtMasterVolumeSlider := MyGui.Add("Text", "x10 cBlue s10", "Moving slider sets master volume")
sldMasterVolume := MyGui.Add("Slider", "xp y+10 +AltSubmit Page10 ToolTip Range0-100", 100)
sldMasterVolume.OnEvent("Change", "MasterVolumeSliderPos")

MasterVolumeSliderPos()
{
	val := sldMasterVolume.Value
	SoundSetVolume(val)
	txtMasterVol.Text := "Volume: " . SoundGetVolume()
}

txtAdjMasterVolumeSlider := MyGui.Add("Text", "x10 cBlue s10", "Moving slider adjusts master volume")
sldAdjMasterVolume := MyGui.Add("Slider", "xp y+10 +AltSubmit Page10 ToolTip Range-100-100", 100)
sldAdjMasterVolume.OnEvent("Change", "AdjustMasterVolumeSliderPos")

AdjustMasterVolumeSliderPos()
{
	val := sldAdjMasterVolume.Value
	
	if (val >= 0)
		val := "+" . val

	SoundSetVolume(val)
	txtMasterVol.Text := "Volume: " . SoundGetVolume()
}

#if WINDOWS
MasterPeak()
{
	global audioMeter
	ComCall 3, audioMeter, "float*", &peak := 0
	return peak
}
#endif

beepBtn := MyGui.Add("Button", "xp y+10", "Beep")
beepBtn.OnEvent("Click", "DoBeep")

DoBeep()
{
	SoundBeep(1500, 1000)
}

wavBtn := MyGui.Add("Button", "xp y+10", "Play wav")
wavBtn.OnEvent("Click", "DoWav")

wavTxt := MyGui.Add("Edit", "x+10 yp+2 w400")

#if LINUX
	wavTxt.Text := "/usr/share/sounds/linuxmint-login.wav"
#else
	wavTxt.Text := "C:\Windows\Media\Windows Shutdown.wav"
#endif

DoWav()
{
	SoundPlay(wavTxt.Text, 1)
}

MyGui.Show("Autosize")