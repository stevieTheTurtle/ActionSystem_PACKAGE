using UnityEngine;

namespace HumanoidInteraction
{
    /// <summary>
    /// Base interface for any object that can be interacted with
    /// </summary>
    public interface IInteractable
    {
        Transform InteractionPoint { get; }
        string Desc { get; }
        bool CanInteract { get; }
    }

    /// <summary>
    /// Interface for objects that can be picked up and carried
    /// </summary>
    public interface IPickable : IInteractable
    {
        public void OnPickup();
        public void SetBeingCarried(bool flag);
        bool IsBeingCarried { get; }
    }
} 