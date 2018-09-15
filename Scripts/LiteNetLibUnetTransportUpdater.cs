using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LiteNetLibUnetTransportUpdater : MonoBehaviour
{
    void Update()
    {
        if (NetworkManager.activeTransport is LiteNetLibUnetTransport)
        {
            (NetworkManager.activeTransport as LiteNetLibUnetTransport).PollEvents();
        }
    }
}
