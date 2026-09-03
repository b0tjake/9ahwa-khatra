using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace QahwaKhatra.Player
{
    public interface IInteractable
    {
        string PromptMessage { get; }
        void OnInteract(PlayerInteraction interactor);
    }

    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float _interactionRadius = 1.6f;
        [SerializeField] private bool _autoCollectJunk = true;

        private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();
        public IInteractable CurrentInteractable => _nearbyInteractables.Count > 0 ? _nearbyInteractables[0] : null;

        public event Action<IInteractable> OnInteractableAvailable;
        public event Action OnInteractableLost;

        private PlayerInventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            // Keyboard interaction support (E or Space to interact)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    TriggerCurrentInteraction();
                }

                // Press Q or Backspace to manually drop an item anywhere on the ground!
                if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame)
                {
                    DropItemOnGround();
                }
            }

            CheckProximityInteractables();
        }

        public void DropItemOnGround()
        {
            if (_inventory != null && !_inventory.IsEmpty)
            {
                Transform item = _inventory.RemoveTopItem();
                if (item != null)
                {
                    // Place it in front of the player on the floor
                    item.position = transform.position + transform.forward * 1.2f + Vector3.up * 0.25f;
                    item.rotation = Quaternion.identity;
                    if (item.TryGetComponent<Collider>(out var col))
                    {
                        col.enabled = true;
                    }
                }
            }
        }

        private void CheckProximityInteractables()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _interactionRadius);
            
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    if (_autoCollectJunk)
                    {
                        interactable.OnInteract(this);
                        return;
                    }

                    if (!_nearbyInteractables.Contains(interactable))
                    {
                        _nearbyInteractables.Add(interactable);
                        OnInteractableAvailable?.Invoke(interactable);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                if (_autoCollectJunk)
                {
                    interactable.OnInteract(this);
                    return;
                }

                if (!_nearbyInteractables.Contains(interactable))
                {
                    _nearbyInteractables.Add(interactable);
                    OnInteractableAvailable?.Invoke(interactable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                if (_nearbyInteractables.Remove(interactable))
                {
                    if (_nearbyInteractables.Count > 0)
                    {
                        OnInteractableAvailable?.Invoke(_nearbyInteractables[0]);
                    }
                    else
                    {
                        OnInteractableLost?.Invoke();
                    }
                }
            }
        }

        public void TriggerCurrentInteraction()
        {
            if (CurrentInteractable != null)
            {
                CurrentInteractable.OnInteract(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
