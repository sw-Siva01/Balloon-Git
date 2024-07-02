using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fill_Img : MonoBehaviour
{
    public GameController controller;
    public Image fill_Img;
    public Image fill_Img_Dup;
    public float totalTime = 10f;  // Total time for the timer
    private float timeRemaining;    // Time remaining for the timer
    public Image timerBar;
    public float Timer;
    void Start()
    {
        timeRemaining = 0f;
    }
    void Update()
    {
        Timer = controller.bonusTimer;

        if (timeRemaining < totalTime)
        {
            if (timeRemaining < Timer)
            {
                timeRemaining += Timer * Time.deltaTime; // Decrease time remaining
                UpdateTimerUI(); // Update the UI
            }
        }

        if (Timer > 0)
        {
            fill_Img_Dup.fillAmount = fill_Img.fillAmount;
        }
    }

    void UpdateTimerUI()
    {
        float fillAmount = timeRemaining / totalTime;
        timerBar.fillAmount = fillAmount;
    }

    private void OnDisable()
    {
        if (timeRemaining == totalTime)
        {
            timeRemaining = totalTime;
            timerBar.fillAmount = 0;
        }
    }
}
