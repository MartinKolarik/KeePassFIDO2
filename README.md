# KeePassFIDO2

A KeePass key provider plugin that allows unlocking a KeePass database using
a FIDO2 authenticator instead of a master password.

*This project was submitted as a Bachelor's Thesis at Czech Technical University
in Prague, Faculty of Information Technology, in June 2020. The submitted version
is available as a v1.0.0 release in this repository.*

The repository contains three packages:

 - `Thesis` - The thesis PDF and its source files, describing the implementation approach and architecture.
 - `KeePassPlugin` - A C# project implementing the KeePass plugin.
 - `DeviceComunnicator` - A C++ project implementing a separate executable that handles communication with FIDO devices and is used by the plugin.

**The implementation should be considered a proof-of-concept only at this stage.**
