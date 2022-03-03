using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FadeOutAfter : MonoBehaviour
{
    public float Lifetime;
    public float FadeDuration;

    private float timePassed = 0;
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void LateUpdate()
    {
        timePassed += Time.deltaTime;

        if(timePassed > Lifetime)
        {
            text.color = SetAlpha(text.color, 1 - (timePassed - Lifetime) / FadeDuration);

            if (timePassed > Lifetime + FadeDuration)
                Destroy(gameObject);
        }
    }
    private Color SetAlpha(Color color, float a) => new Color(color.r, color.g, color.b, a);
}
