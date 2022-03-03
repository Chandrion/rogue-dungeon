using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Brain : Projectile
{
    public bool bounced = false;
    private Player player;
    private Rigidbody2D rb;
    private new Collider2D collider;
    public float MinVelocity = 20f;

    public bool SpawnAsPickup = false;

    private float slowedDuration;

    private void Awake()
    {
        transform.rotation = Quaternion.identity;
        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        if (SpawnAsPickup)
            SetUsed();
    }

    public void SetUsed()
    {
        bounced = true;
        TurnToPickup();
    }

    private void Update()
    {
        //Catch Brain
        if(bounced && (transform.position- player.transform.position).sqrMagnitude < .5f)
        {
            player.GainItem(brain: 1);
            Destroy(gameObject);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = collision.gameObject.GetComponent<Entity>();
        var chest = collision.gameObject.GetComponent<Chest>();

        if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.BrainImpact, transform, 1);

        if (entity || (chest && chest.IsMimic))
        {
            //Apply damage to target
            if (entity is SmartZombie || entity is SmartOrge)
            {
                if (entity is SmartZombie zombie)
                {
                    if (zombie.IsDead)
                    {
                        if (zombie.Revive())
                            Destroy(gameObject);
                    }
                    else
                    {
                        if (AudioCollection.Instance)
                            AudioManager.PlaySound(AudioCollection.Instance.ZombieEat, transform, .8f);
                        zombie.Heal(1);
                        Destroy(gameObject);
                    }
                }
                else if (entity is SmartOrge orge)
                {
                    if (AudioCollection.Instance)
                        AudioManager.PlaySound(AudioCollection.Instance.ZombieEat, transform, .8f);
                    orge.Heal(1);
                    Destroy(gameObject);
                }
                return;

            }
            else if (entity)
            {
                entity.TakeDamage(transform.position, Knockback, Damage, player);
            }
            else if (chest && chest.Befriend())
            {
                Destroy(gameObject);
                return;
            }
                

        }
        if (bounced)
        {
            TurnToPickup(collision.relativeVelocity.x);
        }
        else if (!collision.gameObject.GetComponent<Projectile>())
        {
            bounced = true;
            collider.sharedMaterial = null;
        }
    }

    private void TurnToPickup(float bounceDirection = 0)
    {
        //Turn brain into pickup
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        collider.isTrigger = true;
        gameObject.layer = 10;

        if(bounceDirection != 0)
            StartCoroutine(DropAnimation(.08f, .05f, .2f, bounceDirection < 0 ? -1 : 1));
    }

    private IEnumerator DropAnimation(float upSlideTime, float slideTime, float dropTime, float flip)
    {
        Vector3 oldPos = transform.position;
        Vector3 targetPos = transform.position + new Vector3(.2f * flip, .1f);
        for (float timePassed = 0; timePassed < upSlideTime; timePassed += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, timePassed / upSlideTime);
            yield return new WaitForEndOfFrame();
        }

        oldPos = transform.position;
        targetPos = transform.position + new Vector3(.1f * flip, 0f);
        for (float timePassed = 0; timePassed < slideTime; timePassed += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, timePassed / slideTime);
            yield return new WaitForEndOfFrame();
        }

        bool inHole = Physics2D.RaycastAll(transform.position, Vector2.one, .1f, Physics2D.AllLayers).Where(x => x.transform.gameObject.tag == "Hole").Count() > 0;
        if (inHole)
            StartCoroutine(Fade(dropTime));

        oldPos = transform.position;
        targetPos = transform.position + new Vector3(.1f * flip, -.4f);
        for (float timePassed = 0; timePassed < dropTime; timePassed += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(oldPos, targetPos, timePassed / dropTime);
            yield return new WaitForEndOfFrame();
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity is Player player)
        {
            player.GainItem(brain: 1);

            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.PickUp, transform, .8f);
            Destroy(gameObject);
        }
        else if(entity is SmartZombie zombie && !zombie.IsDead)
        {

            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.ZombieEat, transform, .8f);
            zombie.Heal(1);
            Destroy(gameObject);
        }
    }

    private IEnumerator Fade(float duration)
    {
        var sprite = GetComponent<SpriteRenderer>();
        var oldCol = sprite.color;
        var newCol = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);

        for (float progress = 0; progress < duration; progress += Time.deltaTime)
        {
            sprite.color = Color.Lerp(oldCol, newCol, progress / duration);
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

}
