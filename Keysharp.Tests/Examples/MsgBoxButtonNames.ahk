#SingleInstance
SetTimer("ChangeButtonNames", 50)
Result := MsgBox("Choose a button:", "Add or Delete", 4)
if (Result == "Yes")
    MsgBox("You chose Add", "Add something")
else
    MsgBox("You chose Delete", "Delete something")

ChangeButtonNames()
{
    if (!WinExist("Add or Delete"))
        return  ; Keep waiting.
    SetTimer( , 0)
    WinActivate()
    ControlSetText("&Add", "Button1")
    ControlSetText("&Delete", "Button2")
}
