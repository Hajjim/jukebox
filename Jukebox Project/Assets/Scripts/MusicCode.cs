using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Audio;
using System.IO;

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
    private int numbermusic=0;
    WWW url;

    // ajout pour prendre dans repertoire
    private FileInfo[] files;
    // string path = "./"; // chemin RELATIF d'où l'application COMPILEE tourne. Pas le mode editeur sur unity. (donc quand j'aurai l'apk, ça se trouvera sur l'endroit même)
    // -----
    static string path = "C:/Users/hajji/OneDrive/Projects/"; // C:\Users\hajji\Documents\
    string[] fileList = Directory.GetFiles(path, "*.wav", SearchOption.TopDirectoryOnly);
    AudioClip musicAJouer;

    void Start()
    {
        source = GetComponent<AudioSource>(); //création d'un composant Audiosource
        //Play(); //pour démarrer directement
        TimerGraphique.enabled = false; //j'empêche de toucher le slider
        GameObject container = GameObject.Find("Elements");
        // --------------------------
        NetworkServer.Listen(25000);
        NetworkServer.RegisterHandler(888, ServerReceiveMessage);
        // -------------------------

        /* if (Application.isEditor)
         {
             path = "C:/Users/hajji/OneDrive/Projects/"; // C:\Users\hajji\Documents\
         }*/
        /* DirectoryInfo info = new DirectoryInfo(path);
         files = info.GetFiles();*/

        //Charge liste string

        foreach (string chemin in fileList)
        {
            Debug.Log(chemin.Replace(path, "").Replace(".wav", ""));
            GameObject item = Instantiate(itemPrefab) as GameObject;
            item.name = Path.GetFileNameWithoutExtension(chemin);
            item.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(chemin);
            item.transform.parent = container.transform;
        }

        // StartCoroutine("playMusic(numbermusic)");
        playMusic(numbermusic);

        // --------------------------


    }

   /* IEnumerator playMusic(int i)
    {
        url = new WWW("file://" + fileList[i]);

        musicAJouer = url.GetAudioClip(false);
        yield return url;
        //useless pour le 3D donc false

        //while (musicAJouer.loadState != AudioDataLoadState.Loaded){}

        //musicAJouer = url.GetAudioClip(false); 
        //string[] parts = path.Split('\\'); 
        musicAJouer.name = Path.GetFileNameWithoutExtension(fileList[i]); //sans l'extension .wav
        Play(musicAJouer);
    }*/
    private void playMusic(int i)
    {
        WWW url = new WWW("file://" + fileList[i]);
        musicAJouer = url.GetAudioClip(false);//useless pour le 3D donc false
    
        while (musicAJouer.loadState != AudioDataLoadState.Loaded){}
        //musicAJouer = url.GetAudioClip(false); 
        //string[] parts = path.Split('\\'); 
        musicAJouer.name = Path.GetFileNameWithoutExtension(fileList[i]); //sans l'extension .wav
        Play(musicAJouer);       
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
        //la musique jouer sera la derniere ajouté?!
        //actuelMusic--;
        /*if (actuelMusic < 0)
        {   //boucle la music
            actuelMusic = clips.Count - 1; //au lieu de Length, j'utilise count car liste 
        }*/
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
       /* actuelMusic++; //incrémentation pour savoir où on se situe dans la liste
        //dans le cas où j'arrive à la fin de la liste
        if (actuelMusic > clips.Count - 1)
        {
            actuelMusic = 0;
        }*/
        music.UnloadAudioData(); //désallouer
        //StartCoroutine("playMusic(k)");
        playMusic(k);

       /* source.clip = music;
        //Play sur l'audio source (appartient à unity et donc différent de ma méthode Play())
        source.Play(); // PLAY de UNITY
        //j'affiche le titre
        ShowTitreClip(k);
        ShowPlayTime(music);
        //je remet le slider à nul en définissant bien le max du slider
        TimerGraphique.maxValue = music.length;
        TimerGraphique.value = 0;
        //ma coroutine
        StartCoroutine("WaitMusicEnd");*/
    }


    //méthode pour afficher le titre du clip
    void ShowTitreClip(int k)
    {
        clipTitre.text = Path.GetFileNameWithoutExtension(fileList[k]);
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


   
}
