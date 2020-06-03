# DeviceCommunicator

This project implements a module used by the KeePass plugin to communicate
with the FIDO device using [libfido2](https://github.com/Yubico/libfido2).
To build it, download libfido2 and put its header and dll files
 into `include` directory in this project, and then use the included cmake configuration.
