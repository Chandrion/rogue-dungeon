using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionController : MonoBehaviour
{
    [SerializeField]
    private Image Fader = null;
    [SerializeField]
    private float TransitionDuration = 1;
    [SerializeField]
    private float BlackoutDuration = 1;

    private bool Loading = false;
    private float transitionSteps = .01f;

    [SerializeField]
    private Text TransitionLabel = null;

    public void LoadScene(string scene, bool showLabel = true) => LoadScene(scene, Color.black, showLabel);
    public void LoadScene(string scene, Color color, bool showLabel = true)
    {
        if (!Loading)
        {
            Loading = true;
            Fader.color = color;
            FindObjectsOfType<Entity>().ToList().ForEach(x => x.Freeze(TransitionDuration));

            if (showLabel)
            {
                if (Regex.IsMatch(scene, @"Level \d+"))
                {
                    int levelIndex = GameManager.Instance.GetLevelIndex(scene) - 1;

                    if (levelIndex >= 0 && levelIndex < GameManager.LEVEL_TITLES.Length)
                        TransitionLabel.text = scene + "\n" + GameManager.LEVEL_TITLES[levelIndex];
                }
                else
                    TransitionLabel.text = scene;
            }
            else
                TransitionLabel.text = "";

            StartCoroutine(Transition(scene));
        }
    }

    public void QuitGame()
    {
        Loading = true;
        Fader.color = new Color(1, 1, .87f);
        StartCoroutine(QuitTransition());
    }

    private IEnumerator Transition(string scene)
    {
        SetFaderAlpha(0);
        for (float i = 0; i < TransitionDuration; i+= transitionSteps)
        {
            SetFaderAlpha(i / TransitionDuration);
            yield return new WaitForSeconds(transitionSteps);
        }
        SetFaderAlpha(1);

        SceneManager.LoadScene(scene);
        yield return new WaitForSeconds(BlackoutDuration);
        StopSteps = true;

        for (float i = 0; i < TransitionDuration; i += transitionSteps)
        {
            SetFaderAlpha(1 - i / TransitionDuration);
            yield return new WaitForSeconds(transitionSteps);
        }
        SetFaderAlpha(0);
        Loading = false;
    }

    private IEnumerator QuitTransition()
    {
        SetFaderAlpha(0);
        for (float i = 0; i < TransitionDuration; i += transitionSteps)
        {
            SetFaderAlpha(i / TransitionDuration, true);
            yield return new WaitForSeconds(transitionSteps);
        }
        SetFaderAlpha(1, true);

        if (!Application.isEditor)
            Application.Quit();
        else
            Debug.Log("Game ended");
    }

    private bool StopSteps = false;

    public void StartSteps() => StartCoroutine(StepSounds());

    private IEnumerator StepSounds()
    {
        float stepGap = .3f;
        float stepTime = 0;
        StopSteps = false;

        while (!StopSteps)
        {
            yield return new WaitForEndOfFrame();
            stepTime -= Time.deltaTime;
            if (stepTime < 0)
            {
                stepTime = stepGap;
                AudioManager.PlaySound(AudioCollection.Instance.Walk, transform, .1f, .9f);
            }
        }
    }

    private void SetFaderAlpha(float a, bool noLabel = false)
    {
        Fader.color = new Color(Fader.color.r, Fader.color.g, Fader.color.b, a);
        if(!noLabel)
            TransitionLabel.color = new Color(TransitionLabel.color.r, TransitionLabel.color.g, TransitionLabel.color.b, a);
    }
}
