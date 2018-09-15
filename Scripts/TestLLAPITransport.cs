using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public class TestLLAPITransport : INetworkTransport
{
    public const string TAG = "TestTransport";
    public INetworkTransport defaultTransport { get { return NetworkManager.defaultTransport; } }
    public bool IsStarted { get { return defaultTransport.IsStarted; } }
    private GlobalConfig globalConfig;

    public int AddHost(HostTopology topology, int port, string ip)
    {
        var hostId = defaultTransport.AddHost(topology, port, ip);
        Debug.Log("[" + TAG + "] added host " + hostId + " port=" + port + " ip=" + ip);
        return hostId;
    }

    public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
    {
        return AddHost(topology, port, null);
    }

    public int AddWebsocketHost(HostTopology topology, int port, string ip)
    {
        return AddHost(topology, port, ip);
    }

    public int Connect(int hostId, string address, int port, int specialConnectionId, out byte error)
    {
        var connectionId = defaultTransport.Connect(hostId, address, port, specialConnectionId, out error);
        Debug.Log("[" + TAG + "] connected to " + connectionId + " hostId=" + hostId + " address=" + address + " port =" + port + " specialConnectionId=" + specialConnectionId + " error=" + (NetworkError)error);
        return connectionId;
    }

    public void ConnectAsNetworkHost(int hostId, string address, int port, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a dedicated connection to Relay server
        Debug.LogError("[" + TAG + "] ConnectAsNetworkHost() not implemented");
        throw new System.NotImplementedException();
    }

    public int ConnectEndPoint(int hostId, EndPoint endPoint, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to the peer specified by the given C# System.EndPoint.
        return Connect(hostId, ((IPEndPoint)endPoint).Address.ToString(), ((IPEndPoint)endPoint).Port, specialConnectionId, out error);
    }

    public int ConnectToNetworkPeer(int hostId, string address, int port, int specialConnectionId, int relaySlotId, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a connection to another peer in the Relay group.
        Debug.LogError("[" + TAG + "] ConnectToNetworkPeer() not implemented");
        throw new System.NotImplementedException();
    }

    public int ConnectWithSimulator(int hostId, string address, int port, int specialConnectionId, out byte error, ConnectionSimulatorConfig conf)
    {
        // Tries to establish a connection to another peer with added simulated latency.
        Debug.LogWarning("[" + TAG + "] ConnectWithSimulator() not implemented, it will use Connect()");
        return Connect(hostId, address, port, specialConnectionId, out error);
    }

    public bool Disconnect(int hostId, int connectionId, out byte error)
    {
        var disconnected = defaultTransport.Disconnect(hostId, connectionId, out error);
        Debug.Log("[" + TAG + "] disconnect from hostId=" + hostId + " connectionId=" + connectionId + " error=" + (NetworkError)error);
        return disconnected;
    }

    public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
    {
        return false;
    }

    public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
    {
        defaultTransport.GetBroadcastConnectionInfo(hostId, out address, out port, out error);
    }

    public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        defaultTransport.GetBroadcastConnectionMessage(hostId, buffer, bufferSize, out receivedSize, out error);
    }

    public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
    {
        Debug.Log("[" + TAG + "] GetConnectionInfo()");
        defaultTransport.GetConnectionInfo(hostId, connectionId, out address, out port, out network, out dstNode, out error);
    }

    public int GetCurrentRTT(int hostId, int connectionId, out byte error)
    {
        error = 0;
        return 0;
    }

    public void Init()
    {
        Debug.Log("[" + TAG + "] Init() globalConfig=" + (globalConfig != null));
        defaultTransport.Init();
    }

    public void Init(GlobalConfig config)
    {
        globalConfig = config;
        //config.ConnectionReadyForSend += (int1, int2) => { Debug.Log("[" + TAG + "] ConnectionReadyForSend, int1=" + int1 + " int2=" + int2); };
        //config.NetworkEventAvailable += (int1) => { Debug.Log("[" + TAG + "] NetworkEventAvailable, int1=" + int1); };
        defaultTransport.Init(config);
    }

    public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        var evt = defaultTransport.Receive(out hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
        Debug.Log("[" + TAG + "] Receive hostId=" + hostId + " connectionId=" + connectionId + " channelId=" + channelId + " error=" + (NetworkError)error);
        return evt;
    }

    public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        var evt = defaultTransport.ReceiveFromHost(hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
        Debug.Log("[" + TAG + "] ReceiveFromHost hostId=" + hostId + " connectionId=" + connectionId + " channelId=" + channelId + " error=" + (NetworkError)error);
        return evt;
    }

    public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
    {
        var evt = defaultTransport.ReceiveRelayEventFromHost(hostId, out error);
        Debug.Log("[" + TAG + "] ReceiveRelayEventFromHost hostId=" + hostId + " error=" + (NetworkError)error);
        return evt;
    }

    public bool RemoveHost(int hostId)
    {
        return defaultTransport.RemoveHost(hostId);
    }

    public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
    {
        var result = defaultTransport.Send(hostId, connectionId, channelId, buffer, size, out error);
        Debug.Log("[" + TAG + "] Send hostId=" + hostId + " connectionId=" + connectionId + " channelId=" + channelId + " error=" + (NetworkError)error);
        return result;
    }

    public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
    {
        defaultTransport.SetBroadcastCredentials(hostId, key, version, subversion, out error);
    }

    public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
    {
        defaultTransport.SetPacketStat(direction, packetStatId, numMsgs, numBytes);
    }

    public void Shutdown()
    {
        defaultTransport.Shutdown();
    }

    public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
    {
        return defaultTransport.StartBroadcastDiscovery(hostId, broadcastPort, key, version, subversion, buffer, size, timeout, out error);
    }

    public void StopBroadcastDiscovery()
    {
        defaultTransport.StopBroadcastDiscovery();
    }
}
