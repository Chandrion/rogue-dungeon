using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableGroup : Activatable
{
    private readonly List<Activatable> activatables = new List<Activatable>();

    private void Awake()
    {
        var children = GetComponentsInChildren<Activatable>();

        foreach(var child in children)
            if (!(child is ActivatableGroup))
                activatables.Add(child);

    }

    private void Start()
    {
        var parentGroup = GetComponentsInParent<ActivatableGroup>();
        if (parentGroup.Length == 1)
            OnSetActive(Active);
    }

    protected override bool OnSetActive(bool active)
    {
        activatables.ForEach(x => x.Active = active);
        return active;
    }
}
