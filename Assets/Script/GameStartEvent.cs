using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameStartEvent : MonoBehaviour
{
    public GameObject startPanel;
    public Text timerText;

    void Start()
    {
        StartCoroutine(GameStartCountdown());
    }

    IEnumerator GameStartCountdown()
    {
        Time.timeScale = 0f; // 게임 정지

        for (int i = 5; i > 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
            // timeScale이 0이라 Realtime 사용
        }

        timerText.text = "Start!";
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f; // 게임 시작
        startPanel.SetActive(false);
    }
}