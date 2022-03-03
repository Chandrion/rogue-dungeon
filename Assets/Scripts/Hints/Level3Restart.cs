using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Restart : RestartCondition
{
    public Player Player;
    public Switch LeftLever;
    public Switch CentralLever;
    public Switch RightLever;

    public override bool ShowHint =>
        (Player.Daggers < 1 && LeftLever.Active && CentralLever.Active && !RightLever.Active) ||//SoftLock Center
        (Player.Daggers < 1 && LeftLever.Active && !CentralLever.Active && RightLever.Active);  //Softlock Right


}
