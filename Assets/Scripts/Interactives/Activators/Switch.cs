using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : Activator
{
    public override bool ManualControllable => true;

    public override bool Controllable => true;

    protected override void OnToggle(bool newVal)
    {
        if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Switch, transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Trigger on projectile
        if (collision.gameObject.GetComponent<Projectile>())
            Toggle();
    }

}
