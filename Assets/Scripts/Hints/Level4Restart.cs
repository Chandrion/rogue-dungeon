using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level4Restart : RestartCondition
{
    public Player Player;
    public Switch Lever;
    public SmartZombie BottomZombie, TopZombie;
    public PressurePlate ButtonWest;

    private bool PlayerGotBrain = false;
    private bool TopZombieWasRevived = false;
    private bool PlayerGotBrainFromTopZombie = false;

    public override bool ShowHint =>
        (Player.Brains < 1 && BottomZombie.IsDead && TopZombie.IsDead && !ButtonWest.Active && PlayerGotBrain && Player.transform.position.y < 5 && GetBrainPos() != null && GetBrainPos().Value.y > 5) || //SoftLock Bottom
        (Player.Brains < 1 && !Lever.Active && BottomZombie.IsDead && TopZombie.IsDead && PlayerGotBrain && Player.transform.position.y > 5 && PlayerGotBrainFromTopZombie && GetBrainPos() == null);  //Softlock Top


    private void Update()
    {
        if (!PlayerGotBrain && Player.Brains > 0)
            PlayerGotBrain = true;
        if (!TopZombieWasRevived && !TopZombie.IsDead)
            TopZombieWasRevived = true;
        if (!PlayerGotBrainFromTopZombie && Player.Brains > 0 && TopZombieWasRevived)
            PlayerGotBrainFromTopZombie = true;
    }

    private Vector2? GetBrainPos() => FindObjectOfType<Brain>()?.transform.position;

}
