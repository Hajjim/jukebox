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
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

public class Client : MonoBehaviour
{
    public GameObject itemPrefab; //ma liste d'élements clips
    public Text mytext;

    private GameObject container;
    private GameObject item;
    String[] musics = null;
    private bool pooled = false;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public string clientName = "Une personne mystérieuse";
    private bool ok = false;
    private GameObject Button; //pour valider
    string info; //MAC bien ecrit pour ONGUI
    string id; //MAC envoyé forma string
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    void Start()
    {
        container = GameObject.Find("Elements");
        ConnectToServer();
        id = SystemInfo.deviceUniqueIdentifier;
    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                    pooled = false;
                }
                    
            }
        }
        
        if (musics != null && musics.Length > 0 && pooled == false)
        {
            Debug.Log("Got musics");

            foreach (Transform child in container.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
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
                itemBtn.onClick.AddListener(() => SendMusic(txt)); //Envoie de la musique ici
                //  item.GetComponentInChildren<Text>().text = "0";
                item.transform.parent = container.transform;
            }
            pooled = true;
        }
       
    }
    /***************************************************************************************************************************************************/
    // ------------- RESEAU -------------------
    /***************************************************************************************************************************************************/


    void OnGUI()
    {
        //Graphiquement, je met l'ip
        //string ipaddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 320, 25), "Id : " + id);
    }
    //---------------------------------------------------------------------------//
    public void SendValider()
    {
        ok = true;
        Button = GameObject.Find("Button");
        Button.GetComponent<Button>().interactable = false;
        if(GameObject.Find("InputField").GetComponent<InputField>().text != null && ok == true)
            {
                clientName = GameObject.Find("InputField").GetComponent<InputField>().text;
            }
            Send("%NAME|" + clientName); //j'envoie le pseudo
    }
    private void OnIncomingData(string data)
    {
       
        if (data == "%NAME")
        {
            Send("%NAME|" + clientName); //j'envoie le pseudo
            return;
        }
        Debug.Log(data);
        //GameObject go = Instantiate(messagePrefab, chatContainer.transform); 
        if(data.Contains("pour") || data.Contains("...")) 
        {
            mytext.text = "...";
            mytext.text = data;
            // messagePrefab.GetComponentInChildren<Text>().text = data;
        }
        if (data.Contains(";"))
        {
            Debug.Log("Spliting....");   
            String value = data;
            value = value.Substring(0, value.Length - 1); //je delete le dernier '';'' sinon element vide de trop 
            Char delimiter = ';';
            musics = null;
            musics = value.Split(delimiter);           
            Debug.Log(musics);
        }
    }
    //---------------------------------------------------------------------------//
    public void ConnectToServer()
    {
        //Si déja connecté, ignore cette fonction
        if (socketReady)
            return;

        //Valeur par defaut pour hôte/port 
        string host = ClientStart.ipstart; //"192.168.0.15";//"127.0.0.1";  => Ipconfig pas oublié de check
        int port = 6321;
        //....... Idée : Créé un menu pour changer hôte ? Port ? A voir...

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;

        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }
    //---------------------------------------------------------------------------//

    private void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();

    }
    //---------------------------------------------------------------------------//
    //public void OnSendButton()
    //{
    //    string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
    //    Send(message);
    //}
    //---------------------------------------------------------------------------//
    
    public void SendMusic(Text music) //pour l'avoir en format Text => contenair 
    {
       
        string musica = music.text;
        Char delimiter = ':';
        string[] musicnovote = musica.Split(delimiter);

        Debug.Log(id);
        Send(musicnovote[0] + "=" + id + "=" + DateTime.Now);

    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    // On gère la deconnection 
    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;

    }
    //---------------------------------------------------------------------------//
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    //---------------------------------------------------------------------------//
    private void OnDisable()
    {
        CloseSocket();
    }
    //---------------------------------------------------------------------------//

}
