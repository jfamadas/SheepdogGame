using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {
    private GameObject canvasAsDog;
    private GameObject canvasAsShepherd;

    void Start()
    {
        canvasAsDog = GameObject.Find("PlayDog");
        canvasAsDog.SetActive(false);
        canvasAsShepherd = GameObject.Find("PlayShepherd");
        canvasAsShepherd.SetActive(false);
    }

    public void PlayGame()
    {
        canvasAsDog.SetActive(true);
        canvasAsShepherd.SetActive(true);
    }

    public void PlayAsDog()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayAsShepherd()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}
