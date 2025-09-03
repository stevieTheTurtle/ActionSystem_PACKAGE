using HumanoidInteraction;using UnityEngine;

public class Pickable : Interactable, IPickable
{
    public bool IsBeingCarried { get; private set; }

    public void OnPickup()
    {
        Debug.Log($"{this.name} has been picked up");
        
        IsBeingCarried = true;
    }

    public void SetBeingCarried(bool flag)
    {
        IsBeingCarried = false;
    }
}
