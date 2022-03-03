using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpikeFloor : Activatable
{
    public float Knockback = 5;
    public int Damage = 1;

    private bool noSound = true;

    protected override bool OnSetActive(bool active)
    {
        GetComponent<Animator>().SetBool("Active", active);
        GetComponentsInChildren<Collider2D>().ToList().ForEach(x => x.enabled = active);

        if (AudioCollection.Instance)
        {
            if (!noSound)
            {
                if (active)
                {
                    AudioManager.PlaySound(AudioCollection.Instance.SpikesActivate, transform, .2f, .7f);
                }
                else
                {
                    AudioManager.PlaySound(AudioCollection.Instance.SpikesActivate, transform, .2f, -.7f, .3f);
                }
            }
            else
                noSound = false;
        }

        return active;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Active)
        {
            Entity entity = collision.gameObject.GetComponent<Entity>();
            if (entity)
            {
                entity.TakeDamage(transform.position, Knockback, Damage);
                if(AudioCollection.Instance && (!(entity is SmartZombie zombie) || !zombie.IsDead))
                    AudioManager.PlaySound(AudioCollection.Instance.SpikesAttack, transform);
            }
        }
    }
}
