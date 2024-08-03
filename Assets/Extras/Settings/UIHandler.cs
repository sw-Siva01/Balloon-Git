using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public virtual void ShowMe()
    {
        UI_Controller.instance.AddToOpenPages(this);
        gameObject.SetActive(true);
    }
    public virtual void HideMe()
    {
        UI_Controller.instance.RemoveFromOpenPages(this);
        gameObject.SetActive(false);
    }
    public virtual void OnBack()
    {
        HideMe();
    }

}
