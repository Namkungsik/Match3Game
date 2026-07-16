using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonEvent : MonoBehaviour
{
    //public GameObject ModeSelectWindow;
    public Button GameQuit;
    /*
        public void ModeSelectButton()
        {
            ModeSelectWindow.SetActive(true);
            GameQuit.interactable = false;
        }

        public void ModeSelectCancleButton()
        {
            ModeSelectWindow.SetActive(false);
            GameQuit.interactable = true;
        }*/

    public void GameQuitButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void StageModeStart()
    {
        SceneManager.LoadScene("SelectStage");
    }

    public void MarathonModeStart()
    {
        SceneManager.LoadScene("MarathonModeScene");
    }
}