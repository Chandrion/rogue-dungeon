using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    public GameObject ImpactFX;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Apply damage to target
        Entity entity = collision.gameObject.GetComponent<Entity>();
        Player player = FindObjectOfType<Player>();
        if (entity)
            entity.TakeDamage(transform.position, Knockback, Damage, player);

        var box = collision.gameObject.GetComponent<Box>();
        if (box)
            box.Destroy();

        Instantiate(ImpactFX, transform.position, Quaternion.identity);
        Destroy(gameObject);

        if (AudioCollection.Instance)
        {
            if (entity)
                AudioManager.PlaySound(AudioCollection.Instance.BulletImpactEnemy, transform, .1f);
            else
                AudioManager.PlaySound(AudioCollection.Instance.BulletImpact, transform, .1f);
        }

    }

}