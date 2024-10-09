using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    public ScrollRect Scroll;
    public Button ExitBtn;
    private void OnEnable()
    {
        Scroll.verticalNormalizedPosition = 1;
    }

    private void Awake()
    {
        ExitBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound();});
    }
}
