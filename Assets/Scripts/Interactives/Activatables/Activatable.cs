using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Activatable : MonoBehaviour
{
    [SerializeField]
    private bool active = false;
    public bool Active
    {
        get => active;
        set => active = OnSetActive(value);
    }

    protected abstract bool OnSetActive(bool active);

    private void Awake()
    {
        OnSetActive(Active);
    }
}
