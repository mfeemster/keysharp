x := 0
y := 0
last := "0x000000"
white := "0xffffff"
black := "0x000000"
found := false
pix := ""
CoordMode("Mouse", "Screen")

Loop 100
{
	y := 0
	
    Loop 100
	{
		pix := PixelGetColor(x, y)
		
		if (pix != last && pix != white && pix != black)
			found := true

		if (found == true)
			break
			
		last = pix
		y++
	}
    
	if (found == true)
		break
		
	x++
}

if (found == true)
{
	outx := 
	outy := 0
	PixelSearch(&outx, &outy, x, y, x + 1, y + 1, pix)
	
	if (outx == x && outy == y)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}
else
  	FileAppend "fail", "*"