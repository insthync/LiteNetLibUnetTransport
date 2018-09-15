using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestLiteNetLibUnetTransportCharacter : NetworkBehaviour
{
    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(transform.forward);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-transform.forward);
            }
        }
    }
}
