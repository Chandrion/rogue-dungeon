using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Elf : SmartEnemy
{
    public override AudioClip DamageSound { get => AudioCollection.Instance.ElfDamage; }
    public override AudioClip StepSound => AudioCollection.Instance.ElfWalk;
    public override float AudioPitch => 1f;
    public override float DamagePitch => 1.3f;

    [Header("Elf Specifics")]
    public bool Female = false;

    public float PresentCooldown = 3;
    public float BoxRecognisionRange = 3f;
    public float PresentMakingRange = 1.5f;
    public GameObject PresentPrefab;
    public GameObject PresentFX;

    public GameObject MessagePrefab;
    public string idleText, PresentText, CustomText;
    public Activator customTextTrigger;
    [HideInInspector]
    public GameObject CurrentMessage;
    private Canvas HUD;

    public List<Sprite> PresentTopSprites, PresentBottomSprites;
    private GameObject targetBox = null;

    public  new void Start()
    {
        base.Start();
        Animator.SetBool("Female", Female);
        HUD = FindObjectOfType<Canvas>();

        CurrentMessage = Instantiate(MessagePrefab, HUD.transform);
        CurrentMessage.GetComponent<AlignUIToGO>().AlignTo = gameObject;
        CurrentMessage.GetComponent<ShowWhenPlayerNearby>().Target = gameObject;
        CurrentMessage.GetComponent<Text>().text = idleText;
    }

    public override void EntityUpdate()
    {
        if(targetBox == null)
        {
            Box[] newBoxes = FindObjectsOfType<Box>();
            SmartZombie[] zombies = FindObjectsOfType<SmartZombie>();

            List<GameObject> gifts = new List<GameObject>();
            foreach (var box in newBoxes)
                gifts.Add(box.gameObject);
            foreach (var zombie in zombies)
                if (zombie.IsDead)
                    gifts.Add(zombie.Animator.gameObject);

            GameObject plainBox = null;
            float range = BoxRecognisionRange * BoxRecognisionRange;

            foreach (var box in gifts)
                if (!box.GetComponent<PresentBox>() && (box.transform.position - transform.position).sqrMagnitude < range)
                {
                    plainBox = box;
                    range = (box.transform.position - transform.position).sqrMagnitude;
                }

            targetBox = plainBox;

            if (plainBox == null)
            {
                base.EntityUpdate();

                var message = CurrentMessage.GetComponent<Text>();
                if (customTextTrigger.Active)
                    message.text = CustomText;
                else if(message.text == CustomText)
                    message.text = idleText;

            }
            else
            {
                CurrentMessage.GetComponent<Text>().text = PresentText;
                StopCoroutine("ResetTest");
                AlertManager.Instance?.Alert(this);
            }
        }
        else
        {
            float distance = (targetBox.transform.position - transform.position).sqrMagnitude;

            if (distance > BoxRecognisionRange * BoxRecognisionRange)
            {
                AlertManager.Instance?.Lose(this);
                targetBox = null;
            }
            else if(distance > PresentMakingRange * PresentMakingRange)
            {
                if (!targetBox || targetBox.GetComponent<PresentBox>())
                {
                    targetBox = null;
                    return;
                }
                Move(targetBox.transform.position - transform.position);
            }
            else
            {
                MakePresent();
            }
        }
    }

    public virtual void MakePresent()
    {
        float distance = (targetBox.transform.position - transform.position).sqrMagnitude;
        if (distance > PresentMakingRange * PresentMakingRange)
            return;
        if (targetBox && targetBox.GetComponent<PresentBox>())
        {
            targetBox = null;
            return;
        }
        SmartZombie zombie;
        if ((zombie = targetBox.GetComponentInParent<SmartZombie>()) != null && zombie.Puller)
        {
            var box = Instantiate(PresentPrefab, zombie.Animator.transform.position, Quaternion.identity).GetComponent<Box>();
            box.transform.localScale *= 1.5f;
            Instantiate(PresentFX, zombie.Animator.transform.position, Quaternion.identity).transform.localScale = Vector3.one * 3;
            (zombie.Puller as Player).PullList.Remove(zombie);
            zombie.SetPull(false, zombie.Puller);
            Destroy(targetBox.transform.parent.gameObject);
        }
        else
        {
            Instantiate(PresentFX, targetBox.transform.position, Quaternion.identity).transform.localScale = Vector3.one * 2;
            var present = targetBox.gameObject.AddComponent<PresentBox>();
            present.TopSprites = PresentTopSprites;
            present.BottomSprites = PresentBottomSprites;
        }

        if (AudioCollection.Instance)
        {
            AudioManager.PlaySound(AudioCollection.Instance.ElfPresent, transform, .4f, pitch: AudioPitch);
            AudioManager.PlaySound(AudioCollection.Instance.ElfPresent, transform, .4f, pitch: -AudioPitch, .4f);
        }

        Freeze(PresentCooldown);
        StartCoroutine(ResetTest(3));
    }

    private IEnumerator ResetTest(float timer)
    {
        yield return new WaitForSeconds(timer);
        CurrentMessage.GetComponent<Text>().text = idleText;
    }

    public override void GainAggro(Entity entity)
    {
        // This one won't attack
    }

    public override void TakeDamage(int damage = 1, Entity entity = null)
    {
        base.TakeDamage(0, entity);
        //He won't die either
    }

    protected override bool AttackTargetsIfNearby()
    {
        return false;
    }

    protected override bool EngageTargetIfAggroed()
    {
        return false;
    }

    protected override bool GainAggroOnPlayerIfVisible()
    {
        return false;
    }

    protected override void UpdateAggroTarget()
    {
        LoseAggro();
    }

}
