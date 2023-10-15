using System;
using System.Collections;

using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public int pacmanRegenCounter = 5;

    [SerializeField]
    GameObject timeUpPanel;

    [SerializeField]
    GameObject gameOverPanel;

    [SerializeField]
    GameObject gameWonPanel;

    [SerializeField]
    GameObject pacmanRegenPanel;

    [SerializeField]
    TextMeshProUGUI timeLabel;

    [SerializeField]
    TextMeshProUGUI timeText;

    [SerializeField]
    TextMeshProUGUI gameProgressLabel;

    [SerializeField]
    TextMeshProUGUI gameProgressPercentText;

    [SerializeField]
    TextMeshProUGUI pacRegDurationLabel;

    [SerializeField]
    GameObject[] pacmanLives;

    [SerializeField]
    DpadController dpadController;

    public Action OnGameRestart;

    public Action OnPacmanRegenCounterDone;

    public void Init() 
    {
        dpadController.Show();
        gameOverPanel.SetActive(false);
        pacmanRegenPanel.SetActive(false);
        gameProgressLabel.gameObject.SetActive(true);
        gameProgressPercentText.gameObject.SetActive(true);
        timeLabel.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
        gameProgressPercentText.text = "0%";
        timeText.text = "0";

        for (int i = 0; i < pacmanLives.Length; i++)
        {
            pacmanLives[i].SetActive(true);
        }
    }

    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        gameWonPanel.SetActive(false);
        timeUpPanel.SetActive(false);

        OnGameRestart?.Invoke();
    }

    IEnumerator _ShowPacmanRegenCounter()
    {
        pacmanRegenPanel.SetActive(true);

        for (int i = pacmanRegenCounter; i > 0; i--)
        {
            pacRegDurationLabel.text = string.Format("{0} secs", i);
            if (i == 1)
            {
                pacRegDurationLabel.text = "1 sec";
            }
            yield return new WaitForSeconds(1);
        }

        pacmanRegenPanel.SetActive(false);

        OnPacmanRegenCounterDone?.Invoke();
    }

    public void UpdateTime(float remainingTime)
    {
        timeText.text = Mathf.FloorToInt(remainingTime).ToString();
    }

    public void UpdateProgress(int percentComplete)
    {
        gameProgressPercentText.text = percentComplete.ToString() + "%";
    }

    public void ShowPacmanRegenCounter()
    {
        StartCoroutine(_ShowPacmanRegenCounter());
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowGameWon()
    {
        gameWonPanel.SetActive(true);
    }

    public void ShowTimeUp()
    {
        timeUpPanel.SetActive(true);
    }

    public void RemoveLife(int index)
    {
        pacmanLives[index].SetActive(false);
    }
}
