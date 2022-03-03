using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : Activatable
{
    [Header("Monster Settings")]
    public GameObject MonsterPrefab;
    public List<Enemy> Monsters;
    public int MaxMonsters;

    [Header("Spawn Settings")]
    public float SpawnTimer;
    public float SpawnFreeze = .5f;
    public float SpawnDistance;
    private float SpawnCooldown;

    [Header("Prefabs")]
    public GameObject SpawnFX;

    protected override bool OnSetActive(bool active)
    {
        GetComponent<Animator>().SetBool("Active", active);

        return active;
    }

    void Start()
    {
        SpawnCooldown = 0;
    }

    void Update()
    {
        SpawnCooldown += Time.deltaTime;

        if (SpawnCooldown > SpawnTimer)
        {
            SpawnCooldown = Random.Range(SpawnTimer * -.1f, SpawnTimer * .1f);
            Monsters.RemoveAll(x => x == null);

            if(Active && Monsters.Count < MaxMonsters)
            {
                GameObject newMonster = Instantiate(MonsterPrefab, transform.position + Vector3.down * SpawnDistance, Quaternion.identity);
                Monsters.Add(newMonster.GetComponent<Enemy>());
                newMonster.GetComponent<Enemy>().Freeze(SpawnFreeze);
                Instantiate(SpawnFX, newMonster.transform.position, Quaternion.identity);
                if(AudioCollection.Instance)
                    AudioManager.PlaySound(AudioCollection.Instance.DemonSpawn, transform);
            }
        }
    }

}
