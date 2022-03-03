using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [SerializeField]
    private GameObject AlertPrefab = null;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Alert(Entity entity)
    {
        StartCoroutine(ShowAlert(entity));
    }
    public void Lose(Entity entity)
    {
        StartCoroutine(ShowLose(entity));
    }

    private IEnumerator ShowAlert(Entity enemy)
    {
        var collider = enemy.GetComponentInChildren<Collider2D>();

        float offset = 0;
        
        if(collider is BoxCollider2D box)
           offset = box.size.y/2 + box.offset.y;
        else if(collider is CircleCollider2D circle)
            offset = circle.radius + circle.offset.y;
        else if (collider is CapsuleCollider2D capsule)
            offset = capsule.size.y / 2 + capsule.offset.y;

        var alert = Instantiate(AlertPrefab, transform);
        var alertText = alert.GetComponent<Text>();
        alertText.text = "!";

        var growDuration = .1f;
        var fadeDuration = .05f;

        for (float time = 0; time < growDuration && enemy; time += Time.deltaTime)
        {
            alert.transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * offset);
            alert.transform.localScale = Vector3.right + Vector3.forward + Vector3.up * (.5f + .5f * (time / growDuration));
            yield return new WaitForEndOfFrame();
        }

        for (float time = 0; time < fadeDuration && enemy; time += Time.deltaTime)
        {
            alert.transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * offset);
            alert.transform.localScale = Vector3.right + Vector3.forward + Vector3.up * (1 - .2f * (time / fadeDuration));
            alertText.color = new Color(1, 1, 1, 1 - (time / fadeDuration));
            yield return new WaitForEndOfFrame();
        }

        Destroy(alert);
    }

    private IEnumerator ShowLose(Entity enemy)
    {
        var collider = enemy.GetComponentInChildren<Collider2D>();

        float offset = 0;

        if (collider is BoxCollider2D box)
            offset = box.size.y / 2 + box.offset.y;
        else if (collider is CircleCollider2D circle)
            offset = circle.radius + circle.offset.y;
        else if (collider is CapsuleCollider2D capsule)
            offset = capsule.size.y / 2 + capsule.offset.y;

        var alert = Instantiate(AlertPrefab, transform);
        var alertText = alert.GetComponent<Text>();
        alertText.text = "?";

        var growDuration = .3f;
        var fadeDuration = .1f;

        for (float time = 0; time < growDuration && enemy; time += Time.deltaTime)
        {
            alert.transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * offset);
            alert.transform.localScale = Vector3.right + Vector3.forward + Vector3.up * (.5f + .5f * (time / growDuration));
            yield return new WaitForEndOfFrame();
        }

        for (float time = 0; time < fadeDuration && enemy; time += Time.deltaTime)
        {
            alert.transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + Vector3.up * offset);
            alert.transform.localScale = Vector3.right + Vector3.forward + Vector3.up * (1 - .2f * (time / fadeDuration));
            alertText.color = new Color(1, 1, 1, 1 - (time / fadeDuration));
            yield return new WaitForEndOfFrame();
        }

        Destroy(alert);
    }
}
