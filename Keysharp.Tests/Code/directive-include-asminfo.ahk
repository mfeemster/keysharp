#include "directive-header-asminfo.ahk"

if (A_AsmTitle == "This is a title!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmDescription == "This is a description!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmConfiguration == "This is a config!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmCompany == "This is a company!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmProduct == "This is a product!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmCopyright == "This is a copyright!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmTrademark == "This is a trademark!")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_AsmVersion == "9.8.7.6")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"