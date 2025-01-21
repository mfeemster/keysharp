class myclass
{
	static a := unset
	static b := ""
	static c := "asdf"
	static x := 123
	static y := x
	static arr := [1, 2, 3]
	static m := {one : 1, two : 2, three : 3}
}

classobj := myclass.Call()

If (myclass.a == unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (myclass.b == "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (myclass.c == "asdf")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (myclass.x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (myclass.y == myclass.x)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
myclass.x := 456

If (myclass.x == 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (myclass.y == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
classobj2 := myclass.Call()

If (myclass.x == 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
classobj3 := myclass()

If (classobj3.x == 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 1

If (myclass.a == unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

myclass.a := 123

If (a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (myclass.arr is Array && myclass.arr.Length == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (myclass.m is KeysharpObject && myclass.m.one == 1 && myclass.m.two == 2 && myclass.m.three == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; test static member initialized in a complex way.
class TypeSizeMapper {
	static NumTypeSize := this.MapInit()
	
	static MapInit()
	{
		temp := Map()
		for t in [
			[1,  'Int8' ,  'char' ],
			[1, 'UInt8' , 'uchar' ],
			[2,  'Int16',  'short'],
			[2, 'UInt16', 'ushort'],
			[4,  'Int32',  'int'  ],
			[4, 'UInt32', 'uint'  ],
			[8,  'Int64',  'int64'],
			[8, 'UInt64', 'uint64'],
			[4, 'Single', 'float' ],
			[8, 'Double', 'double'],
			[A_PtrSize, 'IntPtr', 'ptr'],
			[A_PtrSize, 'UIntPtr', 'uptr']
			] {
				temp[t[3]] := t[1]
			}

		return temp
	}
}

val := TypeSizeMapper.NumTypeSize["char"]

If (val == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := TypeSizeMapper.NumTypeSize["int64"]

If (val == 8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := TypeSizeMapper.NumTypeSize["ptr"]

If (val == A_PtrSize)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; do the same, but using static __Init()
class TypeSizeMapper2 {
	static NumTypeSize := ""
	
	static __Init()
	{
global
		this.NumTypeSize := Map()
		for t in [
			[1,         'Int8' ,   'char'  ],
			[1,         'UInt8' ,  'uchar' ],
			[2,         'Int16',   'short' ],
			[2,         'UInt16',  'ushort'],
			[4,         'Int32',   'int'   ],
			[4,         'UInt32',  'uint'  ],
			[8,         'Int64',   'int64' ],
			[8,         'UInt64',  'uint64'],
			[4,         'Single',  'float' ],
			[8,         'Double',  'double'],
			[A_PtrSize, 'IntPtr',  'ptr'   ],
			[A_PtrSize, 'UIntPtr', 'uptr'  ]
		] {
			this.NumTypeSize[t[3]] := t[1]
		}
	}
}

val := TypeSizeMapper2.NumTypeSize["char"]

If (val == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := TypeSizeMapper2.NumTypeSize["int64"]

If (val == 8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := TypeSizeMapper2.NumTypeSize["ptr"]

If (val == A_PtrSize)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class sclass1
{
	static c2 := sclass2()
}

class sclass2
{
	x := 1
}

sc1 := sclass1()

If (sc1.c2.x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (sclass1.c2.x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
