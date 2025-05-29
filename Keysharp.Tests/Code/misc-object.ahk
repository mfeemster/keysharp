x := 1
a := [10, 20, 30]

if (HasMethod(a, "Contains"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"	

if (HasMethod(a, "CoNtAiNs")) ; test case insensitive once.
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"	

if (HasMethod(a, "Clear"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (HasMethod(a, "RemoveAt"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (HasMethod(a, "Push"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (HasMethod(a, "Pop"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.HasMethod("Contains"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"	
	
if (a.HasMethod("CoNtAiNs")) ; test case insensitive once.
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"	

if (a.HasMethod("Clear"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (a.HasMethod("RemoveAt"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (a.HasMethod("Push"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (a.HasMethod("Pop"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := a.GetMethod("Clear")
fo()

if (a.Length == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := [10, 20, 30]
fo := GetMethod(a, "Clear")
fo()

if (a.Length == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := [10, 20, 30]
fo := GetMethod(a, "RemoveAt")
fo(1)

if (a.Length == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a = [20, 30])
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

obj := Map(1, "a", "b", 2)
punk := ObjPtr(obj)
ObjAddRef(punk), count := ObjRelease(punk)

if (count == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

got := ObjFromPtr(punk)

if (got is Map)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

ObjAddRef(punk), count := ObjRelease(punk)

if (count == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

obj2 := Map(1, "a", "b", 2)
punk := ObjPtrAddRef(obj2) ; returns a raw pointer with the ref count being 1 initially
ObjAddRef(punk), count := ObjRelease(punk)

if (count == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

punk2 := ObjPtrAddRef(obj2)
ObjAddRef(punk2), count := ObjRelease(punk2)

if (count == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"