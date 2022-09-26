SetTitleMatchMode(2)

If !(WinExist("Untitled - Notepad"))
    Run("Notepad.exe")
WinWait("Untitled - Notepad")
Sleep(10)
ControlSend("This is working.{Enter}", "Edit1", "Untitled - Notepad") 
ControlSendText("Notice that {Enter} is not sent as an Enter keystroke with ControlSendText.", "Edit1", "Untitled - Notepad") 

ExitApp()

