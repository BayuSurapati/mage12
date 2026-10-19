using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private void Start()
    {
        AudioManager.Instance.MainkanBGM("Main Menu.mpeg");
    }
    public void GoToGameplay()
    {
        AudioManager.Instance.HentikanBGM();
        AudioManager.Instance.MainkanSFX("Clickkk");
        SceneManager.LoadScene("GameplayScene-MiniGames");
    }
    public void QuitGame()
    {
        AudioManager.Instance.MainkanSFX("Clickkk");
        Application.Quit();
    }
}
