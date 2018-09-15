using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using LiteNetLib;

public class LiteNetLibUnetTransport : INetworkTransport
{
    public const string TAG = "LiteNetLibUnetTransport";
    public bool IsStarted => throw new System.NotImplementedException();

    public int AddHost(HostTopology topology, int port, string ip)
    {
        // Creates a host based on HostTopology.
        throw new System.NotImplementedException();
    }

    public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
    {
        // Creates a host and configures it to simulate Internet latency(works on Editor and development
        // builds only).
        Debug.LogError("[" + TAG + "] AddHostWithSimulator() not implemented");
        throw new System.NotImplementedException();
    }

    public int AddWebsocketHost(HostTopology topology, int port, string ip)
    {
        // Creates a web socket host.
        Debug.LogError("[" + TAG + "] AddWebsocketHost() not implemented");
        throw new System.NotImplementedException();
    }

    public int Connect(int hostId, string address, int port, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to another peer.
        throw new System.NotImplementedException();
    }

    public void ConnectAsNetworkHost(int hostId, string address, int port, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a dedicated connection to Relay server
        error = (byte)NetworkError.Timeout;
    }

    public int ConnectEndPoint(int hostId, EndPoint endPoint, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to the peer specified by the given C# System.EndPoint.
        throw new System.NotImplementedException();
    }

    public int ConnectToNetworkPeer(int hostId, string address, int port, int specialConnectionId, int relaySlotId, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a connection to another peer in the Relay group.
        throw new System.NotImplementedException();
    }

    public int ConnectWithSimulator(int hostId, string address, int port, int specialConnectionId, out byte error, ConnectionSimulatorConfig conf)
    {
        // Tries to establish a connection to another peer with added simulated latency.
        throw new System.NotImplementedException();
    }

    public bool Disconnect(int hostId, int connectionId, out byte error)
    {
        // Sends a disconnect signal to the connected peer and closes the connection.
        throw new System.NotImplementedException();
    }

    public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
    {
        // Checks whether the specified end point uses platform-specific protocols.
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
    {
        // After INetworkTransport.Receive() returns a NetworkEventType.BroadcastEvent, this function 
        // returns the connection information of the broadcast sender. This information can then be used 
        // for connecting to the broadcast sender.
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // 	After INetworkTransport.Receive() returns NetworkEventType.BroadcastEvent, this function
        // returns a complimentary message from the broadcast sender.
        throw new System.NotImplementedException();
    }

    public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
    {
        // Returns the connection parameters for the specified connectionId. These parameters can be sent
        // to other users to establish a direct connection to this peer. If this peer is connected to the host
        // via Relay, the Relay-related parameters are set.
        throw new System.NotImplementedException();
    }

    public int GetCurrentRTT(int hostId, int connectionId, out byte error)
    {
        // Return the round trip time for the given connectionId.
        // TODO: implement this
        error = (byte)NetworkError.Ok;
        return 0;
    }

    public void Init()
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        throw new System.NotImplementedException();
    }

    public void Init(GlobalConfig config)
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        throw new System.NotImplementedException();
    }

    public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Polls the underlying system for events.
        throw new System.NotImplementedException();
    }

    public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Similar to INetworkTransport.Receive but will only poll for the provided hostId.
        throw new System.NotImplementedException();
    }

    public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
    {
        // Polls the host for the following events: NetworkEventType.ConnectEvent and
        // NetworkEventType.DisconnectEvent.
        throw new System.NotImplementedException();
    }

    public bool RemoveHost(int hostId)
    {
        // Closes the opened transport pipe, and closes all connections belonging to that transport pipe.
        throw new System.NotImplementedException();
    }

    public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
    {
        // Sends data to peer with the given connection ID.
        throw new System.NotImplementedException();
    }

    public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
    {
        // Sets the credentials required for receiving broadcast messages. If the credentials of a received
        // broadcast message do not match, that broadcast discovery message is dropped.
        throw new System.NotImplementedException();
    }

    public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
    {
        // Keeps track of network packet statistics.
        throw new System.NotImplementedException();
    }

    public void Shutdown()
    {
        // Shuts down the transport object.
        throw new System.NotImplementedException();
    }

    public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
    {
        // Starts sending a broadcasting message across all local subnets.
        throw new System.NotImplementedException();
    }

    public void StopBroadcastDiscovery()
    {
        // Stops sending the broadcast discovery message across all local subnets.
        throw new System.NotImplementedException();
    }
}
