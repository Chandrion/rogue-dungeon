using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : Projectile
{

    public Sprite StuckSprite;
    public Vector3 StuckVector;
    public Transform StuckTo = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Turn dagger into a pick-up
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<SpriteRenderer>().sprite = StuckSprite;
        transform.position = collision.contacts[0].point;
        gameObject.layer = 10;

        //Apply damage to target
        Entity entity = collision.gameObject.GetComponent<Entity>();
        if (entity)
        {
            Player player = FindObjectOfType<Player>();

            if(entity is SmartZombie zombie && zombie.IsDead)
            {
                zombie.TakeDamage(Damage, player); 
                StuckTo = null;
                StuckVector = transform.position - zombie.Animator.transform.position;

                StuckVector = Quaternion.Euler(0, 0, -90) * StuckVector;

                transform.parent = zombie.Animator.transform;
                Destroy(GetComponent<Rigidbody2D>());
                zombie.AddNewStuckDagger(this);
            }
            else
            {
                StuckTo = entity.transform;
                StuckVector = transform.position - entity.transform.position;
                entity.TakeDamage(Damage, player);
            }

            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.DaggerImpactEnemy, transform, .1f);
        }
        else if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.DaggerImpact, transform, 1);

    }

    private void Update()
    {
        if (StuckTo)
            transform.position = StuckTo.position + StuckVector;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.PickUp, transform, .8f);
            player.GainItem(dagger: 1);
            Destroy(gameObject);
        }
    }

}
