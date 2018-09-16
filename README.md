# LiteNetLibUnetTransport

This is transport layer for UNET HLAPI, it is require Unity 2018.3 or above
which uses [LiteNetLib](https://github.com/RevenantX/LiteNetLib) version 0.8

## How to change transport layer for UNET HLAPI

Put following codes to anywhere before NetworkManager initialized
```
NetworkManager.activeTransport = new LiteNetLibUnetTransport();
```
