using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientStart : MonoBehaviour {

    public static string ipstart; //ip de la jukebox

    private void Update()
    {
        ipstart = GameObject.Find("input").GetComponent<InputField>().text;
    }

    public void Connexion()
    {
        SceneManager.LoadScene(1);
    }

    public void Quitter()
    {
        Application.Quit();
    }

}
