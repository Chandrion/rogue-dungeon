using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hint : MonoBehaviour
{
    [Header("General")]
    public float AnimationSpeed = 2;
    public float ShowRange = 3;
    public float TransitionDuration = .2f;

    [Header("Display Constraints")]
    public Activator Restriction;

    private Player player;
    [HideInInspector]
    public List<SubHint> SubHints = new List<SubHint>();

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void LateUpdate()
    {
        transform.localScale = Vector3.up + Vector3.forward + (transform.parent && transform.parent.localScale.x < 0 ? Vector3.left : Vector3.right);

        ToggleVisible((!Restriction || Restriction.Controllable) && Vector2.Distance(transform.position, player.transform.position) < ShowRange);
    }

    private void ToggleVisible(bool visible)
    {
        foreach(var subhint in SubHints)
        {
            subhint.ToggleVisible(visible);
        }
    }
}
