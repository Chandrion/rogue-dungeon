using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class SmartEnemy : Enemy
{
    public static float AliveRange => 15f;

    [Header("Pathfinding")]
    public float Refreshrate = .1f;
    private float timeSizeRefresh = 0;

    public bool DrawPath = false;

    private Seeker Seeker;
    private Path Path;
    private Vector3 LastTargetPos;

    private Vector3? FreezeGuardLocation;

    public new bool PlayerIsVisible { get => IsVisible(Player.transform); }

    public bool OutOfPlayerRange => !GuardLocation && (transform.position - Player.transform.position).sqrMagnitude > AliveRange * AliveRange;

    public new void Start()
    {
        base.Start();

        Seeker = GetComponent<Seeker>();
        Player = FindObjectOfType<Player>();

        Seeker.pathCallback += PathUpdate;
        Seeker.StartPath(transform.position, transform.position);
        LastTargetPos = Player.transform.position;
    }

    public override void EntityUpdate()
    {
        UpdatePath();

        if (!OutOfPlayerRange)
            FreezeGuardLocation = null;

        base.EntityUpdate();

        //Get eaten by mimics
        if (Health == 1)
        {
            var closeMimics = mimics.Where(x => ((Vector2)x.transform.position - (Vector2)transform.position).sqrMagnitude < 4);
            closeMimics.FirstOrDefault()?.Attack(this);
        }

    }

    protected override bool EngageTargetIfAggroed()
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
                    Move(NextDirection);
                else
                    Stop();
            }
        }
        else
            return false;
        return true;
    }

    protected override bool WanderAroundIfAble()
    {
        if (WanderTarget != null)
        {
            WanderTime += Time.deltaTime;

            if (ReachedEndOfPath || WanderTime > WanderRange)
            {
                WanderTime = 0;
                WanderTarget = null;
            }
            else
                Move(NextDirection);
        }
        else
            return false;
        return true;
    }

    protected override bool LookForWanderTarget()
    {
        if (!IsAggro && WanderTarget == null)
        {
            WanderTarget = null;
            TimeStanding += Time.deltaTime;
            Stop();

            if (TimeStanding > WanderCooldown)
            {
                Vector2 direction;
                Vector2 randomDirection = new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f));
                if (!GuardLocation && OutOfPlayerRange)
                {
                    if (FreezeGuardLocation == null)
                        FreezeGuardLocation = transform.position;

                    direction = (Vector2)FreezeGuardLocation + Vector2.ClampMagnitude(randomDirection, 1) * GuardRadius - (Vector2)transform.position;
                }
                else if (!GuardLocation)
                    direction = randomDirection * WanderRange;
                else
                    direction = GuardTarget + Vector2.ClampMagnitude(randomDirection, 1) * GuardRadius - (Vector2)transform.position;

                direction = Vector2.ClampMagnitude(direction, WanderRange);

                GetPathTo((Vector2)transform.position + direction);

                if(DrawPath)
                    Debug.DrawLine(transform.position, (Vector2)transform.position + direction, Color.magenta, WanderCooldown);

                WanderTarget = (Vector2)transform.position + direction;

                TimeStanding = Random.Range(0, .3f * WanderCooldown);
                WanderObstuctionDuration = 0;
                WanderTime = 0;
            }
        }
        else
            return false;
        return true;
    }


    protected new bool IsVisible(Transform target)
    {
        var hit = Physics2D.Raycast(transform.position, target.position - transform.position, float.PositiveInfinity, IgnoreLayerMask);
        bool isVisible = hit.transform == target;

        return isVisible && PathFound;
    }


    protected override void UpdateAggroTarget()
    {
        if (PathFound && AggroedEntity)
        {
            //Found you!
            TargetLost = false;
            AggroTarget = AggroedEntity.transform.position;
        }
        else
        {
            TargetLost = true;
            AggroTarget = null;
        }
    }

    public void UpdatePath()
    {

        timeSizeRefresh += Time.deltaTime;

        if (timeSizeRefresh > Refreshrate)
        {
            timeSizeRefresh = 0;

            if (IsAggro && AggroedEntity)
                GetPathTo(AggroedEntity.transform.position);

            else if (WanderTarget.HasValue)
                GetPathTo(WanderTarget.Value);
        }

        if (DrawPath && Path != null)
        {
            for (int i = 0; i < Path.vectorPath.Count - 1; i++)
                Debug.DrawLine(Path.vectorPath[i], Path.vectorPath[i + 1], Color.green);
        }
    }

    public bool PathFound => Path == null || Path.vectorPath.Count == 0 ? false : (Path.vectorPath[Path.vectorPath.Count - 1] - LastTargetPos).sqrMagnitude < 1f;

    private bool ReachedEndOfPath = false;

    public Vector2 NextWaypoint {
        get
        {
            if (Path != null && Path.vectorPath != null && Path.vectorPath.Count > 0)
            {
                List<Vector3> obsolete = new List<Vector3>();
                Vector3? nextPoint = null;

                for (int i = 1; i < Path.vectorPath.Count; i++)
                {
                    if ((Path.vectorPath[i] - transform.position).sqrMagnitude > WanderTolerance)
                    {
                        nextPoint = Path.vectorPath[i];
                        break;
                    }
                    else
                        obsolete.Add(Path.vectorPath[i]);

                }
                foreach(var i in obsolete)
                    Path.vectorPath.Remove(i);

                if (nextPoint.HasValue)
                    return nextPoint.Value;

                ReachedEndOfPath = true;
                return transform.position;
            }
            else
                return transform.position;
        }
    }
    public Vector2 NextDirection => NextWaypoint - (Vector2)transform.position;

    public void PathUpdate(Path path)
    {
        Path = path;
        currentlyGettingPath = false;
    }

    private void GetPathToPlayer() => GetPathTo(Player.transform.position);

    private void GetPathTo(Vector3 target)
    {
        StartCoroutine(GetPathToAsync(target));
    }

    private bool currentlyGettingPath = false;

    private IEnumerator GetPathToAsync(Vector3 target)
    {
        yield return new WaitUntil(() => !GameManager.IsScanning && !currentlyGettingPath);
        currentlyGettingPath = true;
        Seeker.StartPath(transform.position, target);
        LastTargetPos = target;
        ReachedEndOfPath = false;
    }
}
