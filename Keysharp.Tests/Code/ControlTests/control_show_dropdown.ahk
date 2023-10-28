Send("#r")  ; Open the Run dialog.
WinWaitActive("ahk_class #32770")  ; Wait for the dialog to appear.
ControlShowDropDown("ComboBox1")  ; Show the drop-down list. The second parameter is omitted so that the last found window is used.
Sleep(2000)
ControlHideDropDown("ComboBox1")  ; Hide the drop-down list.
Sleep(1000)
Send("{Esc}")  ; Close the Run dialog.