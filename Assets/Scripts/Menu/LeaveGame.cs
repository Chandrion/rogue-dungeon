using System.Collections;
using UnityEngine;

public class LeaveGame : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            FindObjectOfType<Cinemachine.CinemachineVirtualCamera>().Follow = null;// transform;
            player.Remote(10);
            StartCoroutine(ExitGame(player));
        }
    }

    private IEnumerator ExitGame(Player player)
    {
        while(transform.position.y - player.transform.position.y < 5)
        {
            player.Move(Vector2.down);
            yield return new WaitForEndOfFrame();
        }

        GameManager.Instance.TransitionController.QuitGame();

        while (transform.position.y - player.transform.position.y < 5)
        {
            player.Move(Vector2.down);
            yield return new WaitForEndOfFrame();
        }
    }
}
