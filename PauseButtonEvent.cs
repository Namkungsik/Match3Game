using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseButtonEvent : MonoBehaviour
{
    public GameObject PausePanel;
    public GameObject CheckPanel;
    public Button PauseButton;
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isPaused)
        {
            PauseButton.interactable = false;
        }
        else
        {
            PauseButton.interactable = true;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // ∞‘¿” ∏ÿ√„
        PausePanel.SetActive(true);
        isPaused = true;
    }
    
    public void StartButtonClick()
    {
        Time.timeScale = 1f; // ∞‘¿” ¿Á∞≥
        PausePanel.SetActive(false);
        CheckPanel.SetActive(false);
        isPaused = false;
    }

    public void RestartButtonClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MarathonModeScene");
    }

    public void HomeButtonClick()
    {
        PausePanel.SetActive(false);
        CheckPanel.SetActive(true);
        isPaused = true;
    }

    public void OKButtonClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        isPaused = false;
    }
}
