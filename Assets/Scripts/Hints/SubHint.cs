using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class SubHint : MonoBehaviour
{
    [Header("Config")]
    public bool InputRestricted = false;
    public bool ControllerHint = false;

    [Header("Sprites")]
    public Sprite OnSprite;
    public Sprite OffSprite;

    public bool ShouldBeVisible { get => !InputRestricted || GameManager.HasController == ControllerHint; }

    private Hint Hint;
    private SpriteRenderer Renderer;

    private bool visible;
    private float frameTime;

    private void Awake()
    {
        Hint = GetComponentInParent<Hint>();

        if (!Hint)
            Destroy(gameObject);

        Renderer = GetComponent<SpriteRenderer>();
        visible = false;
        SetAlphas(Renderer, 0);
        frameTime = 0;

        if (!ShouldBeVisible)
            gameObject.SetActive(false);
        else
            Hint.SubHints.Add(this);
    }

    public void ToggleVisible(bool newState)
    {
        if (newState != visible)
        {
            visible = newState;
            if (visible)
                StartCoroutine(Show(Renderer));
            else
                StartCoroutine(Hide(Renderer));
        }
    }

    private void LateUpdate()
    {
        if (OnSprite && OffSprite)
        {
            frameTime -= Time.deltaTime * Hint.AnimationSpeed;
            if (frameTime < -1) frameTime += 2;
            Renderer.sprite = frameTime > 0 ? OnSprite : OffSprite;
        }

    }

    private IEnumerator Show(SpriteRenderer sprite)
    {
        SetAlphas(sprite, 0);
        float transitionTime = 0;

        while (transitionTime < Hint.TransitionDuration)
        {
            SetAlphas(sprite, transitionTime / Hint.TransitionDuration);
            yield return new WaitForEndOfFrame();
            transitionTime += Time.deltaTime;
        }

        SetAlphas(sprite, 1);
    }

    private IEnumerator Hide(SpriteRenderer sprite)
    {
        SetAlphas(sprite, 1);
        float transitionTime = 0;

        while (transitionTime < Hint.TransitionDuration)
        {
            SetAlphas(sprite, 1 - transitionTime / Hint.TransitionDuration);
            yield return new WaitForEndOfFrame();
            transitionTime += Time.deltaTime;
        }

        SetAlphas(sprite, 0);
    }

    private void SetAlphas(SpriteRenderer sprite, float a)
    {
        sprite.color = SetAlpha(sprite.color, a);
    }

    private Color SetAlpha(Color color, float a) => new Color(color.r, color.g, color.b, a);
}
