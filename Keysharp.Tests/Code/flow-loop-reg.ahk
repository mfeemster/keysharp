; #Include %A_ScriptDir%/header.ahk

try
{
	RegDeleteKey("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest")
}
catch
{
}

RegWrite("ksdefval", "REG_SZ", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "")
			
if (val = "ksdefval")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RegWrite("ksval", "REG_SZ", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval")
			
if (val = "ksval")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RegWrite("stringone`nstringtwo`nstringthree", "REG_MULTI_SZ", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "")

if (val = [ "stringone", "stringtwo", "stringthree" ])
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RegWrite(1, "REG_DWORD", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1")

if (val = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RegWrite(2, "REG_QWORD", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1")

if (val = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RegWrite("AABBCCDD", "REG_BINARY", "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1")
val := RegRead("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1")

if (val.ToHex() = "AABBCCDD")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

i := 0

Loop Reg "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "kvr" ; this is a comment
{
	val := RegRead(,,"testdefault")

	if (i == 0)
	{
		if (val = "ksval")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"

		if (A_LoopRegType == "REG_SZ")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "testval")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 1)
	{
		if (val = "ksdefval")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "REG_SZ")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 2)
	{
		if (val = "testdefault")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"

		if (A_LoopRegType == "KEY")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "ks_sub2")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 3)
	{
		if (val.ToHex() = "AABBCCDD")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "REG_BINARY")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "bin1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 4)
	{
		if (val = [ "stringone", "stringtwo", "stringthree" ])
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "KEY")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "ks_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 5)
	{
		if (val = [ "stringone", "stringtwo", "stringthree" ])
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "REG_MULTI_SZ")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 6)
	{
		if (val = "testdefault")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "KEY")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "ks_sub1_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 7)
	{
		if (val = 2)
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "REG_QWORD")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "qword1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}
	else if (i == 8)
	{
		if (val = 1)
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
		
		if (A_LoopRegType == "REG_DWORD")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegName == "dword1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
			
		if (A_LoopRegKey == "HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1")
			FileAppend "pass", "*"
		else
			FileAppend "fail", "*"
	}

	i++
}

RegDelete("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest", "testval")
RegDelete("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1", "")
RegDelete("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "dword1")
RegDelete("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub1\ks_sub1_sub1", "qword1")
RegDelete("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2", "bin1")
RegDeleteKey("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest\ks_sub2")
RegDeleteKey("HKEY_CURRENT_USER\SOFTWARE\KeysharpTest")