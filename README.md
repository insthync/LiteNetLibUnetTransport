# LiteNetLibUnetTransport

This is project which make [LiteNetLib](https://github.com/RevenantX/LiteNetLib) version 0.8 as transport layer for UNET HLAPI, it is require Unity 2018.3 or above

## How to change transport layer for UNET HLAPI

Put following codes to anywhere before NetworkManager initialized
```
NetworkManager.activeTransport = new LiteNetLibUnetTransport();
```
