using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Clock : Activator
{
    public override bool ManualControllable => false;

    public override bool Controllable => true;

    public float ToggleTimer = 5;
    private float timerProgress = 0;

    public List<Activator> Conditions = new List<Activator>();

    protected override void OnToggle(bool newVal)
    {
        //Nothing special
    }

    private void Update()
    {
        bool conditionsMet = Conditions.Select(x => x.Active).Aggregate(true, (current, next) => current && next);
        if (conditionsMet)
            timerProgress += Time.deltaTime;

        if(timerProgress >= ToggleTimer)
        {
            timerProgress = 0;
            Toggle();
        }
    }

}
