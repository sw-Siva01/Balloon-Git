using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInternet : MonoBehaviour
{
    IEnumerator Start()
    {
        //APIController.instance.OnInternetStatusChange += GetNetworkStatus;
        while (true)
        {
            yield return new WaitForSeconds(2f);
            //APIController.instance.CheckInternetStatus();
            APIController.instance.CheckInternetForButtonClick((success) =>
            {
                CheckInterNet(!success);
            });
        }
    }
    public void CheckInterNet(bool enable)
    {
        GameController.instance.internetDisconnectPannel.SetActive(enable);
    }
}
