using UnityEngine;
using System.Collections;
using QahwaKhatra.Player;
using QahwaKhatra.Core;

namespace QahwaKhatra.Cleaning
{
    public class SellZone : MonoBehaviour
    {
        [Header("Sell Settings")]
        [SerializeField] private float _sellInterval = 0.2f;
        [SerializeField] private float _detectionRadius = 2.5f;

        private Coroutine _sellCoroutine;

        private void Update()
        {
            // Continuous radius check so even without rigidbodies, walking in drops and sells!
            Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius);
            PlayerInventory inventory = null;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].TryGetComponent<PlayerInventory>(out var inv))
                {
                    inventory = inv;
                    break;
                }
            }

            if (inventory != null && !inventory.IsEmpty && _sellCoroutine == null)
            {
                _sellCoroutine = StartCoroutine(SellItemsRoutine(inventory));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerInventory>(out var inventory))
            {
                if (_sellCoroutine != null) StopCoroutine(_sellCoroutine);
                _sellCoroutine = StartCoroutine(SellItemsRoutine(inventory));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerInventory>(out var inventory))
            {
                if (_sellCoroutine != null)
                {
                    StopCoroutine(_sellCoroutine);
                    _sellCoroutine = null;
                }
            }
        }

        private IEnumerator SellItemsRoutine(PlayerInventory inventory)
        {
            while (!inventory.IsEmpty)
            {
                Transform item = inventory.RemoveTopItem();
                if (item != null)
                {
                    float earnedAmount = 15f;
                    string itemName = "Old Junk";

                    if (item.TryGetComponent<JunkItem>(out var junk) && junk.ItemData != null)
                    {
                        earnedAmount = junk.ItemData.sellPrice;
                        itemName = junk.ItemData.itemName;
                    }

                    // Animate item flying into sell zone center
                    Vector3 startPos = item.position;
                    Vector3 targetPos = transform.position + Vector3.up * 0.5f;
                    float elapsed = 0f;
                    float duration = 0.15f;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        if (item != null)
                        {
                            item.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                        }
                        yield return null;
                    }

                    if (item != null)
                    {
                        Destroy(item.gameObject);
                    }

                    // Add Dirhams
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.AddDirhams(earnedAmount, $"Sold {itemName}");
                    }
                }

                yield return new WaitForSeconds(_sellInterval);
            }

            _sellCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }
}
