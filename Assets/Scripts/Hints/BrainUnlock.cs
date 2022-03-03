using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainUnlock : Activator
{
    private float GotBrains = 0;
    private bool Disabled = false;
    private Player player;

    public override bool ManualControllable => false;

    public override bool Controllable => !Disabled && GotBrains > 0 && player.Brains > 0 && player.EquipmentList.GetSelectedItem() == EquipmentList.EquipmentType.Brain;

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
            if (player.Brains > 0 && GotBrains == 0)
                GotBrains = player.Brains;

            if (GotBrains > player.Brains)
                Disabled = true;
        }
        
    }

}
