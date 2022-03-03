using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmartOrge : SmartEnemy
{
    public override AudioClip AttackSound { get => AudioCollection.Instance.OrgeAttack; }
    public override AudioClip DamageSound { get => AudioCollection.Instance.OrgeDamage; }
    public override AudioClip StepSound => AudioCollection.Instance.OrgeWalk;
    public override float AudioPitch => 1f;
    public override float AudioVolume => 1.2f;
    public override float DamagePitch => 1f;

    [Header("Orge Specific")]
    public bool Asleep = true;

    public bool GuardWakeupPosition = false;

    public SpriteRenderer WeaponSprite;

    private Quaternion BaseRotation;

    public new void Start()
    {
        base.Start();

        BaseRotation = WeaponSprite.transform.rotation;

        Animator.SetBool("Awake", !Asleep);
    }


    public override void EntityUpdate()
    {
        if (!Asleep)
        {
            var entities = FindObjectsOfType<Entity>().Where(x => !(x is SmartOrge) && x.IsAlive && (!(x is SmartZombie zombie) || !zombie.IsDead) && (transform.position - x.transform.position).sqrMagnitude < AttackRange*AttackRange);
            if (entities.Count() > 0)
                AttackEntity(entities.First());
            else
                base.EntityUpdate();
        }

        else
            Stop();
    }

    public override void Heal(int heal)
    {
        if (Asleep)
        {
            Asleep = false;
            Animator.SetBool("Awake", true);

            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.OrgeWake, transform, .6f);

            AlertManager.Instance.Alert(this);
        }
        base.Heal(heal);
    }

    public override void GainAggro(Entity entity)
    {
        if (Asleep)
        {
            Asleep = false;
            Animator.SetBool("Awake", true);

            if (GuardWakeupPosition && GuardLocation)
                GuardTarget = transform.position;

            if (AudioCollection.Instance)
                AudioManager.PlaySound(AudioCollection.Instance.OrgeWake, transform, .6f);
        }
        base.GainAggro(entity);
    }

    protected override void AttackEntity(Entity entity)
    {
        base.AttackEntity(entity);

        Vector2 direction = (entity.transform.position - transform.position).normalized;

        if (direction.x >= 0)
        {
            WeaponSprite.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        }
        else
        {
            WeaponSprite.transform.rotation = Quaternion.FromToRotation(Vector3.left, direction);
        }
        StartCoroutine(SwingWeapon(.2f, -180));
    }

    private IEnumerator SwingWeapon(float duration, float angle)
    {
        for (float timePassed = 0; timePassed < duration; timePassed += Time.deltaTime)
        {
            WeaponSprite.transform.Rotate(new Vector3(0, 0, angle * Time.deltaTime / duration));
            yield return new WaitForEndOfFrame();
        }

        WeaponSprite.transform.localRotation = BaseRotation;
    }
}
