using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCollection : MonoBehaviour
{
    [Header("UI")]
    public AudioClip Equip;

    [Header("Player")]
    public AudioClip Walk;
    public AudioClip Throw;
    public AudioClip Shoot;
    public AudioClip PlayerDamage;
    public AudioClip PickUp;
    public AudioClip DaggerImpact;
    public AudioClip DaggerImpactEnemy;
    public AudioClip BrainImpact;
    public AudioClip BulletImpact;
    public AudioClip BulletImpactEnemy;
    public AudioClip GunReload;

    [Header("Knight")]
    public AudioClip KnightDamage;
    public AudioClip Sword;
    public AudioClip KnightWalk;
    public AudioClip KnightSpawn;

    [Header("Demon")]
    public AudioClip DemonDamage;
    public AudioClip DemonAttack;
    public AudioClip DemonWalk;
    public AudioClip DemonSpawn;

    [Header("Zombie")]
    public AudioClip Attack;
    public AudioClip ZombieDamage;
    public AudioClip FallOver;
    public AudioClip ZombieWalk;
    public AudioClip ZombieEat;

    [Header("Orge")]
    public AudioClip OrgeDamage;
    public AudioClip OrgeAttack;
    public AudioClip OrgeWalk;
    public AudioClip OrgeWake;

    [Header("Elf")]
    public AudioClip ElfDamage;
    public AudioClip ElfPresent;
    public AudioClip ElfWalk;

    [Header("World")]
    public AudioClip Mimic;
    public AudioClip MimicBefriend;
    public AudioClip ChestOpen;
    public AudioClip GateOpen;
    public AudioClip GateClose;
    public AudioClip SpikesActivate;
    public AudioClip SpikesAttack;
    public AudioClip Switch;

    public static AudioCollection Instance { get; private set; }

    public void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;
    }
}
