MyGui := Gui()
LV := MyGui.Add("ListView", "w300 r8 Grid -LV0x10", ["Column1", "Column2", "Column3"])
loop 7
	LV.Add(, A_Index * 1, A_Index * 2, A_Index * 3)
LV.OnNotify(LVN_ITEMCHANGED := -101, LV_CountSelectedItems)
MyGui.Show()


LV_CountSelectedItems(LV, *)
{
	MyGui.Title := "[" LV.GetCount("S") " / " LV.GetCount() "]"
}
