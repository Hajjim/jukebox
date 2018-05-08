using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Audio;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

[RequireComponent(typeof(AudioSource))]
public class MusicCode : MonoBehaviour
{

    public GameObject itemPrefab; //ma liste d'élements clips

    public AudioSource source; //ma source music

    //public AudioClip[] clips; //les musiques contenant les info 
    public List<AudioClip> clips = new List<AudioClip>(); //les musiques contenant les info 
    public Slider TimerGraphique;

    public Text clipTitre;
    public Text clipActuelTime;
    public Text clipTotalTime;

    private int MusicLongueur; //longueur total de l'audio
    private int playTimer;
    private int secondes;
    private int minutes;
    private int numbermusic = 0;
    //private int id = 0;
    WWW url;

    // string path = "./"; // chemin RELATIF d'où l'application COMPILEE tourne. Pas le mode editeur sur unity. (donc quand j'aurai l'apk, ça se trouvera sur l'endroit même)
    // -----
    static string path = "C:/Users/hajji/OneDrive/Projects"; // C:\Users\hajji\Documents\
    string[] fileList = Directory.GetFiles(path, "*.wav", SearchOption.TopDirectoryOnly); //affichage par ordre alphabétique
    string listSongsNames;
    AudioClip musicAJouer;
   

    public int port = 6321;
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;
    private TcpListener server;
    private bool serverStarted;

    Dictionary<string, int> voteCount;


    void Start()
    {
        
        source = GetComponent<AudioSource>(); //création d'un composant Audiosource
        TimerGraphique.enabled = false; //j'empêche de toucher le slider
        GameObject container = GameObject.Find("Elements");

        voteCount = new Dictionary<string, int>();
        // --------------------------

        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Serveur a demarré sur le port " + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }

        // -------------------------

        //Charge liste string
        foreach (string chemin in fileList)
        {
            Debug.Log(chemin.Replace(path, "").Replace(".wav", ""));
            GameObject item = Instantiate(itemPrefab) as GameObject;
            var title = Path.GetFileNameWithoutExtension(chemin);
            item.name = title;
            item.GetComponentInChildren<Text>().text = title;
            voteCount.Add(title, 0); //pour le dico
            listSongsNames += title + ";";
            item.transform.parent = container.transform;
        }

        StartCoroutine(loadAudio(numbermusic));

    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    // Update is called once per frame
    void Update()
    {
        if (!serverStarted)
            return;

        foreach (ServerClient c in clients)
        {
            //Est-ce que un client est encore connecté ?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // Check pour le message venant du client
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                        switch (data)
                        { 
                            //case "connected":
                            //    foreach (TcpClient item in listConnectedClients)
                            //    {
                            //        SendMessage(item);
                            //    }
                            //    msg = null;
                            //    break;
                            default:
                                if (voteCount.ContainsKey(data))
                                {
                                    voteCount[data] += 1;
                                }
                                Debug.Log(voteCount);
                                data = null;
                                break;
                        }
                    }                        
                }             
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (listSongsNames != null)
            {
                Broadcast(listSongsNames, clients);               
                Debug.Log("Envoie de : " + listSongsNames);
            }
            else {
                Debug.Log("Echec de l'envoie de : " + listSongsNames);
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast(disconnectList[i].clientName + " a été déconnecté(e)", clients);

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }

        foreach (string chemin in fileList)
        {
            var title = Path.GetFileNameWithoutExtension(chemin);
            var item = GameObject.Find(title);
            item.GetComponentInChildren<Text>().text = title + " -    +" + voteCount[title].ToString();
        }
    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//

    private void Broadcast(string data, List<ServerClient> cl) //pour l'envoie de message à all
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message + " to client " + c.clientName);
            }
        }
    }
    //---------------------------------------------------------------------------//
    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("%NAME"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast(c.clientName + " s'est connecté(e) ", clients);
            return;
        }
        Debug.Log(c.clientName + " a envoyé le vote suivant : " + data);
        Broadcast(c.clientName + " a envoyé le vote suivant : " + data, clients); //renvoi à tout les clients
    }
    //---------------------------------------------------------------------------//
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead)) //pour déterminer l'état du socket avec Read (voir doc)
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0); 
                }
                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    //---------------------------------------------------------------------------//
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        //Envoie un message à tout le monde, dit quand quelqu'un est connecté
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
        Broadcast(listSongsNames, clients);
        Debug.Log("Envoie de : " + listSongsNames);
    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    //pour chaque client
    public class ServerClient
    {
        public TcpClient tcp;
        public string clientName;

        public ServerClient(TcpClient clientSocket)
        {
            clientName = "Guest";
            tcp = clientSocket;

        }
    }
    //---------------------------------------------------------------------------//
    //---------------------------------------------------------------------------//
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);

    }


    /***************************************************************************************************************************************************/
    /***************************************************************************************************************************************************/
    IEnumerator loadAudio(int i)
    {
        if (fileList.Length > 0)
        {
            WWW url = new WWW("file://" + fileList[i]);
            yield return url;
            musicAJouer = url.GetAudioClip(false);
            musicAJouer.name = Path.GetFileNameWithoutExtension(fileList[i]); //sans l'extension .wav
            Play(musicAJouer);
        }
    }

    //Partie Coroutine :
    /* Un coroutine est une fonction qui a cette possibilité d’être « mise en pause » à un
    certain endroit de l’exécution (et rendre la main à Unity) et reprise plus tard, au calcul de
    la frame suivante ou après un certain temps.*/
    IEnumerator WaitMusicEnd()  //Une co-routine renvoie toujours un type spécial : IEnumarator
    {

        while (source.isPlaying)
        {
            TimerGraphique.value += Time.deltaTime; //utilisation du deltatime pour incrémenter dans le slider
            if (TimerGraphique.value >= musicAJouer.length) // && numbermusic < fileList.Length
            {
                numbermusic++;
                numbermusic = numbermusic % fileList.Length; // 1 2 3 ... 1 2 3 ... 1 2 3 ... (opti)         
                Suivant(musicAJouer, numbermusic);
            }
            playTimer = (int)source.time; //temps depuis le début pour cette même musique
            //ShowPlayTime();
            yield return null; //C’est au niveau de l’instruction yield que la pause s’effectue.
        }
        numbermusic++;
        numbermusic = numbermusic % fileList.Length; // 1 2 3 ... 1 2 3 ... 1 2 3 ... (opti)    
        Suivant(musicAJouer, numbermusic);
    }



    public void Play(AudioClip music)
    {
        source.clip = music;
        //slider taille total en fonction de l'audiosource
        //TimerGraphique.maxValue = source.clip.length
        TimerGraphique.maxValue = music.length;
        //slider remis au début
        TimerGraphique.value = 0;
        //Play sur l'audio source (appartient à unity et donc différent de ma méthode Play())
        source.Play();
        //j'affiche le titre
        ShowTitreClip(numbermusic);
        ShowPlayTime(music);
        //ma coroutine
        StartCoroutine("WaitMusicEnd");

    }

    public void Suivant(AudioClip music, int k)
    {
        source.Stop(); //j'arrête la source
        StopCoroutine("WaitMusicEnd"); //je met un stop sur la coroutine
        music.UnloadAudioData(); //désallouer
        StartCoroutine(loadAudio(numbermusic));
    }


    //méthode pour afficher le titre du clip
    void ShowTitreClip(int k)
    {
        var title = Path.GetFileNameWithoutExtension(fileList[k]);
        clipTitre.text = title;
    }

    //méthode pour afficher le temps en minute seconde
    void ShowPlayTime(AudioClip music)
    {
        MusicLongueur = (int)music.length;
        secondes = playTimer % 60;
        minutes = (playTimer / 60) % 60;
        clipTotalTime.text = ((MusicLongueur / 60) % 60) + ":" + (MusicLongueur % 60).ToString("D2"); //affichage deux decimal
        //ensuite j'appel ma variable minute seconde qui utilise le playTimer = temps déjà joué
        clipActuelTime.text = minutes + ":" + secondes.ToString("D2");
    }
    /***************************************************************************************************************************************************/
    /***************************************************************************************************************************************************/




    //private void HandleClientWorker(object token)
    //{
    //    Byte[] bytes = new Byte[1024];

    //    using (var client = token as TcpClient)
    //    using (var stream = client.GetStream()) 
    //    {
    //        Debug.Log("Nouveau Client connecté");
    //        int length;
    //        // Read incomming stream into byte arrary. 						
    //         while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
    //         {
    //          var incommingData = new byte[length];
    //          Array.Copy(bytes, 0, incommingData, 0, length);
    //          // Convert byte array to string message. 							
    //          string clientMessage = Encoding.ASCII.GetString(incommingData);
    //          Debug.Log(clientMessage);
    //          msg = clientMessage;
    //         }
    //        if (connectedTcpClient == null)
    //        {
    //            return;
    //        }
    //    }
    //  //  ThreadPool.QueueUserWorkItem(this.SendMessage, connectedTcpClient);
    //}



    //private void SendMessage(object token)
    //{
    //   if (connectedTcpClient == null)
    //   {
    //        Debug.Log("Problem connectedTCPClient null");
    //        return;
    //   }

    //    using (var client = token as TcpClient)       		
    //    using (var stream = client.GetStream()) // Get a stream object for writing. 
    //    {
    //        if (stream.CanWrite)
    //        {
    //            try
    //            {
    //                // Convert string message to byte array.                 
    //                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(listSongsNames);
    //                // Write byte array to socketConnection stream.               
    //                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
    //                Debug.Log("Server sent his message - should be received by client");

    //            }
    //            catch (SocketException socketException)
    //            {
    //                Debug.Log("Socket exception: " + socketException);
    //            }
    //        }
    //    }
    // }


   


}
