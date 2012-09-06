idTech4.net
===========

idTech4.net is a port of doom3 to C# and using XNA for rendering.


idEngine
--------

This is the port of the engine source code.

Done so far:

* core system.
* file system (implements pak processing but currently does not load files from pak's).
* partial render system.
* command system.
* cvar system.
* console.
* resource loading (decl, materials, user interfaces, scripts).
* user interface rendering and interaction code.
* partial game code.
* content management through XNA pipeline


idGame
------

This is targeted against the 1.3.1 SDK release (not the engine release).  

Before the engine was released in November, a proxy layer (written in c++) was used to load managed code and marshal engine and sdk calls.


idLib
-----

Contains shared code, this is needed by the managed c++ layer.


sdk
---

This is the proxy layer that was used before the engine source was out.  This could be cleaned up, finished and used to write a mod in c# but use the original release or one of the open source engines.