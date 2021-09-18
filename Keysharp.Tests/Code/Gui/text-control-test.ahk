/*
	Text Control Test
	Author: Dinenon (Elesar)
	Date: 26-08-2021

	This script should test all functionality of the text control type.

	Remaining Properties to test
	Gui

	Remaining Methods to test
	OnNotify
	Redraw
*/
;<=====  Basic Setup  =========================================================>
testString := "
( LTrim
	Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
	tempor incididunt ut labore et dolore magna aliqua.
)"

Colors := ["Black", "Silver", "Gray", "White", "Maroon", "Red", "Purple"
			, "Fuchsia", "Green", "Lime", "Olive", "Yellow", "Navy", "Blue"
			, "Teal", "Aqua"]

Fonts := ["Arial", "Courier New", "Impact Regular", "Microsoft Sans Serif Regular"
			, "Segoe UI"]

Styles := ["norm", "bold", "italic", "strike", "underline"]

;<=====  GUI  =================================================================>
; Test GUI
MyGui := Gui("", "Text Control Test")
MyGui.MarginX := 5
MyGui.MarginY := 5
LblTest := MyGui.Add("Text", "W400 H300", testString)
LblTest.OnEvent("Click", LblTest_Click)
LblTest.OnEvent("DoubleClick", LblTest_DoubleClick)
BtnRemoveFocus := MyGui.Add("Button", "X5 W150", "Remove Focus")
BtnRemoveFocus.OnEvent("Click", BtnRemoveFocus_Click)
BtnSetFocus := MyGui.Add("Button", "X+5 YP W150", "Set Focus")
BtnSetFocus.OnEvent("Click", BtnSetFocus_Click)
MyGui.Show("XCenter YCenter")

; Control GUI
MyGui2 := Gui("", "Test Controls")
MyGui2.Opt("+Owner" MyGui.Hwnd)
MyGui2.Opt("+OwnDialogs")
MyGui2.MarginX := 5
MyGui2.MarginY := 5

; Get/Set Text
BtnGetContent := MyGui2.Add("Button", "W150", "Get Text")
BtnGetContent.OnEvent("Click", BtnGetContent_Click)
BtnSetContent := MyGui2.Add("Button", "X+5 YP W150", "Set Text")
BtnSetContent.OnEvent("Click", BtnSetContent_Click)
BtnRstContent := MyGui2.Add("Button", "X+5 YP W150", "Reset Text")
BtnRstContent.OnEvent("Click", BtnRstContent_Click)

; SetFont
BtnSetColor := MyGui2.Add("Button", "X5 Y+25 W150", "Set Color")
BtnSetColor.OnEvent("Click", BtnSetColor_Click)
BtnSetFont := MyGui2.Add("Button", "X+5 YP W150", "Set Font")
BtnSetFont.OnEvent("Click", BtnSetFont_Click)
BtnSetSize := MyGui2.Add("Button", "X+5 YP W150", "Set Size")
BtnSetSize.OnEvent("Click", BtnSetSize_Click)
BtnSetStyle := MyGui2.Add("Button", "X5 W150", "Set Style")
BtnSetStyle.OnEvent("Click", BtnSetStyle_Click)
BtnSetWeight := MyGui2.Add("Button", "X+5 YP W150", "Set Weight")
BtnSetWeight.OnEvent("Click", BtnSetWeight_Click)
BtnSetQuality := MyGui2.Add("Button", "X+5 YP W150", "Set Quality")
BtnSetQuality.OnEvent("Click", BtnSetQuality_Click)

; Enable & Visible
BtnSetDisable := MyGui2.Add("Button", "X5 Y+25 W150", "Disable")
BtnSetDisable.OnEvent("Click", BtnSetDisable_Click)
BtnSetEnable := MyGui2.Add("Button", "X+5 YP W150", "Enable")
BtnSetEnable.OnEvent("Click", BtnSetEnable_Click)
BtnSetHidden := MyGui2.Add("Button", "X+5 YP W150", "Hide")
BtnSetHidden.OnEvent("Click", BtnSetHidden_Click)
BtnSetVisible := MyGui2.Add("Button", "X+5 YP W150", "Show")
BtnSetVisible.OnEvent("Click", BtnSetVisible_Click)

; Focus
LblFocused := MyGui2.Add("Text", "X5 Y+25 W150", "Focused: " . LblTest.Focused)

; Name
LblName := MyGui2.Add("Text", "X5 W150", "Name: " . LblTest.Name)
BtnSetName := MyGui2.Add("Button", "X+5 YP W150", "Set Name")
BtnSetName.OnEvent("Click", BtnSetName_Click)

; Pos/Move
LblX := LblY := LblW := LblH := 0
LblTest.GetPos(&LblX, &LblY, &LblW, &LblH)
LblPos := MyGui2.Add("Text", "X5 Y+25 W150", "Pos X: " . LblX
	. " Y: " . LblY . " W: " . LblW . " H: " . LblH)
BtnMove := MyGui2.Add("Button", "X+5 YP W150", "Move")
BtnMove.OnEvent("Click", BtnMove_Click)
BtnResetPos := MyGui2.Add("Button", "X+5 YP W150", "Reset Pos")
BtnResetPos.OnEvent("Click", BtnResetPos_Click)

; Misc Properties
LblClassNN := MyGui2.Add("Text", "X5 Y+25 W150", "ClassNN: " . LblTest.ClassNN)
LblHWND := MyGui2.Add("Text", "X+5 YP W150", "HWND: " . LblTest.Hwnd)
LblType := MyGui2.Add("Text", "X+5 YP W150", "Type: " LblTest.Type)

; Exit Test
BtnExitTest := MyGui2.Add("Button", "X5 Y+25 W150", "Exit Test")
BtnExitTest.OnEvent("Click", BtnExitTest_Click)

MyGui2.Show("X" . (A_ScreenWidth/2+205) . " YCenter")
Return

;<=====  Functions  ===========================================================>
BtnGetContent_Click(GuiCtrlObj, Info){
	MsgBox LblTest.Text
	Return
}

BtnSetContent_Click(GuiCtrlObj, Info){
	LblTest.Text := InputBox("Enter new text", "Text Changer").Value
	Return
}

BtnRstContent_Click(GuiCtrlObj, Info){
	LblTest.Text := testString
	Return
}

BtnSetColor_Click(GuiCtrlObj, Info){
	LblTest.SetFont("C" . Colors[Random(1, Colors.Length)])
	Return
}

BtnSetFont_Click(GuiCtrlObj, Info){
	LblTest.SetFont(,Fonts[Random(1, Fonts.Length)])
	Return
}

BtnSetSize_Click(GuiCtrlObj, Info){
	LblTest.SetFont("S" . Random(10, 20))
	Return
}

BtnSetStyle_Click(GuiCtrlObj, Info){
	LblTest.SetFont(Styles[Random(1, Styles.Length)])
	Return
}

BtnSetWeight_Click(GuiCtrlObj, Info){
	LblTest.SetFont("W" . Random(1, 1000))
	Return
}

BtnSetQuality_Click(GuiCtrlObj, Info){
	LblTest.SetFont("Q" . Random(0, 5))
	Return
}

BtnSetDisable_Click(GuiCtrlObj, Info){
	LblTest.Enabled := False
	Return
}

BtnSetEnable_Click(GuiCtrlObj, Info){
	LblTest.Enabled := True
	Return
}

BtnSetHidden_Click(GuiCtrlObj, Info){
	LblTest.Visible := False
	Return
}

BtnSetVisible_Click(GuiCtrlObj, Info){
	LblTest.Visible := True
	Return
}

BtnSetName_Click(GuiCtrlObj, Info){
	LblTest.Name := InputBox("Enter control name", "Set Name").Value
	LblName.Text := "Name: " . LblTest.Name
	Return
}

BtnMove_Click(GuiCtrlObj, Info){
	LblTest.Move(10, 10, 400, 300)
	LblX := LblY := LblW := LblH := 0
	LblTest.GetPos(&LblX, &LblY, &LblW, &LblH)
	LblPos.Text := "Pos X: " . LblX
		. " Y: " . LblY . " W: " . LblW . " H: " . LblH
	Return
}

BtnResetPos_Click(GuiCtrlObj, Info){
	LblTest.Move(5, 5, 400, 300)
	LblX := LblY := LblW := LblH := 0
	LblTest.GetPos(&LblX, &LblY, &LblW, &LblH)
	LblPos.Text := "Pos X: " . LblX
		. " Y: " . LblY . " W: " . LblW . " H: " . LblH

	Return
}

BtnExitTest_Click(GuiCtrlObj, Info){
	ExitApp
}

LblTest_Click(GuiCtrlObj, Info){
	MsgBox "Text control clicked."
	Return
}

LblTest_DoubleClick(GuiCtrlObj, Info){
	MyGui.Opt("+OwnDialogs")
	MsgBox "Text control double clicked."
	Return
}

BtnRemoveFocus_Click(GuiCtrlObj, Info){
	BtnRemoveFocus.Focus()
	LblFocused.Text := "Focused: " . LblTest.Focused
	Return
}

BtnSetFocus_Click(GuiCtrlObj, Info){
	LblTest.Focus()
	LblFocused.Text := "Focused: " . LblTest.Focused
	Return
}
