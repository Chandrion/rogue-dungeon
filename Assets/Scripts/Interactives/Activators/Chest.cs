using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chest : Activator
{
    public override bool ManualControllable => !Active;
    public override bool Controllable => !Active;

    [Header("Mimic")]
    public bool IsMimic = false;
    public int Damage = 1;
    public float AttackCooldown = 1f;
    private float attackTimePassed = 1f;
    public float Knockback = 20;
    public GameObject AttackFX;
    public GameObject FeedFX;
    private bool Befriended = false;

    [Header("Loot")]
    public int Health = 0;
    public int Daggers = 0;
    public int Brains = 0;
    public int Bullets = 0;

    [Header("Prefabs")]
    public GameObject DaggerHint;
    public GameObject BrainHint;
    public GameObject GunHint;

    private void Start()
    {
        if(IsMimic)
            StartCoroutine(MakeMimic());
    }

    private IEnumerator MakeMimic()
    {
        yield return new WaitForSeconds(Random.Range(0, .5f));
        Animator.SetBool("IsMimic", true);
    }

    protected override void OnToggle(bool newVal)
    {
        Player player = FindObjectOfType<Player>();

        if (IsMimic && !Befriended)
        {
            Attack(player);
        }
        else
        {
            Animator.SetTrigger("Open");

            if(AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.ChestOpen, transform, startTime: .6f);

            player.Heal(Health);

            StartCoroutine(ShowHints(player));
        }
    }

    public bool Befriend()
    {
        if (IsMimic)
        {
            Instantiate(FeedFX, transform.position + Vector3.right * .2f, Quaternion.identity);
            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.MimicBefriend, transform, pitch: 2);
            Befriended = true;
            StartCoroutine(ToggleSelf());
            return true;
        }
        return false;
    }

    private IEnumerator ToggleSelf()
    {
        yield return new WaitForSeconds(.2f);
        Toggle();
    }

    public void Attack(Entity entity)
    {
        if(entity is Player || (!Active && attackTimePassed >= AttackCooldown))
        {
            attackTimePassed = 0;
            Animator.SetTrigger("Mimic");
            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.Mimic, transform, 1.5f, 1);
            StartCoroutine(DealDamage(entity, .3f));
        }
    }

    private void Update()
    {
        if (attackTimePassed < AttackCooldown)
            attackTimePassed += Time.deltaTime;
    }

    private IEnumerator DealDamage(Entity target, float after)
    {
        yield return new WaitForSeconds(after);
        Instantiate(AttackFX, transform.position, Quaternion.identity)
            .transform.localScale *= 2;
        target.TakeDamage(transform.position, Knockback, Damage);
        Active = false;
    }

    private IEnumerator ShowHints(Player player)
    {
        List<GameObject> UnlockNotes = new List<GameObject>();
        UnlockNotes.AddRange(Enumerable.Repeat(DaggerHint, Daggers));
        UnlockNotes.AddRange(Enumerable.Repeat(BrainHint, Brains));
        UnlockNotes.AddRange(Enumerable.Repeat(GunHint, Bullets));

        foreach (GameObject hint in UnlockNotes)
        {
            Instantiate(hint, transform.position, Quaternion.identity);

            if(hint == DaggerHint)
                player.GainItem(dagger: 1);
            else if (hint == BrainHint)
                player.GainItem(brain: 1);
            else if (hint == GunHint)
                player.GainItem(bullet: 1);

            yield return new WaitForSeconds(.3f);
        }

    }
}
