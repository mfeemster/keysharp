::btw::by the way
; 1 -> :2
::1:::2
; 3 -> ::4
::3::::4
; 5: -> 6
::5`:::6
; 7: -> :8
::7`::::8

::text1::
(
Any text between the top and bottom parentheses is treated literally.
By default, the hard carriage return (Enter) between the previous line and this one is also preserved.
    By default, the indentation (tab) to the left of this line is preserved.
)

myfunc()
{
}

:X:mf1::myfunc

:X:mf2::{
  myfunc
}

:X:mf3::
{
  myfunc
}

::mf4::{
  myfunc
}

::mf5::
{
  myfunc
}

ExitApp()