using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class Activator : MonoBehaviour
{
    public bool Active = false;
    public bool UpdateGraph = true;

    [SerializeField]
    private bool ShiftCamera = false;
    private Cinemachine.CinemachineVirtualCamera VirtualCamera;
    private Player Player;

    [SerializeField]
    private Vector3 ShiftLocation = Vector3.zero;

    public abstract bool ManualControllable { get; }
    public abstract bool Controllable { get; }

    public List<Activatable> Activatables;

    protected Animator Animator;

    private float noCinematics = .1f;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        VirtualCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        Player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (noCinematics > 0)
            noCinematics -= Time.deltaTime;
    }

    public void Toggle()
    {
        if (noCinematics > 0 || !ShiftCamera)
            ToggleNow();
        else
            StartCoroutine(ShiftCameraThenToggle());
    }

    private void ToggleNow()
    {
        if (Controllable)
        {
            Active = !Active;
            Activatables.ForEach(x => x.Active = !x.Active);

            if (Animator && Animator.runtimeAnimatorController)
                Animator.SetBool("Active", Active);

            OnToggle(Active);
            if(UpdateGraph)
                GameManager.UpdatePathFinding();
        }
    }

    private IEnumerator ShiftCameraThenToggle()
    {
        noCinematics = 3f;
        FindObjectsOfType<Entity>().ToList().ForEach(x => x.Freeze(1.1f));

        var target = new GameObject("Camera Target");
        target.transform.position = ShiftLocation;

        VirtualCamera.Follow = target.transform;
        yield return new WaitForSeconds(.5f);
        ToggleNow();
        yield return new WaitForSeconds(.5f);
        VirtualCamera.Follow = Player.transform;

        Destroy(target);
    }

    protected abstract void OnToggle(bool newVal);
}
