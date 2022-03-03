using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinueGameLabel : MonoBehaviour
{
    private void Start()
    {
        var Text = GetComponent<Text>();
        if(GameManager.Instance.LevelsUnlocked > 0)
            Text.text = $"Continue Level {GameManager.Instance.LastLevelUnlocked}";
        else
            Text.text = $"Start Level 1";
    }
}
