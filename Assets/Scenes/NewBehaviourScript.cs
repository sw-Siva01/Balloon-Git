using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public bool isTrue;
    public void Selected()
    {
        isTrue = true;
        Debug.Log(isTrue);
    }
    public void UnSelected()
    {
        isTrue = false;
        Debug.Log(isTrue);
    }
}
