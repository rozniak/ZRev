# ZRev - MSN Gaming Zone Protocol Reversing
Hello! This repo houses my work on reversing the protocol used by the internet games included in Windows Me and Windows XP (Internet Spades, Internet Checkers, etc.). The goal is to produce a custom server for restoring functionality for these games and document the protocol for any other uses someone might have.

## Contributing
Open an issue or something and I'll have a look. In general I suggest poking around in here, and if you reckon you can help then you'll need some tools:
- `decomp/` is where I'm storing my Ghidra repository, so hopefully you can 'Restore Project' in Ghidra and use this to catch up to how much I have reversed so far
- `ZSrv/` is the custom server, written in C# - code is crappy for now, will tidy it when more is complete
- The games look up the server via DNS, so edit your hosts file to hijack this (eg. `checkers.freegames.zone.com`)

## General Run-down
This is a brief overview of how I *think* the games run, using Internet Checkers as an example here:
- `chkzm.exe` looks for the COM server for `Zone.ClientM` and calls `Launch` on it with configuration from its resources
  - (open `chkrzm.exe` in ResHacker or something to see this)
  - the resource strings contain the DLLs to load for code (aka, COM interface implementations) and DLLs for data ('datafiles' aka they host resources)
  - resource strings also contain the domain names for the server
- `zClientm.exe` is the COM server that provides `Zone.ClientM`, after initialisation it calls `CClientCore::DoLaunch()`
- Various COM interfaces are loaded and then execution contains to `IZoneShell`:
  - `ZCoreM.dll` contains generic objects/interfaces like `IDataStore` and `IEventQueue`
  - `Cmnclim.dll` contains most of the exciting stuff like `IZoneShell` and `IWindowManager`
  - `ZNetM.dll` contains the core network logic and control layer, application packet data is encapsulated within control messages here (via `INetwork` and `IConnection`)
  - `Zoneclim.dll` hosts `IGameControl` and is responsible for loading the actual game DLL (eg. `chkr.dll`)
  - Each game has its own DLL with the game logic, like `chkdr.dll`
