using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct LiteNetLibEventData
{
    public NetworkEventType eventType;
    public NetPeer netPeer;
    public byte[] data;
    public byte error;
}
