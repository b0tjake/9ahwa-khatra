using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using QahwaKhatra.Cleaning;
using QahwaKhatra.Utils;

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
        [SerializeField] private float _interactionRadius = 1.8f;
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
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    TriggerCurrentInteraction();
                }

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
            
            // Clean up lost interactables
            for (int j = _nearbyInteractables.Count - 1; j >= 0; j--)
            {
                var item = _nearbyInteractables[j] as MonoBehaviour;
                if (item == null || Vector3.Distance(transform.position, item.transform.position) > _interactionRadius + 0.3f)
                {
                    _nearbyInteractables.RemoveAt(j);
                    if (_nearbyInteractables.Count == 0) OnInteractableLost?.Invoke();
                }
            }

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    // Only auto-collect items that are actual JunkItem!
                    if (_autoCollectJunk && hits[i].GetComponent<JunkItem>() != null)
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

        public void TriggerCurrentInteraction()
        {
            if (CurrentInteractable != null)
            {
                CurrentInteractable.OnInteract(this);
            }
        }

        private void OnGUI()
        {
            // If near an interactable (like the Laptop), render an on-screen interact button!
            if (CurrentInteractable != null)
            {
                string msg = CurrentInteractable.PromptMessage;
                if (string.IsNullOrEmpty(msg)) return;

                float btnW = 260f;
                float btnH = 65f;
                float btnX = (Screen.width - btnW) / 2f;
                float btnY = Screen.height - 110f;

                // Glowing green/gold prompt button
                GUI.color = new Color(0.2f, 0.85f, 0.3f);
                if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), $"💻 {msg}\n(Click or Press E / Space)"))
                {
                    TriggerCurrentInteraction();
                }
                GUI.color = Color.white;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
