idTech4.net
===========

idTech4.net is a port of doom3 to c#.


idEngine
--------

This is the port of the engine source code.

Done so far:

* majority of the initialization code.
* file system (implements pak processing but currently does not load files from pak's).
* partial render system initialization.
* command system.
* cvar system.
* console and some other things.

I am keeping the OGL render pipeline initially.  The reasoning is that the port is going to cause issues of it's own by moving from C++ to C# that I do not want to have to deal
with render system problems.  I want to get a stable system up and then write an alternate pipeline that uses XNA.

This should in theory mean the engine will continue to run on Windows, Linux and MacOS.


idGame
------

This is targeted against the 1.3.1 SDK release (not the engine release).  

Before the engine was released in November, a proxy layer (written in c++) was used to load managed code and marshal engine and sdk calls.

This currently does not compile as the engine port does not implement enough.


idLib
-----

Contains shared code, this was primarily used with the proxy.  Unsure whether or not this will stay.


sdk
---

This is the proxy layer that was used before the engine source was out.  This could be cleaned up, finished and used to write a mod in c# but use the original release or one of the open source engines.