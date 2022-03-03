using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAllSwitch : Activatable
{
    private void Awake()
    {
        if (GameManager.Instance)
            Active = GameManager.Instance.UnlockAll;
    }

    protected override bool OnSetActive(bool active)
    {
        if(GameManager.Instance)
            return GameManager.Instance.UnlockAll = active;
        return false;
    }
}
