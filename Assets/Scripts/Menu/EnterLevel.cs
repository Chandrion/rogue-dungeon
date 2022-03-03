using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterLevel : MonoBehaviour
{
    [Tooltip("Enter \"Level X\" to enter last level unlocked")]
    public string TargetLevel;

    public bool MoveDown = false;

    private Player Player;

    private bool Unlocked;

    private void Awake()
    {
        if (TargetLevel == "Level X")
            TargetLevel = $"Level {GameManager.Instance.LastLevelUnlocked}";
        
        if (TargetLevel == "Level Selection" && GameManager.Instance.LevelsUnlocked == 0)
            Unlocked = false;
        else
            Unlocked = GameManager.Instance.IsUnlocked(TargetLevel);

        GetComponent<Animator>().SetBool("Open", Unlocked);

        if (!Unlocked)
            GetComponent<Collider2D>().isTrigger = false;
    }
    private void Start()
    {
        Player = FindObjectOfType<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.TransitionController.LoadScene(TargetLevel);
        GameManager.Instance.TransitionController.StartSteps();
        StartCoroutine(MovePlayer());
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
