using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Reset : MonoBehaviour
{
    public enum Action { Restart, Quit }
    public Action ResetAction;

    public GameObject Up, Down;
    public GameObject UpAlt, DownAlt;

    public GameObject GetUp => GameManager.HasController ? UpAlt : Up;
    public GameObject GetDown => GameManager.HasController ? DownAlt : Down;

    public float AnimationSpeed = 1;
    private float AnimationProgress = 0;

    public Image ProgressBar;
    public Text ResetText;
    public float MinDuration = 1.5f;

    private float Progress = 0;

    public float UnveilTimer = .5f;

    public RestartCondition Condition;

    private bool ResetDown { get => Input.GetKey(KeyCode.R) || Input.GetAxisRaw("DPad Vertical") > 0; }
    private bool QuitDown { get => Input.GetKey(KeyCode.Q) || Input.GetAxisRaw("DPad Vertical") < 0; }

    private void Awake()
    {
        if (GameManager.HasController)
        {
            Up.SetActive(false);
            Down.SetActive(false);
        }
        else
        {
            UpAlt.SetActive(false);
            DownAlt.SetActive(false);
        }
    }

    private bool GetButton()
    {
        switch (ResetAction)
        {
            case Action.Restart: default:
                return ResetDown;
            case Action.Quit:
                return QuitDown;
        }
    }

    private void Update()
    {
        ResetText.gameObject.SetActive(true);
        if (GetButton())
        {
            GetUp.SetActive(false);
            GetDown.SetActive(true);
        }
        else if (UnveilTimer <= 0)
        {
            AnimationProgress += Time.deltaTime;
            if (AnimationProgress >= AnimationSpeed)
            {
                GetUp.SetActive(!GetUp.activeSelf);
                GetDown.SetActive(!GetUp.activeSelf);
                AnimationProgress = 0;
            }
        }
        else
        {
            if (Condition && Condition.ShowHint && UnveilTimer > 0)
                UnveilTimer -= Time.deltaTime;

            GetUp.SetActive(false);
            GetDown.SetActive(false);
            ResetText.gameObject.SetActive(false);
            AnimationProgress = AnimationSpeed;
        }
    }

    private void LateUpdate()
    {
        if (GetButton())
            Progress += Time.deltaTime;
        else
            Progress = 0;

        float fill = Mathf.Clamp(Progress / MinDuration, 0, 1);

        ProgressBar.fillAmount = fill;

        if (fill == 1)
            MakeAction();
    }

    private void MakeAction()
    {
        switch (ResetAction)
        {
            case Action.Restart: default:
                if (GameManager.Instance)
                    GameManager.Instance.TransitionController.LoadScene(SceneManager.GetActiveScene().name);
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case Action.Quit:
                if (GameManager.Instance)
                    GameManager.Instance.TransitionController.LoadScene("Main Menu");
                else
                    SceneManager.LoadScene("Main Menu");
                break;
        }
    }
}
