using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPullable
{
    SpriteRenderer Renderer { get; }
    
    bool IsPullable { get; }

    void SetPull(bool state, Entity player);

}
