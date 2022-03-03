using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Activator
{
    public override bool ManualControllable => false;

    public override bool Controllable => true;

    private List<Object> entitiesOnTop = new List<Object>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Projectile>())
            return;

        var entity = collision.gameObject.GetComponentInParent<Entity>();
        var box = collision.gameObject.GetComponentInParent<Box>();
        if (entity || box)
        {
            if(entitiesOnTop.Count == 0)
                Toggle();

            if (entity)
                entitiesOnTop.Add(entity);
            else
                entitiesOnTop.Add(box);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Projectile>())
            return;

        var entity = collision.GetComponentInParent<Entity>();
        var box = collision.gameObject.GetComponentInParent<Box>();
        if (entity || box)
        {
            if(entity)
                entitiesOnTop.Remove(entity);
            else
                entitiesOnTop.Remove(box);

            if (entitiesOnTop.Count == 0)
                Toggle();
        }
    }

    protected override void OnToggle(bool newVal)
    {
        if(AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Switch, transform);
    }
}
