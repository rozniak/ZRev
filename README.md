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
- Various COM interfaces are loaded and then execution continues to `IZoneShell`:
  - `ZCoreM.dll` contains generic objects/interfaces like `IDataStore` and `IEventQueue`
  - `Cmnclim.dll` contains most of the exciting stuff like `IZoneShell` and `IWindowManager`
  - `ZNetM.dll` contains the core network logic and control layer, application packet data is encapsulated within control messages here (via `INetwork` and `IConnection`)
  - `Zoneclim.dll` hosts `IGameControl` and is responsible for managing game protocol state (eg. matchmaking and game ready) and loading the actual game DLL (eg. `chkr.dll`)
  - Each game has its own DLL with the game logic, like `chkdr.dll`
- The network architecture uses COM interfaces to decouple message handling from the core networking classes:
  - The control logic is mainly controlled by `CNetworkManager` (in `Cmnclim.dll`), it is the first layer in the application which deals with the 'Hi', 'FirstMsg', and 'Data' packet types
  - The program flow for networking is handled via an event queue, and handlers registered under the COM interface `IEventClient` listen to the queue for certain messages
  - The starting point for all the interesting stuff is `CNetworkManager::NetworkFunc`, which is registered as the callback for networking - based on the type of message received (Hi/FirstMsg/Data), it posts an event in the queue to be handled by another class
  - For instance, when data is received, it posts message ID `0x10003` into the queue for event clients to pick up and analyze
    - Analysis looks at an identifier for these data packets, eg. `rout` for network setup / server messages
    - `rout` messages are picked up by `CMillNetworkCore::ProcessEvent` in `Cmnclim.dll`
    - Other messages (I haven't figured this out just yet) are processed into the game - they are picked up by `CGameControl::ProcessEvent` in `Zoneclim.dll`
- Currently I am investigating `CMillNetworkCore` to see how the communication can be progressed, I think via a server 'hello' message - check `CMillNetworkCore::ProcessMessage` for the logic I'm working on
