using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Enemy
{
    public override AudioClip AttackSound { get => AudioCollection.Instance.Sword; }
    public override AudioClip DamageSound { get => AudioCollection.Instance.KnightDamage; }
    public override AudioClip StepSound => AudioCollection.Instance.KnightWalk;
    public override float AudioPitch => 1f;
    public override float DamagePitch => 1.3f;

    [Header("Knight Specific")]
    public Sprite Sword;
    public Sprite Axe;
    public Sprite Hammer;
    public Sprite GoldSword;

    public enum Weapon { Sword, Axe, Hammer, GoldSword}
    public Weapon EquippedWeapon;

    public SpriteRenderer WeaponSprite;

    private Quaternion BaseRotation;

    public new void Start()
    {
        base.Start();

        BaseRotation = WeaponSprite.transform.rotation;
        EquipRandomWeapon();
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer();

        Vector2 direction = (Player.transform.position - transform.position).normalized;

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
        for(float timePassed = 0; timePassed < duration; timePassed += Time.deltaTime)
        {
            WeaponSprite.transform.Rotate(new Vector3(0, 0, angle * Time.deltaTime / duration));
            yield return new WaitForEndOfFrame();
        }

        WeaponSprite.transform.localRotation = BaseRotation;
    }

    private void EquipRandomWeapon()
    {
        int rand = Random.Range(0, 100);
        
        if(rand < 33)
        {
            //Sword
            //Shorter Attack Cooldown 
            EquippedWeapon = Weapon.Sword;
            WeaponSprite.sprite = Sword;
            AttackCooldown *= .7f;

        }
        else if (rand < 66)
        {
            //Axe
            //More Speed
            EquippedWeapon = Weapon.Axe;
            WeaponSprite.sprite = Axe;
            MoveSpeed *= 1.3f;
        }
        else if (rand < 99)
        {
            //Hammer
            //More Attack Range
            EquippedWeapon = Weapon.Hammer;
            WeaponSprite.sprite = Hammer;
            AttackRange *= 1.2f;
        }
        else
        {
            //GoldSword
            //Everything!
            EquippedWeapon = Weapon.GoldSword;
            WeaponSprite.sprite = GoldSword;
            AttackCooldown *= .7f;
            MoveSpeed *= 1.5f;
            AttackRange *= 1.2f;
        }


    }

}
