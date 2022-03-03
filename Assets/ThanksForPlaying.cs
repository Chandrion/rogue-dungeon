using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanksForPlaying : MonoBehaviour
{

    void Update()
    {
        if (GameManager.Instance && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton4)))
            {
            GameManager.Instance.TransitionController.LoadScene("Main Menu", false);
        }
    }

}
