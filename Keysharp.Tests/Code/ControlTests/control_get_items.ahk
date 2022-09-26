ThisGui := Gui(,"Get Items Test")
ThisLb := ThisGui.Add("ListBox", "x10 w100 yp Multi", ["Orange","Purple","Fuchsia","Lime","Aqua"])
IndexText := ThisGui.Add("Text", "x10 y+10 cBlue s10 w400", "ListBox items:")
ThisIndexText := ThisGui.Add("Text", "x10 y+10 cRed", "")
UpdateOSD()  ; Make the first update immediate rather than waiting for the timer.
ThisGui.Show("w280 h220")

UpdateOSD()
{
    ThisIndexText.Text := ""
    IndexText.SetFont("bold s8")
    LbItems := ControlGetItems(ThisLb)
    ArrayText := ""
    loop LbItems.Length {
        ArrayText .= LbItems[A_Index] "`r`n"
    }
    ThisIndexText.Text := ArrayText
}