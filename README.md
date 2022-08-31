# Mediator
Mediator pattern over HTTP for Blazor WASM.
Define action contract on a shared project, define action handler on server-side and fire the action from the client. The mediator will carry about all of the communication for you.

This package was designed for fast and flexible development of simultaneously developed client and server applications.

Documentation:
 - [Wiki](https://github.com/pipaslot/Mediator/wiki)
 - [Release notes and breaking changes](https://github.com/pipaslot/Mediator/wiki/Release-notes-and-breaking-changes)
 - [Basic Demo for Blazor WASM](https://github.com/pipaslot/Mediator/wiki/1.2.-Example-for-client-server-application(Blazor-WASM)-usage)

Demo: [Blazor WASM App](https://pipaslot-mediator.azurewebsites.net)

Nuget packages:
 - [Pipaslot.Mediator](https://www.nuget.org/packages/Pipaslot.Mediator/) - Core logic for in-process usage
 - [Pipaslot.Mediator.Http](https://www.nuget.org/packages/Pipaslot.Mediator.Http/) - Extension for communication across HTTP
