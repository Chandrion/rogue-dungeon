using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchHint : Activator
{
    private bool GotBrain = false;
    private bool Disabled = false;
    private Player player;

    public override bool ManualControllable => false;

    public override bool Controllable => !Disabled && GotBrain;

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
            if (player.Brains > 0 && !GotBrain)
                GotBrain = true;

            if (GotBrain && player.EquipmentList.GetSelectedItem() != EquipmentList.EquipmentType.Dagger)
                Disabled = true;
        }
    }
}
