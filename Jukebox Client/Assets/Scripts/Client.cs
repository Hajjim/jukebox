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
    public GameObject itemPrefab; //ma liste d'élements clips

    private GameObject container;
    private GameObject item;
    String[] musics = null;
    private bool pooled = false;
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
            SendMsg("Hello");
        }
    }

    // Use this for initialization
    void Start()
    {
        container = GameObject.Find("Elements");
        ConnectToTcpServer();
       // client = new NetworkClient();
    }

    private void Update()
    {
        if (musics != null && musics.Length > 0 && pooled == false)
        {
            Debug.Log("Got musics");
            foreach (string music in musics)
            {
                Debug.Log(music);
                item = Instantiate(itemPrefab);
                item.SetActive(true);
                item.name = music;
                // item.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(chemin) + "     " + 0;
                Text txt = item.GetComponentInChildren<Text>();
                txt.text = music;
                Button itemBtn = item.GetComponent<Button>();
                itemBtn.onClick.AddListener(() => SendMusic(txt));
                //  item.GetComponentInChildren<Text>().text = "0";
                item.transform.parent = container.transform;
            }
            pooled = true;
        }
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
            SendMsg("connected");
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
            socketConnection = new TcpClient("127.0.0.1", 8052);
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
                        String value = serverMessage;
                        Char delimiter = ';';
                        musics = value.Split(delimiter);
                        Debug.Log(musics);
                       
                    }
                   
                }
            }

        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendMsg(string msg)
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
                string clientMessage = msg;
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

    public void SendMusic(Text music)
    {
        Debug.Log(music.text);
        SendMsg(music.text);
    }


}
