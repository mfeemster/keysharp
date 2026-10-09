; Included twice by compat_mode_include.ahk. Each inclusion starts in the mode its includer has at that point, and its
; own #Requires ends with it.
#if NESTED_AGAIN
NestedSecondBeforeNoReturn() {
}
#Requires AutoHotkey v2.1-alpha
NestedSecondAfterNoReturn() {
}
#else
#define NESTED_AGAIN
#Requires AutoHotkey v2.1-alpha
NestedFirstNoReturn() {
}
#endif
