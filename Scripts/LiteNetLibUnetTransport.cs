using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using LiteNetLib;
using System.Threading;

public class LiteNetLibUnetTransport : INetworkTransport
{
    public const string TAG = "LiteNetLibUnetTransport";
    
    private int nextConnectionId = 1;
    private int tempConnectionId;
    private int nextHostId = 1;
    private int tempHostId;
    private bool isStarted;
    private Thread pollEventThread;
    private bool isPollEventRunning;
    private GlobalConfig globalConfig;
    private Dictionary<int, HostTopology> topologies = new Dictionary<int, HostTopology>();
    private Dictionary<int, Dictionary<int, NetManager>> hosts = new Dictionary<int, Dictionary<int, NetManager>>();
    private Dictionary<int, Dictionary<int, NetPeer>> connections = new Dictionary<int, Dictionary<int, NetPeer>>();
    private Dictionary<long, int> connectionIds = new Dictionary<long, int>();
    private Dictionary<int, LiteNetLibEventQueueListener> hostEventListeners = new Dictionary<int, LiteNetLibEventQueueListener>();
    private Queue<int> updatedHostEventQueue = new Queue<int>();
    private NetManager tempHost;
    private NetPeer tempPeer;
    private LiteNetLibEventQueueListener tempEventListener;

    public bool IsStarted
    {
        // True if the object has been initialized and is ready to be used.
        get { return isStarted; }
    }

    private void AddConnection(int hostId, int connectionId, NetPeer peer)
    {
        if (!connections.ContainsKey(hostId))
            connections.Add(hostId, new Dictionary<int, NetPeer>());
        connections[hostId][connectionId] = peer;
        connectionIds[peer.ConnectId] = connectionId;
    }

    private bool AddHostByConfig(int hostId, int port, string ip, int specialConnectionId, HostTopology topology)
    {
        var success = false;
        var maxConnections = topology.MaxDefaultConnections;
        var config = topology.DefaultConfig;
        if (specialConnectionId > 0)
            config = topology.SpecialConnectionConfigs[specialConnectionId - 1];

        // Create new host with its event listener
        tempEventListener = new LiteNetLibEventQueueListener(this, hostId, specialConnectionId, maxConnections);
        tempHost = new NetManager(tempEventListener);
        tempHost.PingInterval = (int)config.PingTimeout;
        tempHost.DisconnectTimeout = (int)config.DisconnectTimeout;
        tempHost.ReconnectDelay = (int)config.ConnectTimeout;
        tempHost.MaxConnectAttempts = (int)config.MaxConnectionAttempt;

        if (specialConnectionId > 0)
        {
            if (tempHost.Start())
                success = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(ip))
            {
                if (tempHost.Start(IPAddress.Parse(ip), IPAddress.IPv6Any, port))
                    success = true;
            }
            else if (port > 0)
            {
                if (tempHost.Start(port))
                    success = true;
            }
            else
            {
                if (tempHost.Start())
                    success = true;
            }
        }

        if (success)
        {
            if (!hosts.ContainsKey(hostId))
                hosts.Add(hostId, new Dictionary<int, NetManager>());
            hosts[hostId][specialConnectionId] = tempHost;
            hostEventListeners.Add(hostId, tempEventListener);
            Debug.Log("[" + TAG + "] added host " + hostId + " port=" + port + " ip=" + ip);
        }
        else
        {
            tempHost.Stop();
            Debug.Log("[" + TAG + "] cannot add host " + hostId + " port=" + port + " ip=" + ip);
        }

        return success;
    }

    public int AddHost(HostTopology topology, int port, string ip)
    {
        // Creates a host based on HostTopology.
        tempHostId = nextHostId++;
        topologies[tempHostId] = topology;
        for (var i = 0; i < topology.SpecialConnectionConfigsCount + 1; ++i)
        {
            AddHostByConfig(tempHostId, port, ip, i, topology);
        }
        return tempHostId;
    }

    public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
    {
        // Creates a host and configures it to simulate Internet latency(works on Editor and development
        // builds only).
        Debug.LogWarning("[" + TAG + "] AddHostWithSimulator() not implemented, it will use AddHost()");
        return AddHost(topology, port, null);
    }

    public int AddWebsocketHost(HostTopology topology, int port, string ip)
    {
        // Creates a web socket host.
        Debug.LogWarning("[" + TAG + "] AddWebsocketHost() not implemented, it will use AddHost()");
        return AddHost(topology, port, ip);
    }

    public int Connect(int hostId, string address, int port, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to another peer.
        Debug.Log("[" + TAG + "] Connecting to hostId " + hostId + " address " + address + " port " + port);
        error = (byte)NetworkError.UsageError;
        tempConnectionId = 0;
        if (hosts.ContainsKey(hostId))
        {
            tempPeer = hosts[hostId][specialConnectionId].Connect(address, port, "");
            if (tempPeer != null)
            {
                tempConnectionId = nextConnectionId++;
                AddConnection(hostId, tempConnectionId, tempPeer);
                error = (byte)NetworkError.Ok;
            }
            else
            {
                Debug.LogError("[" + TAG + "] Cannot connect to hostId " + hostId + " address " + address + " port " + port);
            }
        }
        else
        {
            Debug.LogError("[" + TAG + "] Cannot connect to hostId " + hostId);
        }
        return tempConnectionId;
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
        // Sends a disconnect signal to the connected peer and closes the connection.
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return false;
        }
        if (!connections[hostId].TryGetValue(connectionId, out tempPeer))
        {
            error = (byte)NetworkError.WrongConnection;
            return false;
        }
        error = (byte)NetworkError.Ok;
        tempPeer.Disconnect();
        return true;
    }

    public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
    {
        // Checks whether the specified end point uses platform-specific protocols.
        return false;
    }

    public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
    {
        // After INetworkTransport.Receive() returns a NetworkEventType.BroadcastEvent, this function 
        // returns the connection information of the broadcast sender. This information can then be used 
        // for connecting to the broadcast sender.
        Debug.LogError("[" + TAG + "] GetBroadcastConnectionInfo() not implemented");
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // 	After INetworkTransport.Receive() returns NetworkEventType.BroadcastEvent, this function
        // returns a complimentary message from the broadcast sender.
        Debug.LogError("[" + TAG + "] GetBroadcastConnectionMessage() not implemented");
        throw new System.NotImplementedException();
    }

    public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
    {
        // Returns the connection parameters for the specified connectionId. These parameters can be sent
        // to other users to establish a direct connection to this peer. If this peer is connected to the host
        // via Relay, the Relay-related parameters are set.
        Debug.Log("[" + TAG + "] GetConnectionInfo()");
        address = "";
        port = -1;
        network = NetworkID.Invalid;
        dstNode = NodeID.Invalid;
        error = (byte)NetworkError.UsageError;
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return;
        }
        tempPeer = connections[hostId][connectionId];
        address = tempPeer.EndPoint.Address.ToString();
        port = tempPeer.EndPoint.Port;
        error = (byte)NetworkError.Ok;
    }

    public int GetCurrentRTT(int hostId, int connectionId, out byte error)
    {
        // Return the round trip time for the given connectionId.
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return 0;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return 0;
        }
        // TODO: implement this
        error = (byte)NetworkError.Ok;
        return 0;
    }

    public void Init()
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        Debug.Log("[" + TAG + "] Init() globalConfig=" + (globalConfig != null));
        if (!isPollEventRunning)
        {
            pollEventThread = new Thread(new ThreadStart(PollEventThreadFunction));
            pollEventThread.Start();
            isPollEventRunning = true;
        }
        // Init default transport to make everything works, this is HACK
        // I actually don't know what I have to do with init function
        NetworkManager.defaultTransport.Init();
        isStarted = true;
    }

    public void Init(GlobalConfig config)
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        globalConfig = config;
        Init();
    }

    public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Polls the underlying system for events.
        hostId = 0;
        connectionId = 0;
        channelId = 0;
        receivedSize = 0;
        error = (byte)NetworkError.Ok;
        if (updatedHostEventQueue.Count > 0)
            hostId = updatedHostEventQueue.Dequeue();
        else
            return NetworkEventType.Nothing;
        return ReceiveFromHost(hostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
    }

    public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Similar to INetworkTransport.Receive but will only poll for the provided hostId.
        connectionId = 0;
        channelId = 0;
        receivedSize = 0;
        error = (byte)NetworkError.Ok;

        if (!hosts.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return NetworkEventType.Nothing;
        }

        // Receive events data after poll events, and send out data
        if (!hostEventListeners.TryGetValue(hostId, out tempEventListener) || 
            tempEventListener.eventQueue.Count <= 0)
            return NetworkEventType.Nothing;

        var eventData = tempEventListener.eventQueue.Dequeue();
        error = eventData.error;
        switch (eventData.eventType)
        {
            case NetworkEventType.ConnectEvent:
                if (!connectionIds.ContainsKey(eventData.netPeer.ConnectId))
                {
                    connectionId = nextConnectionId++;
                    AddConnection(hostId, connectionId, eventData.netPeer);
                }
                break;
            case NetworkEventType.DataEvent:
                var length = eventData.data.Length;
                if (length <= bufferSize)
                {
                    System.Buffer.BlockCopy(eventData.data, 0, buffer, 0, length);
                    receivedSize = length;
                }
                else
                    error = (byte)NetworkError.MessageToLong;
                break;
        }
        connectionId = connectionIds[eventData.netPeer.ConnectId];
        return eventData.eventType;
    }

    public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
    {
        // Polls the host for the following events: NetworkEventType.ConnectEvent and
        // NetworkEventType.DisconnectEvent.
        Debug.LogError("[" + TAG + "] ReceiveRelayEventFromHost() not implemented");
        throw new System.NotImplementedException();
    }

    public bool RemoveHost(int hostId)
    {
        // Closes the opened transport pipe, and closes all connections belonging to that transport pipe.
        // Disconnection connection
        if (connections.ContainsKey(hostId))
        {
            var tempConnections = connections[hostId].ToArray();
            foreach (var entry in tempConnections)
            {
                entry.Value.Disconnect();
                connections[hostId].Remove(entry.Key);
                connectionIds.Remove(entry.Value.ConnectId);
            }
            connections[hostId].Clear();
            connections.Remove(hostId);
        }
        // Stop host
        if (hosts.ContainsKey(hostId))
        {
            var tempHosts = hosts[hostId].ToArray();
            foreach (var entry in tempHosts)
            {
                entry.Value.Stop();
                hosts[hostId].Remove(entry.Key);
            }
            hosts.Remove(hostId);
            return true;
        }
        return false;
    }

    public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
    {
        // Sends data to peer with the given connection ID.
        error = (byte)NetworkError.UsageError;
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return false;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return false;
        }
        var sendOptions = DeliveryMethod.Unreliable;
        switch (topologies[hostId].DefaultConfig.Channels[channelId].QOS)
        {
            case QosType.AllCostDelivery:
            case QosType.ReliableStateUpdate:
            case QosType.ReliableFragmentedSequenced:
                sendOptions = DeliveryMethod.ReliableOrdered;
                break;
            case QosType.ReliableSequenced:
                sendOptions = DeliveryMethod.ReliableSequenced;
                break;
            case QosType.Reliable:
            case QosType.ReliableFragmented:
                sendOptions = DeliveryMethod.ReliableUnordered;
                break;
            case QosType.StateUpdate:
            case QosType.UnreliableFragmentedSequenced:
            case QosType.UnreliableSequenced:
                sendOptions = DeliveryMethod.Sequenced;
                break;
        }
        connections[hostId][connectionId].Send(buffer, 0, size, sendOptions);
        error = (byte)NetworkError.Ok;
        return true;
    }

    public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
    {
        // Sets the credentials required for receiving broadcast messages. If the credentials of a received
        // broadcast message do not match, that broadcast discovery message is dropped.
        Debug.LogError("[" + TAG + "] SetBroadcastCredentials() not implemented");
        throw new System.NotImplementedException();
    }

    public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
    {
        // Keeps track of network packet statistics.
        NetworkManager.defaultTransport.SetPacketStat(direction, packetStatId, numMsgs, numBytes);
    }

    public void Shutdown()
    {
        // Shuts down the transport object.
        var tempHosts = hosts.Keys.ToArray();
        foreach (var hostId in tempHosts)
        {
            RemoveHost(hostId);
        }
        connections.Clear();
        connectionIds.Clear();
        hosts.Clear();
        nextConnectionId = 1;
        nextHostId = 1;
        if (isPollEventRunning)
        {
            isPollEventRunning = false;
            pollEventThread.Abort();
        }
    }

    public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
    {
        // Starts sending a broadcasting message across all local subnets.
        Debug.LogError("[" + TAG + "] StartBroadcastDiscovery() not implemented");
        throw new System.NotImplementedException();
    }

    public void StopBroadcastDiscovery()
    {
        // Stops sending the broadcast discovery message across all local subnets.
        Debug.LogError("[" + TAG + "] StopBroadcastDiscovery() not implemented");
        throw new System.NotImplementedException();
    }

    public void UpdateHostEventListener(int hostId)
    {
        updatedHostEventQueue.Enqueue(hostId);
    }

    public NetManager GetHost(int hostId, int specialConnectionId)
    {
        return hosts[hostId][specialConnectionId];
    }
    
    private void PollEventThreadFunction()
    {
        while (isPollEventRunning)
        {
            var hosts = this.hosts.Values;
            lock (hosts)
            {
                foreach (var host in hosts)
                {
                    var hostsByConfig = host.Values;
                    lock (hostsByConfig)
                    {
                        foreach (var hostByConfig in hostsByConfig)
                        {
                            hostByConfig.PollEvents();
                        }
                    }
                }
            }
            Thread.Sleep(15);
        }
    }
}
