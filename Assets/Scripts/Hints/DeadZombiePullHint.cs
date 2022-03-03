using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZombiePullHint : Activator
{
    private bool Disabled = false;
    private SmartZombie zombie;

    public override bool ManualControllable => false;

    public override bool Controllable => !Disabled && zombie.IsPullable;

    protected override void OnToggle(bool newVal)
    {
        //Can't be toggled
    }

    private void Awake()
    {
        zombie = GetComponentInParent<SmartZombie>();
    }

    private void Update()
    {
        if (!Disabled && zombie.IsPulled)
            Disabled = true;
    }
}
