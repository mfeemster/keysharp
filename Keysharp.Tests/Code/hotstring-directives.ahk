#Hotstring NoMouse

if (A_DefaultHotstringNoMouse)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Reset to what it was for the sake of other tests in this class.
Hotstring("MouseReset", true)

#Hotstring EndChars -()[]{}':;"/\,.?!`n`s`t

if (A_DefaultHotstringEndChars == "-()[]{}':;`"/\,.?!`n`s`t")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	

; End char required.
newVal := false
origVal := A_DefaultHotstringEndCharRequired

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
#hotstring *

if (origVal != A_DefaultHotstringEndCharRequired)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringEndCharRequired)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	

; Case sensitivity.
newVal := true
origVal := A_DefaultHotstringCaseSensitive

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring C

if (origVal != A_DefaultHotstringCaseSensitive)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringCaseSensitive)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Case sensitivity restore to default.
newVal := false
origVal := A_DefaultHotstringCaseSensitive

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring C0

if (origVal != A_DefaultHotstringCaseSensitive)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringCaseSensitive)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Inside word.
newVal := true
origVal := A_DefaultHotstringDetectWhenInsideWord

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring ?

if (origVal != A_DefaultHotstringDetectWhenInsideWord)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringDetectWhenInsideWord)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	

; Automatic backspacing off.
newVal := false
origVal := A_DefaultHotstringDoBackspace

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring B0

if (origVal != A_DefaultHotstringDoBackspace)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringDoBackspace)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Automatic backspacing back on.
newVal := true
origVal := A_DefaultHotstringDoBackspace

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring B

if (origVal != A_DefaultHotstringDoBackspace)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringDoBackspace)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Do not conform to typed case.
newVal := false
origVal := A_DefaultHotstringConformToCase

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#hotstring C1

if (origVal != A_DefaultHotstringConformToCase)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringConformToCase)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Omit ending character.
newVal := true
origVal := A_DefaultHotstringOmitEndChar

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring O

if (origVal != A_DefaultHotstringOmitEndChar)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringOmitEndChar)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Restore ending character.
newVal := false
origVal := A_DefaultHotstringOmitEndChar

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring O0

if (origVal != A_DefaultHotstringOmitEndChar)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringOmitEndChar)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Exempt from suspend.
newVal := true
origVal := A_SuspendExempt

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring S

if (origVal != A_SuspendExempt)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_SuspendExempt)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Remove suspend exempt.
newVal := false
origVal := A_SuspendExempt

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring S0

if (origVal != A_SuspendExempt)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_SuspendExempt)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Reset on trigger.
newVal := true
origVal := A_DefaultHotstringDoReset

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring Z

if (origVal != A_DefaultHotstringDoReset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringDoReset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

		
; Restore reset on trigger.
newVal := false
origVal := A_DefaultHotstringDoReset

if (origVal != newVal)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring Z0

if (origVal != A_DefaultHotstringDoReset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newVal == A_DefaultHotstringDoReset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Send replacement text raw.
newMode := "Raw"
origMode := A_DefaultHotstringSendRaw

if (origMode == "NotRaw")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring R

if (origMode != A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newMode == A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Restore replacement text mode.
newMode := "NotRaw"
origMode := A_DefaultHotstringSendRaw

if (origMode == "Raw")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring R0

if (origMode != A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newMode == A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Send replacement text mode.
newMode := "RawText"
origMode := A_DefaultHotstringSendRaw

if (origMode == "NotRaw")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring T

if (origMode != A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newMode == A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Restore replacement text mode.
newMode := "NotRaw"
origMode := A_DefaultHotstringSendRaw

if (origMode == "RawText")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring T0

if (origMode != A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newMode == A_DefaultHotstringSendRaw)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Key delay.
newInt := 42
origInt := A_DefaultHotstringKeyDelay

if (origInt == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring K42

if (origInt != A_DefaultHotstringKeyDelay)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newInt == A_DefaultHotstringKeyDelay)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


; Priority.
newInt := 42
origInt := A_DefaultHotstringPriority

if (origInt == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring P42

if (origInt != A_DefaultHotstringPriority)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newInt == A_DefaultHotstringPriority)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
			
; Send mode Event.
newSendMode := "Event"
origSendMode := A_DefaultHotstringSendMode

if (origSendMode == "Input")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
#Hotstring SE

if (origSendMode != A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newSendMode == A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Send mode Play.
newSendMode := "Play"
origSendMode := A_DefaultHotstringSendMode

if (origSendMode == "Event")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring SP

if (origSendMode != A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (newSendMode == A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Send mode Input.
newSendMode := "Input"
origSendMode := A_DefaultHotstringSendMode

if (origSendMode == "Play")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#Hotstring SI

if (origSendMode != A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if ("InputThenPlay" == A_DefaultHotstringSendMode)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

ExitApp()
