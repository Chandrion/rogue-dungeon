using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Restart : RestartCondition
{
    public Player Player;
    public Switch GateLever;
    public Chest Chest;
    public SpikeFloor TopSpike;

    public override bool ShowHint => 
        (Player.Daggers < 1 && Player.transform.position.x < -4.5f && GateLever.Active) || //Softlock in west chamber
        (Player.Daggers < 1 && Player.transform.position.y > 2.5f && Chest.Active && TopSpike.Active); // Softlock in north chamber

}
