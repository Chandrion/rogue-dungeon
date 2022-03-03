using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public abstract AudioClip DamageSound { get; }
    public abstract AudioClip StepSound { get; }
    public virtual float AudioPitch { get => 1; }
    public virtual float AudioVolume { get => 1; }
    public virtual float DamagePitch => AudioPitch;
    public virtual float DamageVolume => AudioVolume;
    public virtual float DamageStart => 0;

    [Header("General Stats")]
    public int MaxHealth;
    [HideInInspector]
    public int Health;
    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed
    {
        get => IsChanneling ? moveSpeed * .5f : moveSpeed;
        set => moveSpeed = value;
    }
    [SerializeField]
    private float acceleration;
    public float Acceleration
    {
        get => IsStunned ? acceleration * .1f : acceleration;
        set => acceleration = value;
    }
    public bool LookingLeft;

    public GameObject HealFX;
    public float FXScale;

    public bool IsStunned { get => StunDuration > 0; }
    private float StunDuration = 0;
    public bool IsImmortal { get => IFrameDuration > 0; }
    private float IFrameDuration = 0;
    public bool IsFrozen { get => FreezeDuration > 0; }
    private float FreezeDuration = 0;
    public bool IsChanneling { get => ChannelDuration > 0; }
    private float ChannelDuration = 0;
    public bool IsRemote { get => RemoteDuration > 0; }
    private float RemoteDuration = 0;

    public bool IsAlive { get => Health > 0; }

    private float stepGap = .35f;
    private float stepTime = 0;

    [HideInInspector]
    public Rigidbody2D Rigidbody2D;
    public Animator Animator;
    [HideInInspector]
    public SpriteRenderer SpriteRenderer;

    protected new BoxCollider2D collider;

    private void Awake()
    {
        Rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        collider = GetComponentInChildren<BoxCollider2D>();

        if(!Animator)
            Animator = GetComponentInChildren<Animator>();

        Health = MaxHealth;
    }

    public void Update()
    {
        if (StunDuration > 0)
            StunDuration -= Time.deltaTime;
        if (IFrameDuration > 0)
            IFrameDuration -= Time.deltaTime;
        if (FreezeDuration > 0)
            FreezeDuration -= Time.deltaTime;
        if (ChannelDuration > 0)
            ChannelDuration -= Time.deltaTime;
        if (RemoteDuration > 0)
            RemoteDuration -= Time.deltaTime;

        if (!IsFrozen && !IsRemote)
            EntityUpdate();
        else if(!IsRemote)
            Stop();
    }

    public abstract void EntityUpdate();

    public void Freeze(float timespan)
    {
        FreezeDuration = Mathf.Max(FreezeDuration, timespan);
        Rigidbody2D.velocity = Vector2.zero;
    }
    public void Channel(float timespan)
    {
        ChannelDuration = Mathf.Max(ChannelDuration, timespan);
    }
    public void Remote(float timespan)
    {
        RemoteDuration = Mathf.Max(RemoteDuration, timespan);
    }
    public void Invincible(float timespan)
    {
        IFrameDuration = Mathf.Max(IFrameDuration, timespan);
    }

    public void Move(Vector2 direction)
    {
        direction = direction.normalized;

        //Update Graphics
        Animator.SetFloat("Velocity", direction.sqrMagnitude);
        if (direction.x != 0)
            gameObject.transform.localScale = Vector3.up + Vector3.forward + ((direction.x < 0) == LookingLeft ? Vector3.right : Vector3.left);

        //Calc Force
        Vector2 velocityDelta = direction * MoveSpeed - Rigidbody2D.velocity;

        velocityDelta.x = Mathf.Clamp(velocityDelta.x, -Acceleration, Acceleration);
        velocityDelta.y = Mathf.Clamp(velocityDelta.y, -Acceleration, Acceleration);

        Rigidbody2D.AddForce(velocityDelta * Rigidbody2D.mass, ForceMode2D.Impulse);

        stepTime += Time.deltaTime;

        //Sounds
        if (direction.sqrMagnitude != 0 && AudioCollection.Instance)
        {

            if (stepTime > stepGap)
            {
                AudioManager.PlaySound(StepSound, transform, .1f * AudioVolume, AudioPitch);
                stepTime = 0;
            }
        }
    }

    public void Stop() => Move(Vector2.zero);

    public virtual void TakeDamage(int damage = 1, Entity attacker = null)
    {
        if (IsImmortal || !IsAlive)
            return;
        IFrameDuration = .1f;

        Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
        Animator.SetTrigger("Damaged");

        if(AudioCollection.Instance)
            AudioManager.PlaySound(DamageSound, transform, .4f * DamageVolume, DamagePitch * .8f, DamageStart);

        StartCoroutine(DamageTint(.4f));

        if (Health < 1)
            Die();
    }

    public void TakeDamage(Vector2 attacker, float knockbackStrength, int damage = 1, Entity from = null)
    {
        if (IsImmortal)
            return;

        TakeDamage(damage, from);

        Vector2 knockbackDirection = (new Vector2(transform.position.x, transform.position.y) - attacker).normalized;

        Rigidbody2D.AddForce(knockbackDirection * knockbackStrength * Rigidbody2D.mass, ForceMode2D.Impulse);

        StunDuration = .5f;
        if (!(this is Player))
            Freeze(.1f);
    }

    public virtual void Heal(int heal)
    {
        if (heal < 1)
            return;

        int hearts = Mathf.Clamp(heal, 0, MaxHealth - Health);
        Health = Mathf.Clamp(Health + heal, 0, MaxHealth);

        int i = 0;
        do 
        {
            var offset = new Vector2(Random.Range(-FXScale, FXScale) * .2f, Random.Range(-FXScale, FXScale) * .1f);
            var scaleOffset = Mathf.Clamp( FXScale * Random.Range(.6f, .8f), .5f, 1.2f);

            Instantiate(HealFX, (Vector2)Animator.transform.position + offset, Quaternion.identity).transform.localScale = Vector3.one * scaleOffset;
            i++;
        } while (i < hearts) ;
    }

    private bool InRed = false;
    private IEnumerator DamageTint(float duration)
    {
        if (!InRed)
        {
            InRed = true;
            SpriteRenderer.color = new Color(1, .5f, .5f);
            for (float time = 0; time < duration && gameObject; time += Time.deltaTime)
            {
                SpriteRenderer.color = new Color(1, .5f + .5f * time / duration, .5f + .5f * time / duration);
                yield return new WaitForEndOfFrame();
            }
            SpriteRenderer.color = Color.white;
        }
        InRed = false;
    }


    public abstract void Die();
}
