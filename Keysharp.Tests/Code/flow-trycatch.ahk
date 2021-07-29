;#Include %A_ScriptDir%/header.ahk

b := false

try
{
	throw Error("asdf")
}
catch
	b := true

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw Error("asdf")
}
catch
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw Error("asdf")
}
catch Error
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false
str := ""

try
{
	throw Error("tester")
}
catch Error as errex
{
	b := true
	str := errex.Message
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (str == "tester")
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw Error("tester")
}
catch Error as errex
{
}
finally
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false
str := ""

try
{
	throw Error("tester")
}
catch Error as errex
{
}
else
{
	b := true
	str := errex.Message
}

if (b == false)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (str == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw IndexError("tester")
}
catch IndexError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw IndexError("tester")
}
catch IndexError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw KeyError("tester")
}
catch KeyError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw MemberError("tester")
}
catch MemberError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw MemoryError("tester")
}
catch MemoryError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw MethodError("tester")
}
catch MethodError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw OSError(123)
}
catch OSError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw PropertyError("tester")
}
catch PropertyError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw TargetError("tester")
}
catch TargetError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw TimeoutError("tester")
}
catch TimeoutError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw TypeError("tester")
}
catch TypeError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw ValueError("tester")
}
catch ValueError as errex
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	throw ZeroDivisionError("tester")
}
catch ZeroDivisionError as errex
{
	b := true
}
catch(KeysharpException)
{
	b := false
}
catch(OSError)
{
	b := false
}
catch(IndexError)
{
	b := false
}
catch(PropertyError)
{
	b := false
}
catch(KeyError())
{
	b := false
}
catch(MemberError)
{
	b := false
}
catch(MemoryError)
{
	b := false
}
catch(MethodError)
{
	b := false
}
catch(TargetError)
{
	b := false
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *