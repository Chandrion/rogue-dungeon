using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAllLabel : MonoBehaviour
{
    
    void LateUpdate()
    {
        GetComponent<Text>().text = $"Unlock all Levels\n{(GameManager.Instance.UnlockAll?"ON":"OFF")}";
    }
}
