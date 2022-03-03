using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsChest : Activatable
{
    public GameObject CreditsRoot;

    private void Start()
    {
        Image background = CreditsRoot.GetComponent<Image>();
        Text[] texts = CreditsRoot.GetComponentsInChildren<Text>();
        background.color = new Color(0, 0, 0, 0);
        foreach (var text in texts)
            text.color = new Color(1, 1, 1, 0);
    }

    protected override bool OnSetActive(bool active)
    {
        if (active)
        {
            //Show Credits
            StartCoroutine(ShowCredits());

        }
        StartCoroutine(ResetChest());
        return false;
    }
    private IEnumerator ShowCredits()
    {
        Player player = FindObjectOfType<Player>();
        Image background = CreditsRoot.GetComponent<Image>();
        Text[] texts = CreditsRoot.GetComponentsInChildren<Text>();
        float fadeDuration = .5f;
        player.Freeze(fadeDuration);

        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            background.color = new Color(0, 0, 0, time / fadeDuration);
            foreach (var text in texts)
                text.color = new Color(1, 1, 1, time / fadeDuration);
            yield return new WaitForEndOfFrame();
        }

        background.color = Color.black;
        foreach (var text in texts)
            text.color = Color.white;

        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton4))
                break;
            player.Freeze(.1f);
        }

        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            background.color = new Color(0, 0, 0, 1 - time / fadeDuration);
            foreach (var text in texts)
                text.color = new Color(1, 1, 1, 1 - time / fadeDuration);
            yield return new WaitForEndOfFrame();
        }
        background.color = new Color(0, 0, 0, 0);
        foreach (var text in texts)
            text.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator ResetChest()
    {
        yield return new WaitForSeconds(4);
        GetComponent<Chest>().Active = false;
        GetComponent<Animator>().SetTrigger("Reset");
    }
}
