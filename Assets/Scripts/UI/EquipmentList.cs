using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EquipmentList : MonoBehaviour
{
    [Header("List Settings")]
    public List<RectTransform> ListItems;
    private int SelectedItem = 0;

    private RectTransform daggerItem;
    private RectTransform gunItem;
    private RectTransform boomerangItem;
    private RectTransform brainItem;

    [Header("Transition Settings")]
    public float SmallSize;
    public float BigSize;
    public float TransitionDuration;

    private bool inTransition;

    [Header("Prefabs")]
    public GameObject DaggerIcon;
    public GameObject GunIcon;
    public GameObject BoomerangIcon;
    public GameObject BrainIcon;

    public enum EquipmentType { None, Dagger, Gun, Boomerang, Brain}

    private bool LeftTrigger { get => Input.GetAxisRaw("LeftTrigger") > 0; }
    private bool RightTrigger { get => Input.GetAxisRaw("RightTrigger") > 0; }

    private bool NumberKey { get => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3); }

    private bool lastLeftTrigger = false;
    private bool lastRightTrigger = false;
    private bool LeftTriggerDown { get => !lastLeftTrigger && LeftTrigger; }
    private bool RightTriggerDown { get => !lastRightTrigger && RightTrigger; }

    private void Start()
    {
        OnItemChange(FindObjectOfType<Player>());
    }

    private void LateUpdate()
    {
        var mWheel = Input.GetAxisRaw("Mouse ScrollWheel");

        if (ListItems.Count > 1 && (mWheel != 0 || LeftTriggerDown || RightTriggerDown || NumberKey) && !inTransition)
        {
            int newItem = SelectedItem;

            if (mWheel != 0)
            {
                if (mWheel < 0)
                    newItem = (SelectedItem + 1) % ListItems.Count;
                else
                    newItem = (SelectedItem + ListItems.Count - 1) % ListItems.Count;
            }
            else if (RightTriggerDown)
                newItem = (SelectedItem + 1) % ListItems.Count;
            else if (LeftTriggerDown)
                newItem = (SelectedItem + ListItems.Count - 1) % ListItems.Count;
            else if (Input.GetKeyDown(KeyCode.Alpha1) && ListItems.Count > 0)
                newItem = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2) && ListItems.Count > 1)
                newItem = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3) && ListItems.Count > 2)
                newItem = 2;
            else
                return;

            ChangeSelectionByIndex(newItem);
        }

        lastLeftTrigger = LeftTrigger;
        lastRightTrigger = RightTrigger;
    }

    public void ChangeSelection(EquipmentType newItem) => ChangeSelectionByIndex(GetItemIndex(newItem));

    private void ChangeSelectionByIndex(int newItemIndex)
    {
        if (newItemIndex < 0 || newItemIndex >= ListItems.Count || newItemIndex == SelectedItem)
            return;

        StartCoroutine(Shift(ListItems[SelectedItem], ListItems[newItemIndex]));

        if (AudioCollection.Instance)
            AudioManager.PlaySound(AudioCollection.Instance.Equip, Camera.main.transform);

        SelectedItem = newItemIndex;
    }

    public EquipmentType GetSelectedItem() => GetItem(SelectedItem);

    private EquipmentType GetItem(int index)
    {
        if (ListItems.Count == 0 || index < 0 || index >= ListItems.Count)
            return EquipmentType.None;
        else
        {
            RectTransform selected = ListItems[index];

            if (selected == daggerItem)
                return EquipmentType.Dagger;
            else if (selected == gunItem)
                return EquipmentType.Gun;
            else if (selected == boomerangItem)
                return EquipmentType.Boomerang;
            else if (selected == brainItem)
                return EquipmentType.Brain;
            else
                return EquipmentType.None;
        }
    }

    private int GetItemIndex(EquipmentType item)
    {
        switch (item)
        {
            case EquipmentType.Dagger:
                if (ListItems.Contains(daggerItem))
                    return ListItems.IndexOf(daggerItem);
                return -1;
            case EquipmentType.Brain:
                if (ListItems.Contains(brainItem))
                    return ListItems.IndexOf(brainItem);
                return -1;
            case EquipmentType.Gun:
                if (ListItems.Contains(gunItem))
                    return ListItems.IndexOf(gunItem);
                return -1;
            case EquipmentType.Boomerang:
                if (ListItems.Contains(boomerangItem))
                    return ListItems.IndexOf(boomerangItem);
                return -1;
            default:
            case EquipmentType.None: 
                return -1;
        }
    }

    public void OnItemChange(Player player)
    {
        int listLengh = ListItems.Count;

        if(player.Daggers > 0 && !daggerItem)
        {
            daggerItem = Instantiate(DaggerIcon, transform).GetComponent<RectTransform>();
            ListItems.Add(daggerItem);
        }
        if (player.Brains > 0 && !brainItem)
        {
            brainItem = Instantiate(BrainIcon, transform).GetComponent<RectTransform>();
            ListItems.Add(brainItem);
        }
        if (player.Bullets > 0 && !gunItem) 
        { 
            gunItem = Instantiate(GunIcon, transform).GetComponent<RectTransform>();
            ListItems.Add(gunItem);
        }
        if (player.Boomerangs > 0 && !boomerangItem)
        {
            boomerangItem = Instantiate(BoomerangIcon, transform).GetComponent<RectTransform>();
            ListItems.Add(boomerangItem);
        }

        if (daggerItem)
            daggerItem.GetComponentInChildren<Text>().text = $"{player.Daggers}";
        if (gunItem)
            gunItem.GetComponentInChildren<Text>().text = $"{player.Bullets}";
        if (boomerangItem)
            boomerangItem.GetComponentInChildren<Text>().text = $"{player.Boomerangs}";
        if (brainItem)
            brainItem.GetComponentInChildren<Text>().text = $"{player.Brains}";

        //Instantiate List when first entry got added
        if(listLengh == 0 && ListItems.Count > 0)
        {
            foreach (var item in ListItems)
                item.transform.localScale = Vector3.one * SmallSize;
            ListItems[SelectedItem].transform.localScale = Vector3.one * BigSize;
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }

    private IEnumerator Shift(RectTransform oldRect, RectTransform newRect)
    {
        inTransition = true;
        oldRect.transform.localScale = Vector3.one * BigSize;
        newRect.transform.localScale = Vector3.one * SmallSize;
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);

        float progress = 0;

        while(progress < TransitionDuration)
        {
            oldRect.transform.localScale = Vector3.Lerp(Vector3.one * BigSize, Vector3.one * SmallSize, progress / TransitionDuration);
            newRect.transform.localScale = Vector3.Lerp(Vector3.one * SmallSize, Vector3.one * BigSize, progress / TransitionDuration);
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime;
        }

        oldRect.transform.localScale = Vector3.one * SmallSize;
        newRect.transform.localScale = Vector3.one * BigSize;
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        inTransition = false;
    }
}
