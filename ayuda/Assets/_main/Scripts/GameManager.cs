using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text resultText;

    private float timer = 0f; 
    private bool isRunning;
    private float maxTime = 6f; 
    public int currentScore = 0; 

    void Start()
    {
        scoreText.text = "Score: " + currentScore.ToString();
        timerText.text = "Time: " + timer.ToString("F1") + "s";
        resultText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;

            if (timer >= maxTime)
            {
                timer = maxTime;
                isRunning = false;
                CalculateScore();
            }

            timerText.text = "Time: " + timer.ToString("F1") + "s";
        }
    }

    void CalculateScore()
    {
        // Calcula la puntuaci�n basada en la inversa del tiempo.
        float scorePercentage = 1f - (timer / maxTime);
        currentScore = Mathf.RoundToInt(scorePercentage * 1000);

        scoreText.text = "Score: " + currentScore;
        resultText.gameObject.SetActive(true);
        resultText.text = "You earned " + currentScore + " points!";
    }

    public void StartTimer()
    {
        timer = 0f; // Reinicia el contador a 0 segundos al presionar "Start"
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
        CalculateScore();
    }
}