using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeUnlock : Activator
{
    private float GotKnives = 0;
    private bool Disabled = false;
    private Player player;

    public override bool ManualControllable => false;

    public override bool Controllable => !Disabled && GotKnives > 0 && player.Daggers > 0;

    protected override void OnToggle(bool newVal)
    {
        //Can't be toggled
    }

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (!Disabled)
        {
            if (player.Daggers > 0 && GotKnives == 0)
                GotKnives = player.Daggers;

            if (GotKnives > player.Daggers)
                Disabled = true;
        }
    }

}
