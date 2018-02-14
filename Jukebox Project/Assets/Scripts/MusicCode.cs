using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;



[RequireComponent(typeof(AudioSource))]
public class MusicCode : MonoBehaviour {

    public GameObject itemPrefab; //ma liste d'élements clips

    public AudioSource source; //ma source music

    public AudioClip[] clips; //les musiques contenant les info 
    private int actuelMusic;
    public Slider TimerGraphique;

    public Text clipTitre;
    public Text clipActuelTime;
    public Text clipTotalTime;

    private int MusicLongueur; //longueur total de l'audio
    private int playTimer;
    private int secondes;
    private int minutes;


    
    void Start()
    {
        source = GetComponent<AudioSource>(); //création d'un composant Audiosource
        actuelMusic = 0; 
        Play(); //pour démarrer directement
        TimerGraphique.enabled = false; //j'empêche de toucher le slider
        GameObject container = GameObject.Find("Elements");
        foreach (AudioClip clip in clips)
        {
            Debug.Log(clip.name);
            GameObject item = Instantiate(itemPrefab) as GameObject;
            item.name = clip.name;
            item.GetComponentInChildren<Text>().text = clip.name;
            item.transform.parent = container.transform;
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
            if (TimerGraphique.value >= source.clip.length)
            {
                Suivant();

            }
            playTimer = (int)source.time; //temps depuis le début pour cette même musique
            ShowPlayTime();
            yield return null; //C’est au niveau de l’instruction yield que la pause s’effectue.
        }
        Suivant();
    }


    public void Play()
    {
            //la musique jouer sera la derniere ajouté?!
            //actuelMusic--;
            if (actuelMusic < 0) 
            {   //boucle la music
                actuelMusic = clips.Length - 1;
            }
            source.clip = clips[actuelMusic];
            //slider taille total en fonction de l'audiosource
            TimerGraphique.maxValue = source.clip.length;
            //slider remis au début
            if (TimerGraphique.value == 0)
            {
                TimerGraphique.value = 0;
            }
            //Play sur l'audio source (appartient à unity et donc différent de ma méthode Play())
            source.Play();
            //j'affiche le titre
            ShowTitreClip();
            //ma coroutine
            StartCoroutine("WaitMusicEnd");
        
    }

    public void Suivant()
    {
        source.Stop(); //j'arrête la source
        StopCoroutine("WaitMusicEnd"); //je met un stop sur la coroutine
        actuelMusic++; //incrémentation pour savoir où on se situe dans la liste
        //dans le cas où j'arrive à la fin de la liste
        if (actuelMusic > clips.Length - 1)
        {
            actuelMusic = 0;
        }
        source.clip = clips[actuelMusic];
        //Play sur l'audio source (appartient à unity et donc différent de ma méthode Play())
        source.Play();
        //j'affiche le titre
        ShowTitreClip();
        //je remet le slider à nul en définissant bien le max du slider
        TimerGraphique.maxValue = source.clip.length;
        TimerGraphique.value = 0;
        //ma coroutine
        StartCoroutine("WaitMusicEnd");
    }


    //méthode pour afficher le titre du clip
    void ShowTitreClip()
    {
        clipTitre.text = source.clip.name;
        MusicLongueur = (int)source.clip.length; //longueur de la musique attribué à MusicLongueur
    }

    //méthode pour afficher le temps en minute seconde
    void ShowPlayTime()
    {
        secondes = playTimer % 60;
        minutes = (playTimer / 60) % 60;
        clipTotalTime.text = ((MusicLongueur / 60) % 60) + ":" + (MusicLongueur % 60).ToString("D2"); //affichage deux decimal
        //ensuite j'appel ma variable minute seconde qui utilise le playTimer = temps déjà joué
        clipActuelTime.text = minutes + ":" + secondes.ToString("D2");
    }
}
