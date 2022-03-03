using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Box))]
public class PresentBox : MonoBehaviour
{
    private Box Box;

    public SpriteRenderer TopRenderer, BottomRenderer;

    public int Type = -1;
    private static int Presents = 0;

    public List<Sprite> TopSprites, BottomSprites;

    void Start()
    {
        Box = GetComponent<Box>();
        if(!TopRenderer && !BottomRenderer)
        {
            BottomRenderer = Box.Renderer;
            TopRenderer = GetComponentsInChildren<SpriteRenderer>().ToList().Where(x => x != BottomRenderer).FirstOrDefault();

            if (!BottomRenderer || !TopRenderer)
                Destroy(gameObject);
        }

        int entries = Mathf.Min(TopSprites.Count, BottomSprites.Count);
        if (entries > 0)
        {

            Type = Type < 0 ? Presents : Mathf.Clamp(Type, 0, entries-1);
            Presents = ++Presents % entries;

            TopRenderer.sprite = TopSprites[Type];
            BottomRenderer.sprite = BottomSprites[Type];
        }

    }

}
