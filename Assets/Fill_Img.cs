using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Fill_Img : MonoBehaviour
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    [Header("Script")]
    [SerializeField] GameController controller;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Image")]
    [SerializeField] Image fill_Img;
    [SerializeField] Image fill_Img_Dup;
    [SerializeField] Image timerBar;

    [Header("-------------------------------------------------------------------------------------------------------------------------------------------------------")]

    [Header("Float")]
    [SerializeField] float totalTime = 10f;  // Total time for the timer
    private float timeRemaining;    // Time remaining for the timer
    [SerializeField] float Timer;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------//
    #endregion ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    private void OnEnable()
    {
        Timer = controller.bonusTimer;
    }
    void Start()
    {
        timeRemaining = 0f;
    }
    void Update()
    {
        //Timer = controller.bonusTimer;

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
        if (Timer >= 10)
        {
            timeRemaining = 0f;
            timerBar.fillAmount = 0;
            Timer = 0;
            fill_Img_Dup.fillAmount = fill_Img.fillAmount;
        }
    }
}
