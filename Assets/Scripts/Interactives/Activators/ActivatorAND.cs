using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivatorAND : Activator
{
    public List<Activator> activators = new List<Activator>();

    public override bool ManualControllable => false;

    public override bool Controllable => true;

    protected override void OnToggle(bool newVal)
    {
        
    }

    private void Start()
    {
        Animator = null;
    }

    private void LateUpdate()
    {
        bool value = true;

        foreach (var act in activators)
            value = value && act.Active;

        if (Active != value)
            Toggle();
    }
}
