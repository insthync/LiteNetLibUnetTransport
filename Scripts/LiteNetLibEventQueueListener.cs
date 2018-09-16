using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public class LiteNetLibEventQueueListener : INetEventListener
{
    public Queue<LiteNetLibEventData> eventQueue { get; private set; }
    public int hostId { get; private set; }
    public int specialConnectionId { get; private set; }
    public int maxConnections { get; private set; }
    private LiteNetLibUnetTransport transport;
    private LiteNetLibEventData tempEventData;
    public LiteNetLibEventQueueListener(LiteNetLibUnetTransport transport, int hostId, int specialConnectionId, int maxConnections)
    {
        this.transport = transport;
        this.hostId = hostId;
        this.specialConnectionId = specialConnectionId;
        this.maxConnections = maxConnections;
        eventQueue = new Queue<LiteNetLibEventData>();
    }
    
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.Log("OnNetworkError");
        // TODO: implement this later
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        Debug.Log("OnNetworkLatencyUpdate");
        // TODO: implement this later
    }
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        Debug.Log("OnNetworkReceive");
        tempEventData = new LiteNetLibEventData();
        tempEventData.eventType = NetworkEventType.DataEvent;
        tempEventData.netPeer = peer;
        tempEventData.data = reader.GetRemainingBytes();
        tempEventData.error = (byte)NetworkError.Ok;
        eventQueue.Enqueue(tempEventData);
        transport.UpdateHostEventListener(hostId);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Debug.Log("OnNetworkReceiveUnconnected");
        // TODO: implement this later
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("OnPeerConnected");
        tempEventData = new LiteNetLibEventData();
        tempEventData.eventType = NetworkEventType.ConnectEvent;
        tempEventData.netPeer = peer;
        tempEventData.data = null;
        tempEventData.error = (byte)NetworkError.Ok;
        eventQueue.Enqueue(tempEventData);
        transport.UpdateHostEventListener(hostId);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("OnPeerDisconnected");
        tempEventData = new LiteNetLibEventData();
        tempEventData.eventType = NetworkEventType.DisconnectEvent;
        tempEventData.netPeer = peer;
        tempEventData.data = null;
        tempEventData.error = (byte)NetworkError.Ok;
        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.ConnectionFailed:
                break;
            case DisconnectReason.DisconnectPeerCalled:
                break;
            case DisconnectReason.RemoteConnectionClose:
                break;
            case DisconnectReason.SocketReceiveError:
                break;
            case DisconnectReason.SocketSendError:
                break;
            case DisconnectReason.Timeout:
                tempEventData.error = (byte)NetworkError.Timeout;
                break;
        }
        eventQueue.Enqueue(tempEventData);
        transport.UpdateHostEventListener(hostId);
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        var host = transport.GetHost(hostId, specialConnectionId);
        if (host.PeersCount < maxConnections)
            request.Accept();
        else
            request.Reject();
    }
}
