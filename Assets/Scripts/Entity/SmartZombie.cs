using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmartZombie : SmartEnemy, IPullable
{
    public override AudioClip DamageSound { get => AudioCollection.Instance.ZombieDamage; }
    public override AudioClip AttackSound => AudioCollection.Instance.Attack;
    public override AudioClip StepSound => AudioCollection.Instance.ZombieWalk;
    public override float AudioPitch => 1;

    private Vector2 BrainSpawnOffset = new Vector2(-2.6f, -0.5f);
    public SpriteRenderer Renderer => SpriteRenderer;

    public bool IsDead { get; private set; } = false;

    public bool IsPullable => IsDead;
    public Entity Puller = null;
    public bool IsPulled => Puller != null;

    private Vector2 pullOffset;

    private float reviveCooldown = .1f;

    private Dagger[] stuckDaggers;

    [Header("Zombie Specific")]
    public GameObject BrainPrefab;
    public bool SpawnDead = false;

    public new void Start()
    {
        base.Start();

        if (SpawnDead)
        {
            Health = 0;
            Die();
        }
    }

    public override void Die()
    {
        if (!IsDead)
        {
            IsDead = true;

            stuckDaggers = FindObjectsOfType<Dagger>().Where(x => x.StuckTo == transform).ToArray();
            foreach (var dagger in stuckDaggers)
            {
                dagger.StuckTo = null;
                dagger.transform.parent = Animator.transform;
                Destroy(dagger.GetComponent<Rigidbody2D>());
            }

            Animator.SetTrigger("Die");


            if (!SpawnDead)
                StartCoroutine(SpawnBrain(.3f));
            else
                SpawnDead = false;
        }

    }

    private IEnumerator SpawnBrain(float after)
    {
        yield return new WaitForSeconds(after);

        if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.FallOver, transform);

        var hit = Physics2D.Raycast(transform.position, new Vector2(BrainSpawnOffset.x * (transform.localScale.x < 0 ? -1 : 1), BrainSpawnOffset.y), BrainSpawnOffset.magnitude, int.MaxValue ^ 4096 ^ 2048);
        if (hit)
        {
            Instantiate(BrainPrefab, hit.point, Quaternion.identity)
                .GetComponent<Brain>().SetUsed();
        }
        else
        {
            Instantiate(BrainPrefab, (Vector2)transform.position + new Vector2(BrainSpawnOffset.x * transform.localScale.x, BrainSpawnOffset.y), Quaternion.identity)
                .GetComponent<Brain>().SetUsed();
        }

    }

    public override void EntityUpdate()
    {
        if (!IsDead)
            base.EntityUpdate();
        else
        {
            reviveCooldown -= Time.deltaTime;

            if (Puller)
            {
                Puller.Channel(.05f);

                Rigidbody2D.MovePosition(
                    Puller.transform.position + (Vector3)pullOffset -
                    new Vector3(Animator.transform.localPosition.x * (transform.localScale.x < 0 ? -1 : 1), Animator.transform.localPosition.y)
                    );

            }
            else
            {
                Stop();
            }
        }
    }

    public bool Revive()
    {
        if (reviveCooldown > 0)
            return false;

        Freeze(.5f);
        StartCoroutine(RebindDaggers());

        IsDead = false;
        Animator.SetTrigger("Revive");

        reviveCooldown = .1f;

        Heal(MaxHealth);
        return true;
    }

    public void AddNewStuckDagger(Dagger dagger)
    {
        var newList = stuckDaggers.ToList();
        newList.Add(dagger);
        stuckDaggers = newList.ToArray();
    }

    private IEnumerator RebindDaggers()
    {
        yield return new WaitForSeconds(.5f);

        foreach (var dagger in stuckDaggers)
        {
            if (dagger)
            {
                dagger.transform.parent = null;
                dagger.StuckTo = transform;
            }
        }
    }

    public void SetPull(bool state, Entity player)
    {
        if (state && IsPullable)
        {
            Puller = player;
            pullOffset = Animator.transform.position - player.transform.position;

            float minDistance;
            float currentDistance = pullOffset.magnitude;
            float distanceFromZombie = Physics2D.Raycast(Animator.transform.position, -pullOffset, float.PositiveInfinity, 256).distance;
            float distanceFromPlayer = Physics2D.Raycast(Puller.transform.position, pullOffset, float.PositiveInfinity, 4096).distance;

            minDistance = (currentDistance - distanceFromPlayer) + (currentDistance - distanceFromZombie);
            minDistance *= 1.1f;

            pullOffset *= 1 + (minDistance - currentDistance) / currentDistance;
        }
        else
        {
            pullOffset = Vector2.zero;
            Puller = null;
        }
    }


}
