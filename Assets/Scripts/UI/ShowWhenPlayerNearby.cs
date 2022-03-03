using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShowWhenPlayerNearby : MonoBehaviour
{
    public GameObject Target;
    public Player Player;

    public float TransitionDuration;
    public float ShowRange;

    private bool visible = false;
    private bool inTransition = false;
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        text.color = SetAlpha(text.color, 0);

        if (!Player)
            Player = FindObjectOfType<Player>();
    }

    public void LateUpdate()
    {
        if (!Target)
            Destroy(gameObject);
        else if (Vector2.Distance(Player.transform.position, Target.transform.position) < ShowRange != visible && !inTransition)
        {
            if (visible)
                StartCoroutine(Hide());
            else
                StartCoroutine(Show());
            visible = Vector2.Distance(Player.transform.position, Target.transform.position) < ShowRange;
        }
    }

    private IEnumerator Show()
    {
        inTransition = true;
        text.color = SetAlpha(text.color, 0);
        float transitionTime = 0;

        while(transitionTime < TransitionDuration)
        {
            text.color = SetAlpha(text.color, transitionTime / TransitionDuration);
            yield return new WaitForEndOfFrame();
            transitionTime += Time.deltaTime;
        }

        text.color = SetAlpha(text.color, 1);
        inTransition = false;
    }

    private IEnumerator Hide()
    {
        inTransition = true;
        text.color = SetAlpha(text.color, 1);
        float transitionTime = 0;

        while (transitionTime < TransitionDuration)
        {
            text.color = SetAlpha(text.color, 1 - transitionTime / TransitionDuration);
            yield return new WaitForEndOfFrame();
            transitionTime += Time.deltaTime;
        }

        text.color = SetAlpha(text.color, 0);
        inTransition = false;
    }

    private Color SetAlpha(Color color, float a) => new Color(color.r, color.g, color.b, a);

}
