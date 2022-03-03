using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Box : MonoBehaviour, IPullable
{
    public SpriteRenderer Renderer => renderer;

    public bool IsPullable => true;

    [Header("Setup")]
    [SerializeField]
    private new SpriteRenderer renderer = null;
    public GameObject DestroyFx;

    private Entity puller;
    private Vector3 pullOffset;
    private Rigidbody2D Rigidbody2D;
    private Player player;
    private float StopForce = 5;

    [Header("Loot")]
    public int Daggers;
    public int Brains;
    public int Bullets;

    [Header("Prefabs")]
    public GameObject DaggerHint;
    public GameObject BrainHint;
    public GameObject GunHint;

    private void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (puller)
        {
            puller.Channel(.05f);

            Rigidbody2D.MovePosition(
                puller.transform.position + (Vector3)pullOffset -
                new Vector3(Renderer.transform.localPosition.x * (transform.localScale.x < 0 ? -1 : 1), Renderer.transform.localPosition.y)
                );

        }
        else
            Stop();
    }

    public void Destroy()
    {
        Instantiate(DestroyFx, transform.position, Quaternion.identity).transform.localScale = Vector3.one * 2;
        player.GainItem(Daggers, Bullets, 0, Brains);
        StartCoroutine(ShowHints(transform.position));

        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = false;

    }
    private IEnumerator ShowHints(Vector3 position)
    {
        List<GameObject> UnlockNotes = new List<GameObject>();
        UnlockNotes.AddRange(Enumerable.Repeat(DaggerHint, Daggers));
        UnlockNotes.AddRange(Enumerable.Repeat(BrainHint, Brains));
        UnlockNotes.AddRange(Enumerable.Repeat(GunHint, Bullets));

        foreach (GameObject hint in UnlockNotes)
        {
            Instantiate(hint, position, Quaternion.identity);
            yield return new WaitForSeconds(.3f);
        }

        Destroy(gameObject);
    }

    private void Stop()
    {
        var direction = Vector2.zero;
        Vector2 velocityDelta = direction - Rigidbody2D.velocity;

        velocityDelta.x = Mathf.Clamp(velocityDelta.x, -StopForce, StopForce);
        velocityDelta.y = Mathf.Clamp(velocityDelta.y, -StopForce, StopForce);

        Rigidbody2D.AddForce(velocityDelta * Rigidbody2D.mass, ForceMode2D.Impulse);
    }

    public void SetPull(bool state, Entity player)
    {
        if (state && IsPullable)
        {
            puller = player;
            pullOffset = Renderer.transform.position - player.transform.position;

            float minDistance;
            float currentDistance = pullOffset.magnitude;
            float distanceFromZombie = Physics2D.Raycast(Renderer.transform.position, -pullOffset, float.PositiveInfinity, 256).distance;
            float distanceFromPlayer = Physics2D.Raycast(puller.transform.position, pullOffset, float.PositiveInfinity, 4096).distance;

            minDistance = (currentDistance - distanceFromPlayer) + (currentDistance - distanceFromZombie);
            minDistance *= 1.1f;

            pullOffset *= 1 + (minDistance - currentDistance) / currentDistance;
        }
        else
        {
            pullOffset = Vector2.zero;
            puller = null;
        }
    }

}
