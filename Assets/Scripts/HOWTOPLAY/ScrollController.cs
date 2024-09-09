using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
   public ScrollRect Scroll;
    private void OnEnable()
    {
        Scroll.verticalNormalizedPosition = 1;
    }
}
