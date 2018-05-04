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
    WWW url;

    // string path = "./"; // chemin RELATIF d'où l'application COMPILEE tourne. Pas le mode editeur sur unity. (donc quand j'aurai l'apk, ça se trouvera sur l'endroit même)
    // -----
    static string path = "C:/Users/hajji/OneDrive/Projects"; // C:\Users\hajji\Documents\
    string[] fileList = Directory.GetFiles(path, "*.wav", SearchOption.TopDirectoryOnly); //affichage par ordre alphabétique
    string listSongsNames;
    AudioClip musicAJouer;

    private string msg = null;


    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private Thread tcpListenerThread;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;
    #endregion

    Dictionary<string, int> voteCount;


    void Start()
    {
        voteCount = new Dictionary<string, int>();

        source = GetComponent<AudioSource>(); //création d'un composant Audiosource
        //Play(); //pour démarrer directement
        TimerGraphique.enabled = false; //j'empêche de toucher le slider
        GameObject container = GameObject.Find("Elements");
        // --------------------------
        /*NetworkServer.Listen(25000);
        NetworkServer.RegisterHandler(888, ServerReceiveMessage);*/
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();

        // -------------------------

        //Charge liste string
        foreach (string chemin in fileList)
        {
            Debug.Log(chemin.Replace(path, "").Replace(".wav", ""));
            GameObject item = Instantiate(itemPrefab) as GameObject;
            var title = Path.GetFileNameWithoutExtension(chemin);
            item.name = title;
            item.GetComponentInChildren<Text>().text = title;
            voteCount.Add(title, 0);
            listSongsNames += title + ";";
            item.transform.parent = container.transform;
        }

        //StartCoroutine("playMusic(numbermusic)");

        StartCoroutine(loadAudio(numbermusic));

        // --------------------------


    }
    /*
    private void playMusic(int i)
    {
        if (fileList.Length > 0)
        {
            WWW url = new WWW("file://" + fileList[i]);
            musicAJouer = url.GetAudioClip(false);//useless pour le 3D donc false

            while (musicAJouer.loadState != AudioDataLoadState.Loaded) { }
            musicAJouer.name = Path.GetFileNameWithoutExtension(fileList[i]); //sans l'extension .wav
            Play(musicAJouer);
        }
    }*/

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


    //Partie Réseau ----------------------------------------------------------------------------
    /*
    private void ServerReceiveMessage(NetworkMessage message)
    {
        StringMessage msg = new StringMessage();
        msg.value = message.ReadMessage<StringMessage>().value;
        Debug.Log(msg);
        Debug.Log(msg.value);
        switch (msg.value)
        {
            case "mute":
                Mute();
                break;
            case "vote":
                Vote();
                break;
        }
    }

    public void Mute()
    {
        if (source.isPlaying)
        {
            source.mute = !source.mute;
        }
    }

    
    Dictionary<string, int> voteCounts;

    public void Vote()
    {
        foreach (string music in fileList)
        {
            voteCounts[music] += 1;
        }
    }*/



    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052.
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8052);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            Debug.Log(clientMessage);
                            msg = clientMessage;
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }
    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    private void SendMessage()
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                //string serverMessage = "This is a message from your server.";
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(listSongsNames);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage();
        }
        if (msg != null)
        {
            switch (msg)
            {
                case "connected":
                    SendMessage();
                    msg = null;
                    break;
                default:
                    Debug.Log(msg);
                    if (voteCount.ContainsKey(msg))
                    {
                        voteCount[msg] += 1;
                    }
                    Debug.Log(voteCount);
                    msg = null;
                    break;
            }
        }

        foreach (string chemin in fileList)
        {
            var title = Path.GetFileNameWithoutExtension(chemin);
            var item = GameObject.Find(title);
            item.GetComponentInChildren<Text>().text = title + " -    +" + voteCount[title].ToString();
        }
    }


}
