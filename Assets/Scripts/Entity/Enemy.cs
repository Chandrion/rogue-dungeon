using System.Linq;
using UnityEngine;

public class Enemy : Entity
{
    public virtual AudioClip AttackSound => AudioCollection.Instance.DemonAttack;
    public override AudioClip DamageSound => AudioCollection.Instance.DemonDamage;
    public override AudioClip StepSound => AudioCollection.Instance.DemonWalk;
    public override float AudioPitch => 1.2f;
    public override float AudioVolume => .8f;

    [Header("Attack Stats")]
    public float AttackRange = 1f;
    public float AttackCooldown = 1;
    public int AttackDamage = 1;

    [Header("Aggro Stats")]
    public float AggroRange = 3f;
    public float CascadeRange = 3;
    public float AggroDuration = 1;
    protected float OutOfRangeDuration = 0;
    protected bool IsAggro = false;
    protected Vector2? AggroTarget = null;
    protected bool TargetLost = false;
    protected Entity AggroedEntity = null;

    [Header("Wander Stats")]
    public float WanderRange = 2;
    public float WanderCooldown = 2;
    public float WanderTolerance = .3f;
    protected float TimeStanding = 0;
    protected Vector2? WanderTarget;
    protected float WanderObstuctionDuration;
    protected float WanderTime;

    [Header("Guard Stats")]
    public bool GuardLocation = false;
    public float GuardRadius = 2;
    protected Vector2 GuardTarget;

    [Header("FX Settings")]
    public GameObject AttackFX;
    public GameObject DieFX;

    protected Player Player;
    protected int IgnoreLayerMask = int.MaxValue ^ (4 + 512 + 1024 + 4096);

    protected Chest[] mimics;

    public bool PlayerIsVisible { get => IsVisible(Player.transform); }

    public void Start()
    {
        Player = FindObjectOfType<Player>();
        if (GuardLocation)
            GuardTarget = transform.position;
        mimics = FindObjectsOfType<Chest>().Where(x => x.IsMimic).ToArray();
    }

    public override void EntityUpdate()
    {
        if (!AggroedEntity)
            LoseAggro();

        AttackTargetsIfNearby();
        GainAggroOnPlayerIfVisible();

        //Engage target
        bool engaged = EngageTargetIfAggroed();

        //Wander around
        bool wandering = false;
        if (!engaged)
            wandering = WanderAroundIfAble();

        //Look for Wander location
        if (!wandering)
            LookForWanderTarget();

    }

    protected virtual bool AttackTargetsIfNearby()
    {
        if ((Player.transform.position - transform.position).sqrMagnitude < AttackRange * AttackRange)
            AttackPlayer();
        else if (AggroedEntity != null && (AggroedEntity.transform.position - transform.position).sqrMagnitude < AttackRange * AttackRange)
            AttackEntity(AggroedEntity);
        else
            return false;
        return true;
    }

    protected virtual bool GainAggroOnPlayerIfVisible()
    {
        if (!IsAggro && Vector3.Distance(Player.transform.position, transform.position) < AggroRange && PlayerIsVisible)
        {
            GainAggro(Player);
        }
        else
            return false;
        return true;
    }

    protected virtual bool EngageTargetIfAggroed()
    {
        if (IsAggro)
        {
            if (TargetLost || !IsVisible(AggroedEntity.transform) || Vector3.Distance(AggroedEntity.transform.position, transform.position) > AggroRange)
                OutOfRangeDuration += Time.deltaTime;
            else
                OutOfRangeDuration = 0;

            if (OutOfRangeDuration > AggroDuration)
                LoseAggro();
            else
            {
                UpdateAggroTarget();
                if (!TargetLost && AggroTarget != null)
                    Move(AggroTarget.Value - (Vector2)transform.position);
                else
                    Stop();
            }
        }
        else
            return false;
        return true;
    }

    protected virtual bool WanderAroundIfAble()
    {
        if (WanderTarget != null)
        {
            WanderTime += Time.deltaTime;

            if ((WanderTarget.Value - (Vector2)transform.position).sqrMagnitude < WanderTolerance || WanderTime > WanderRange)
            {
                WanderTime = 0;
                WanderTarget = null;


                //Get eaten by mimics
                if (Health == 1)
                {
                    var closeMimics = mimics.Where(x => ((Vector2)x.transform.position - (Vector2)transform.position).sqrMagnitude < 3);
                    closeMimics.FirstOrDefault()?.Attack(this);
                }
            }
            else
                Move(WanderTarget.Value - new Vector2(transform.position.x, transform.position.y));
        }
        else
            return false;
        return true;
    }

    protected virtual bool LookForWanderTarget()
    {
        if (!IsAggro && WanderTarget == null)
        {
            TimeStanding += Time.deltaTime;
            Stop();

            if (TimeStanding > WanderCooldown)
            {
                Vector2 direction;
                Vector2 randomDirection = new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f));
                if (!GuardLocation)
                    direction = randomDirection * WanderRange;
                else
                    direction = GuardTarget + Vector2.ClampMagnitude(randomDirection, 1) * GuardRadius - (Vector2)transform.position;

                direction = Vector2.ClampMagnitude(direction, WanderRange);

                if (!Physics2D.Raycast(transform.position, direction, WanderRange, IgnoreLayerMask))
                {
                    WanderTarget = new Vector2(transform.position.x, transform.position.y) + direction;
                    TimeStanding = Random.Range(0, .3f * WanderCooldown);
                    WanderObstuctionDuration = 0;
                }
            }
        }
        else
            return false;
        return true;
    }


    public virtual void GainAggro(Entity entity)
    {
        if (!IsAggro || entity != AggroedEntity)
        {
            AlertManager.Instance?.Alert(this);
            AggroedEntity = entity;
            IsAggro = true;
            WanderTarget = null;
            UpdateAggroTarget();
        }
        OutOfRangeDuration = 0;
    }

    protected virtual void UpdateAggroTarget()
    {
        if (IsVisible(AggroedEntity.transform))
        {
            //Found you!
            TargetLost = false;
            AggroTarget = AggroedEntity.transform.position;
        }
        else
        {
            float roomThreshhold = .1f;

            var diff = AggroedEntity.transform.position - transform.position;

            //Get 2 opposite corners of collider
            var upperLeftBound = (Vector2)transform.position + (collider.bounds.size.y / 2) * Vector2.up - (collider.bounds.size.x / 2) * Vector2.right + collider.offset;
            var lowerRightBound = (Vector2)transform.position - (collider.bounds.size.y / 2) * Vector2.up + (collider.bounds.size.x / 2) * Vector2.right + collider.offset;

            //See how much room there is from both corners in both directions
            var xHit = Physics2D.Raycast(upperLeftBound, Vector2.right * diff.x, float.PositiveInfinity, IgnoreLayerMask ^ 256);
            var yHit = Physics2D.Raycast(upperLeftBound, Vector2.up * diff.y, float.PositiveInfinity, IgnoreLayerMask ^ 256);
            var xHit2 = Physics2D.Raycast(lowerRightBound, Vector2.right * diff.x, float.PositiveInfinity, IgnoreLayerMask ^ 256);
            var yHit2 = Physics2D.Raycast(lowerRightBound, Vector2.up * diff.y, float.PositiveInfinity, IgnoreLayerMask ^ 256);

            //How far do I even have to go?
            float playerRoomX = Mathf.Abs(diff.x);
            float playerRoomY = Mathf.Abs(diff.y);

            //How much distance is there in both directions
            float xRoom = Mathf.Min(Mathf.Abs(xHit.point.x - transform.position.x), playerRoomX);
            float yRoom = Mathf.Min(Mathf.Abs(yHit.point.y - transform.position.y), playerRoomY);
            xRoom = Mathf.Min(Mathf.Abs(xHit2.point.x - transform.position.x), xRoom);
            yRoom = Mathf.Min(Mathf.Abs(yHit2.point.y - transform.position.y), yRoom);

            //How did you get there?
            if ((Mathf.Abs(diff.x) < roomThreshhold && playerRoomY > yRoom) ||
                (Mathf.Abs(diff.y) < roomThreshhold && playerRoomX > xRoom) ||
                (playerRoomX > xRoom && playerRoomY > yRoom))
            {
                AggroTarget = null;
                TargetLost = true;
            }

            //That corner won't hide you
            else if (xRoom < playerRoomX && yRoom == playerRoomY)
                AggroTarget = transform.position + Vector3.up * diff.y;
            else if (yRoom < playerRoomY && xRoom == playerRoomX)
                AggroTarget = transform.position + Vector3.right * diff.x;

            //I don't know what to do...
            else
                AggroTarget = null;
        }
    }

    public void LoseAggro()
    {
        if (IsAggro)
        {
            AlertManager.Instance?.Lose(this);
            IsAggro = false;
            AggroedEntity = null;
        }
    }

    protected void CascadeAggro(Entity entity)
    {
        var allies = FindObjectsOfType<Enemy>()
            .Where(x => x.GetType() == GetType())
            .Where(x => (x.transform.position - transform.position).sqrMagnitude < CascadeRange * CascadeRange);

        foreach (var ally in allies)
            ally.GainAggro(entity);
    }

    protected bool IsVisible(Transform target)
    {
        var hit = Physics2D.Raycast(transform.position, target.position - transform.position, float.PositiveInfinity, IgnoreLayerMask);
        bool isVisible = hit.transform == target;

        /*
        bool realVisible = false;

        if (isVisible)
        {
            //Get the 4 corners of collider
            var upperLeftBound = (Vector2)transform.position + (collider.bounds.size.y / 2) * Vector2.up - (collider.bounds.size.x / 2) * Vector2.right + collider.offset;
            var upperRightBound = (Vector2)transform.position + (collider.bounds.size.y / 2) * Vector2.up + (collider.bounds.size.x / 2) * Vector2.right + collider.offset;
            var lowerLeftBound = (Vector2)transform.position - (collider.bounds.size.y / 2) * Vector2.up - (collider.bounds.size.x / 2) * Vector2.right + collider.offset;
            var lowerRightBound = (Vector2)transform.position - (collider.bounds.size.y / 2) * Vector2.up + (collider.bounds.size.x / 2) * Vector2.right + collider.offset;

            //Double check if gap is wide enough
            var upperLeftHit = Physics2D.Raycast(upperLeftBound, (Vector2)target.position - upperLeftBound, float.PositiveInfinity, IgnoreLayerMask);
            var upperRightHit = Physics2D.Raycast(upperRightBound, (Vector2)target.position - upperRightBound, float.PositiveInfinity, IgnoreLayerMask);
            var lowerLeftHit = Physics2D.Raycast(lowerLeftBound, (Vector2)target.position - lowerLeftBound, float.PositiveInfinity, IgnoreLayerMask);
            var lowerRightHit = Physics2D.Raycast(lowerRightBound, (Vector2)target.position - lowerRightBound, float.PositiveInfinity, IgnoreLayerMask);

            realVisible = lowerLeftHit.transform == target && lowerRightHit.transform == target &&
                upperLeftHit.transform == target && upperRightHit.transform == target;
        }

        return realVisible;
        */
        return isVisible;
    }

    protected virtual void AttackPlayer() => AttackEntity(Player);

    protected virtual void AttackEntity(Entity entity)
    {

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AttackSound, transform, .2f, pitch: AudioPitch);

        entity.TakeDamage(AttackDamage, this);
        CascadeAggro(entity);
        Freeze(AttackCooldown);

        Vector2 direction = (entity.transform.position - transform.position).normalized;

        if (direction.x != 0)
            gameObject.transform.localScale = Vector3.up + Vector3.forward + ((direction.x < 0) == LookingLeft ? Vector3.right : Vector3.left);

        SpriteRenderer throwFx = Instantiate(AttackFX, transform).GetComponent<SpriteRenderer>();

        throwFx.gameObject.transform.localScale = Vector3.up + Vector3.forward + (transform.localScale.x < 0 ? Vector3.left : Vector3.right);
        throwFx.gameObject.transform.localScale *= FXScale;

        if (direction.x >= 0)
        {
            throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
            throwFx.flipX = false;
        }
        else
        {
            throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.left, direction);
            throwFx.flipX = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        WanderObstuctionDuration += Time.deltaTime;

        if(WanderObstuctionDuration > .3f)
        {
            Vector3 direction;

            if (collision.contactCount == 1)
                direction = transform.position - (Vector3)collision.GetContact(0).point;
            else
            {
                var point1 = collision.GetContact(0).point;
                var point2 = collision.GetContact(1).point;

                if (point1.x == point2.x)
                    direction = (transform.position.x - point1.x) * Vector3.right;
                else
                    direction = (transform.position.y - point1.y) * Vector3.up;
            }



            WanderTarget = transform.position + direction.normalized * .5f;
            WanderObstuctionDuration = 0;
        }
    }

    public override void TakeDamage(int damage = 1, Entity entity = null)
    {
        base.TakeDamage(damage, entity);
        if(entity != null)
            CascadeAggro(entity);
        else
            CascadeAggro(Player);
    }

    public override void Die()
    {
        GameObject dieFx = Instantiate(DieFX, transform.position, Quaternion.identity);
        dieFx.transform.localScale *= FXScale;
        Destroy(gameObject);
    }
}
