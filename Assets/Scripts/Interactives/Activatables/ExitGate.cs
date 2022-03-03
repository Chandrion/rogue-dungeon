using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class ExitGate : Activatable
{
    private Player Player;
    private Cinemachine.CinemachineVirtualCamera VirtualCamera;

    public string NextLevel;

    public bool MoveDown = false;

    public bool ShowLabel = true;

    private void Start()
    {
        Player = FindObjectOfType<Player>();
        VirtualCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();

        SetDoor(Active);
    }

    protected override bool OnSetActive(bool active)
    {
        if(Player && VirtualCamera)
            StartCoroutine(OpenSequence(active));
        return active;
    }

    private IEnumerator OpenSequence(bool active)
    {
        FindObjectsOfType<Entity>().ToList().ForEach(x => x.Freeze(1.1f));

        VirtualCamera.Follow = transform;
        yield return new WaitForSeconds(.5f);
        SetDoor(active);
        yield return new WaitForSeconds(.5f);
        VirtualCamera.Follow = Player.transform;
    }

    private void SetDoor(bool active)
    {
        GetComponent<Animator>().SetBool("Open", active);
        GetComponent<BoxCollider2D>().isTrigger = active;

        if (AudioCollection.Instance)
        {
            if (active)
                AudioManager.PlaySound(AudioCollection.Instance.GateOpen, transform);
            else
                AudioManager.PlaySound(AudioCollection.Instance.GateClose, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform == Player.transform && Player.IsAlive)
        {
            Player.Invincible(2);
            if (GameManager.Instance)
            {
                GameManager.Instance.LevelCleared(SceneManager.GetActiveScene().name);
                GameManager.Instance.TransitionController.LoadScene(NextLevel, ShowLabel);
                GameManager.Instance.TransitionController.StartSteps();
                StartCoroutine(MovePlayer());
            }
            else
                Debug.Log("Level Complete!");
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator MovePlayer()
    {
        Player.Channel(1);
        while (transform.position.y - Player.transform.position.y > 0 != MoveDown)
        {
            Player.Move(MoveDown ? Vector2.down : Vector2.up);
            yield return new WaitForEndOfFrame();
        }
    }


}
