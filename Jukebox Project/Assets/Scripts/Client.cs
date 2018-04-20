using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public Text TextList;
   // static NetworkClient client;
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private string serverMessage = "";
    #endregion

    void OnGUI()
    {
        //Graphiquement, je met le status + connexion 
        string ipaddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipaddress);
        //GUI.Label(new Rect(20, Screen.height - 30, 100, 20), "Status:" + client.isConnected);

       /* if (!client.isConnected)
        {
            if (GUI.Button(new Rect(10, 10, 200, 180), "Connect"))
            {
                Connect();
                ConnectToTcpServer();
            } 
        }*/

        if (GUI.Button(new Rect(10, 10, 250, 250), "TOUCH"))
        {
            SendMessage();
        }
    }

    // Use this for initialization
    void Start()
    {
        ConnectToTcpServer();
       // client = new NetworkClient();
    }

    //void Connect()
    //{
    //    // Je met l'ipv4 ici (vérifier le wifi)
    //    client.Connect("127.0.0.1", 25000);
    //}

    /*public void SendCommand(string command)
    {
        if (client.isConnected)
        {
            StringMessage msg = new StringMessage();
            msg.value = command;
            client.Send(888, msg);
        }
    }*/

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            Debug.Log("Client is listening");
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }
    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient("172.30.40.19", 8052);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log(serverMessage);
                        //TextList.text = serverMessage;
                    }
                   
                }
            }

        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "salut";
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        SendMessage();
    //    }
    //}

}
