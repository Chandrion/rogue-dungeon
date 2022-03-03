using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    public override AudioClip DamageSound => AudioCollection.Instance.PlayerDamage;
    public override AudioClip StepSound => AudioCollection.Instance.Walk;
    public override float AudioPitch => 1;
    public override float AudioVolume => .8f;
    public override float DamageStart => 0.6f;
    public override float DamageVolume => 1.5f;

    [Header("Player Stats")]
    public float ActivatorRange = 1f;
    public float PullRange = 1.3f;
    public float AttackCooldown = .2f;
    private float currentCd = 0;

    [Header("Weapons")]
    public int Daggers = 0;
    public int Bullets = 0;
    public int Boomerangs = 0;
    public int Brains = 0;

    public GameObject AimPointer;
    private SpriteRenderer AimRenderer;
    private Vector2 lastAim = new Vector2(-1, 0);

    [Header("Dagger Settings")]
    public float DaggerSpeed;
    public GameObject DaggerProjectile;
    public GameObject DaggerThrowFX;

    [Header("Brain Settings")]
    public float BrainSpeed;
    public GameObject BrainProjectile;
    public GameObject BrainThrowFX;

    [Header("Gun Settings")]
    public float GunSpeed;
    public float GunKnockback;
    public GameObject GunProjectile;
    public GameObject GunShootFX;
    public Animator GunAnimator;
    private bool reloading = false;

    [HideInInspector]
    public EquipmentList EquipmentList;
    [HideInInspector]
    public List<IPullable> PullList = new List<IPullable>();

    private Vector2 lastControllerTarget, lastMouseTarget;
    private bool OnController = false;

    private void Start()
    {
        EquipmentList = FindObjectOfType<EquipmentList>();

        /*
        if (!EquipmentList)
            Debug.Log("No Equipment List found");
        */

        AimRenderer = AimPointer.GetComponentInChildren<SpriteRenderer>();
        if (!GameManager.HasController)
            AimPointer.SetActive(false);

        var gunRenderer = GunAnimator.transform.GetComponent<SpriteRenderer>();
        gunRenderer.enabled = false;
        GunAnimator.enabled = false;

        Freeze(.5f);
    }

    public override void EntityUpdate()
    {
        if (!IsAlive)
        {
            Stop();
            return;
        }

        var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Move(move);

        if (currentCd > 0)
            currentCd -= Time.deltaTime;

        //Controller Aim Aid
        var aimDirection = new Vector2(Input.GetAxisRaw("Horizontal2"), Input.GetAxisRaw("Vertical2"));
        if (aimDirection.sqrMagnitude != 0)
        {
            lastAim = aimDirection;
            var aimAngle = Quaternion.FromToRotation(Vector3.up, lastAim.normalized);
            AimPointer.transform.rotation = aimAngle;

            //AimPointer.transform.localScale = Vector3.one;
            AimRenderer.color = Color.white;
        }
        else
        {
            //AimPointer.transform.localScale = Vector3.one * .8f;
            //AimRenderer.color = new Color(1, 1, 1, .6f);
            AimRenderer.color = new Color(1, 1, 1, 0);
        }

        //Check if user uses mouse or controller to aim
        var controllerAim = lastAim;
        var mouseAim = (Vector2)Input.mousePosition;

        if (controllerAim != lastControllerTarget)
            OnController = true;
        else if (mouseAim != lastMouseTarget)
            OnController = false;

        lastControllerTarget = controllerAim;
        lastMouseTarget = mouseAim;

        //Shoot
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {

            if (OnController)
            {
                Shoot((Vector2)transform.position + lastAim);
            }
            else
                Shoot(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        //Secondary / Use
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            OnController = Input.GetKeyDown(KeyCode.JoystickButton4);
            UseActivators();
            TogglePullables();
        }

        //Release Pulled Things that are to far away
        if(PullList.Count > 0)
        {
            foreach (var entity in PullList.ToArray())
                if ((entity.Renderer.transform.position - transform.position).sqrMagnitude > PullRange * PullRange)
                {
                    entity.SetPull(false, this);
                    PullList.Remove(entity);
                }
        }
        

    }

    void UseActivators()
    {
        Activator[] activators = FindObjectsOfType<Activator>().Where(x => x.ManualControllable && Vector2.Distance(transform.position, x.transform.position) < ActivatorRange).ToArray();
        foreach (Activator activator in activators)
            activator.Toggle();
    }

    void TogglePullables()
    {

        var entities = FindObjectsOfType<Object>()
            .Where(x => x is IPullable)
            .Cast<IPullable>()
            .Where(x => x.IsPullable)
            .Where(x => !PullList.Contains(x))
            .Where(x => (transform.position - x.Renderer.transform.position).sqrMagnitude < PullRange * PullRange)
            .ToArray();

        PullList.ForEach(x => x.SetPull(false, this));
        PullList.Clear();
        PullList.AddRange(entities);

        foreach (var entity in PullList)
            entity.SetPull(true, this);
    }

    void Shoot(Vector3 target)
    {
        if (currentCd > 0 || reloading)
            return;
        currentCd = AttackCooldown;

        if(!EquipmentList)
            ShootDagger(target);
        else
            switch (EquipmentList.GetSelectedItem())
            {
                case EquipmentList.EquipmentType.Dagger:
                    ShootDagger(target);

                    if (Daggers <= 0)
                        AdjustSelection();
                    break;
                case EquipmentList.EquipmentType.Gun:
                    ShootGun(target);

                    if (Bullets <= 0)
                        AdjustSelection();
                    break;
                case EquipmentList.EquipmentType.Boomerang:
                    Debug.Log("Throw boomerang");
                    break;
                case EquipmentList.EquipmentType.Brain:
                    ShootBrain(target);

                    if (Brains <= 0)
                        AdjustSelection();
                    break;
                default:
                    Debug.Log("Shoot what?");
                    break;
            }
    }

    private void AdjustSelection()
    {
        if (Daggers > 0)
            EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Dagger);
        else if (Brains > 0)
            EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Brain);
        else if (Bullets > 0)
            EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Gun);
        else if (Boomerangs > 0)
            EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Boomerang);
    }

    private void ShootDagger(Vector2 target)
    {
        if (Daggers <= 0)
            return;

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Throw, transform);

        Vector2 diretion = (target - (Vector2)transform.position).normalized;

        GameObject dagger = Instantiate(DaggerProjectile, (Vector2)transform.position + diretion * .3f, Quaternion.identity);
        dagger.transform.Rotate(Vector3.forward, Mathf.Atan(diretion.y / diretion.x) * Mathf.Rad2Deg + (diretion.x < 0 ? 90 : -90));
        Rigidbody2D daggerBody = dagger.GetComponent<Rigidbody2D>();

        if (daggerBody)
        {
            daggerBody.velocity = diretion * DaggerSpeed;
            GainItem(dagger: -1);

            SpriteRenderer throwFx = Instantiate(DaggerThrowFX, transform).GetComponent<SpriteRenderer>();

            throwFx.gameObject.transform.localScale = Vector3.up + Vector3.forward + (transform.localScale.x < 0 ? Vector3.left : Vector3.right);

            if (diretion.x > 0)
            {
                throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.right, diretion);
                throwFx.flipX = false;
            }
            else
            {
                throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.left, diretion);
                throwFx.flipX = true;
            }
        }
        else
            Destroy(dagger);
    }

    private void ShootBrain(Vector2 target)
    {
        if (Brains <= 0)
            return;

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Throw, transform);

        Vector2 diretion = (target - (Vector2)transform.position).normalized;

        GameObject brain = Instantiate(BrainProjectile, transform.position, Quaternion.identity);
        //brain.transform.Rotate(Vector3.forward, Mathf.Atan(diretion.y / diretion.x) * Mathf.Rad2Deg + (diretion.x < 0 ? 90 : -90));
        Rigidbody2D brainBody = brain.GetComponent<Rigidbody2D>();

        if (brainBody)
        {
            brainBody.velocity = diretion * BrainSpeed;
            GainItem(brain: -1);

            SpriteRenderer throwFx = Instantiate(BrainThrowFX, transform).GetComponent<SpriteRenderer>();

            throwFx.gameObject.transform.localScale = Vector3.up + Vector3.forward + (transform.localScale.x < 0 ? Vector3.left : Vector3.right);

            if (diretion.x > 0)
            {
                throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.right, diretion);
                throwFx.flipX = false;
            }
            else
            {
                throwFx.transform.rotation = Quaternion.FromToRotation(Vector3.left, diretion);
                throwFx.flipX = true;
            }
        }
        else
            Destroy(brain);
    }

    private void ShootGun(Vector2 target)
    {
        if (Bullets <= 0 || reloading)
            return;

        reloading = true;

        Vector2 direction = (target - (Vector2)transform.position).normalized;

        Remote(.2f);
        Rigidbody2D.velocity = Vector3.zero;
        Rigidbody2D.AddForce(-direction * GunKnockback * Rigidbody2D.mass, ForceMode2D.Impulse);
        transform.localScale = Vector3.up + Vector3.forward + Vector3.right * (direction.x > 0 ? -1 : 1);

        StartCoroutine(DisplayGunshot(target));

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Shoot, transform, 1);

        GameObject bullet = Instantiate(GunProjectile, (Vector2)transform.position + new Vector2(direction.x * (direction.x > 0 ? -1 : 1), direction.y) * .3f, Quaternion.identity);
        bullet.transform.Rotate(Vector3.forward, Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg);
        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();

        if (bulletBody)
        {
            bulletBody.velocity = direction * GunSpeed;
            GainItem(bullet: -1);

            SpriteRenderer shootFx = Instantiate(GunShootFX, transform).GetComponent<SpriteRenderer>();
            shootFx.transform.localPosition = new Vector2(direction.x * (direction.x > 0 ? -1 : 1), direction.y);
            shootFx.gameObject.transform.localScale = Vector3.one * FXScale * 1.2f;

            if (direction.x > 0)
            {
                shootFx.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
                shootFx.flipX = false;
            }
            else
            {
                shootFx.transform.rotation = Quaternion.FromToRotation(Vector3.left, direction);
                shootFx.flipX = true;
            }
        }
        else
            Destroy(bullet);

        //Alert all Monsters
        foreach (var enemy in FindObjectsOfType<Enemy>())
            enemy.GainAggro(this);
    }

    private IEnumerator DisplayGunshot(Vector2 target)
    {

        Vector2 direction = (target - (Vector2)transform.position).normalized;

        var gunRenderer = GunAnimator.transform.GetComponent<SpriteRenderer>();
        gunRenderer.enabled = true;

        if (direction.x >= 0)
            gunRenderer.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
        else
            gunRenderer.transform.rotation = Quaternion.FromToRotation(Vector3.left, direction);

        yield return new WaitForSeconds(.2f);

        Channel(.9f);
        GunAnimator.enabled = true;
        GunAnimator?.SetTrigger("Reload");

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.GunReload, transform);

        yield return new WaitForSeconds(1f);
        GunAnimator.enabled = false;
        gunRenderer.enabled = false;
        reloading = false;
    }

    public void GainItem(int dagger = 0, int bullet = 0, int boomerang = 0, int brain = 0)
    {

        Daggers += dagger;
        Bullets += bullet;
        Boomerangs += boomerang;
        Brains += brain;

        if (EquipmentList)
        {
            EquipmentList.OnItemChange(this);

            if(dagger > 0)
                EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Dagger);
            else if (brain > 0)
                EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Brain);
            else if (bullet > 0)
                EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Gun);
            else if (boomerang > 0)
                EquipmentList.ChangeSelection(EquipmentList.EquipmentType.Boomerang);
        }
    }

    public override void Die()
    {
        Animator.SetBool("Dead", true);
        StartCoroutine(ReloadScene());
    }

    public override void TakeDamage(int damage = 1, Entity attacker = null)
    {
        base.TakeDamage(damage, attacker);
        GameManager.DamageFlash();
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(.1f);
        if (GameManager.Instance)
            GameManager.Instance.TransitionController.LoadScene(SceneManager.GetActiveScene().name);
        else
            Debug.Log("You Died!");
    }
}
