MyGui := Gui(, "Get Positions")
Btn1 := MyGui.Add("Button", "x10 y10", "X = 10 Y = 10")
Btn2 := MyGui.Add("Button", "x10 y50", "X = 10 Y = 50")
Btn3 := MyGui.Add("Button", "x10 y90 h40", "X = 10 Y = 90")

BtnShow := MyGui.Add("Button", "x10 y130", "Get positions")
BtnShow.OnEvent("Click", "GetPos")

MyGui.Show()

GetPos() {
    pos1 := ControlGetPos("X = 10 Y = 10", "Get Positions")
    pos2 := ControlGetPos("X = 10 Y = 50", "Get Positions")
    pos3 := ControlGetPos("X = 10 Y = 90", "Get Positions")
    MsgBox("X: " pos1["X"] " Y: " pos1["Y"] " Width: " pos1["Width"] " Height: " pos1["Height"], "Button 1")
    MsgBox("X: " pos2["X"] " Y: " pos2["Y"] " Width: " pos2["Width"] " Height: " pos2["Height"], "Button 2")
    MsgBox("X: " pos3["X"] " Y: " pos3["Y"] " Width: " pos3["Width"] " Height: " pos3["Height"], "Button 3")
}