using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine;

public class Client : MonoBehaviour
{

    static NetworkClient client;
    void OnGUI()
    {
        //Graphiquement, je met le status + connexion 
        string ipaddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipaddress);
        GUI.Label(new Rect(20, Screen.height - 30, 100, 20), "Status:" + client.isConnected);

        if (!client.isConnected)
        {
            if (GUI.Button(new Rect(10, 10, 200, 180), "Connect"))
            {
                Connect();
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        client = new NetworkClient();
    }

    void Connect()
    {
        // Je met l'ipv4 ici (vérifier le wifi)
        client.Connect("172.30.40.61", 25000);
    }

    public void SendCommand(string command)
    {
        if (client.isConnected)
        {
            StringMessage msg = new StringMessage();
            msg.value = command;
            client.Send(888, msg);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
