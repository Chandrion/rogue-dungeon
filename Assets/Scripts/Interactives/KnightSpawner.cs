using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnightSpawner : MonoBehaviour
{
    private Player Player;
    private Cinemachine.CinemachineVirtualCamera VirtualCamera;

    public GameObject KnightPrefab;
    public GameObject SpawnPrefab;

    public float Timer;
    private float TimePassed = 0;
    private int iterations = 0;

    private List<Entity> knights = new List<Entity>();
    public int MaxKnights = 5;

    private void Start()
    {
        Player = FindObjectOfType<Player>();
        VirtualCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
    }

    private void Update()
    {
        TimePassed += Time.deltaTime;

        if(TimePassed > Timer)
        {
            iterations++;
            TimePassed = Mathf.Min(Timer-3, Timer * iterations*2 / (1+iterations*2));

            knights = knights.Where(x => x).ToList();

            if(knights.Count < MaxKnights)
            {
                AlertManager.Instance?.Alert(Player);
                StartCoroutine(DrawFocus());
            }
        }

    }

    private IEnumerator DrawFocus()
    {
        if(true || iterations < 5)
        {
            FindObjectsOfType<Entity>().ToList().ForEach(x => x.Freeze(1.2f));
            yield return new WaitForSeconds(.2f);

            VirtualCamera.Follow = transform;
            yield return new WaitForSeconds(.5f);
        }

        var knight = Instantiate(KnightPrefab, transform.position, Quaternion.identity).GetComponent<Entity>();
        knight.Freeze(.5f);
        knights.Add(knight);

        Instantiate(SpawnPrefab, transform.position, Quaternion.identity).transform.localScale *= 2;

        if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.KnightSpawn, transform, pitch: 1.2f);

        if (true || iterations < 5)
        {
            yield return new WaitForSeconds(.5f);
            VirtualCamera.Follow = Player.transform;
        }
    }
}
