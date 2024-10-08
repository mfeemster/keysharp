This document serves as a journal for why the linux implementation was done the way it was.

At the time of development, there existed several scattered attempts at making linux text replacers. We investigated all that were known at the time and used them as a guide for how we implemented mouse and keyboard functionality on linux. Below is an analysis of each and a discussion of how it did or did not fit into the overall goeals of Keysharp on linux.

#Alternative/example projects examined#


##espanso##
	+ Description:
	+ Language:
	+ Implementation methodology:
	+ Dependencies:
	+ Pros:
	+ Cons:
	+ Notes:
		espanso's linux keyboard detection code seems to be here: https://github.com/espanso/espanso/blob/dev/espanso-detect/src/x11/native.cpp
		Note they do Windows keyboard/mouse detection differently than AHK. They used raw WM_INPUT rather than installing a keyboard/mouse hook.
		
		
##pkeymacs##
	+ Description: xkeysnail is based on this library and implements things similarly.
	+ Language: Python
	+ Implementation methodology:
	+ Dependencies:
	+ Pros:
	+ Cons:
	+ Notes:
	
##xkeysnail##
	+ Description:
	+ Language: Python
	+ Implementation methodology:
	+ Dependencies:
	+ Pros:
	+ Cons:
	+ Notes: