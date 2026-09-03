using UnityEngine;
using QahwaKhatra.Player;
using QahwaKhatra.Core;
using QahwaKhatra.Data;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cafe
{
    public class LaptopShop : MonoBehaviour, IInteractable
    {
        [Header("Machine Catalog")]
        [SerializeField] private MachineSO _basicEspressoMachine;
        [SerializeField] private GameObject _espressoMachinePrefab;
        [SerializeField] private Transform _counterSpawnPoint;

        [Header("State")]
        [SerializeField] private bool _hasBoughtEspresso = false;
        private bool _isShopOpen = false;

        public string PromptMessage => _hasBoughtEspresso ? "Laptop (Store)" : ArabicFixer.Fix("Laptop: Buy Espresso Machine (شري آلة قهوة)");

        public void OnInteract(PlayerInteraction interactor)
        {
            _isShopOpen = !_isShopOpen;
        }

        public void BuyBasicEspresso()
        {
            if (_hasBoughtEspresso) return;

            float price = _basicEspressoMachine != null ? _basicEspressoMachine.price : 100f;

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(price))
            {
                _hasBoughtEspresso = true;
                _isShopOpen = false;
                SpawnEspressoMachine();
                Debug.Log("[LaptopShop] Purchased Basic Espresso Machine for 100 DH!");
            }
            else
            {
                Debug.LogWarning("[LaptopShop] Not enough DH to buy the machine!");
            }
        }

        private void SpawnEspressoMachine()
        {
            Vector3 spawnPos = _counterSpawnPoint != null ? _counterSpawnPoint.position : new Vector3(-2f, 1.4f, 4f);

            var machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machine.name = "Espresso_Machine";
            machine.transform.position = spawnPos;
            machine.transform.localScale = new Vector3(0.9f, 0.8f, 0.7f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.75f, 0.15f, 0.15f);
            machine.GetComponent<Renderer>().material = mat;

            machine.AddComponent<QahwaKhatra.CoffeeCrafting.EspressoStation>();
        }

        private void OnGUI()
        {
            if (!_isShopOpen) return;

            float w = 360f;
            float h = 230f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), ArabicFixer.Fix("💻 سوق الأجهزة (Cafe Equipment)"));

            if (!_hasBoughtEspresso)
            {
                GUI.Label(new Rect(x + 20, y + 40, w - 40, 50), ArabicFixer.Fix("آلة إسبريسو كلاسيك (Basic Espresso)\nثمن: 100 درهم (Price: 100 DH)\nتمكنك من تحضير قهوة خاترة!"));

                if (GUI.Button(new Rect(x + 20, y + 105, w - 40, 45), ArabicFixer.Fix("🛒 شري دابا (Buy for 100 DH)")))
                {
                    BuyBasicEspresso();
                }
            }
            else
            {
                GUI.Label(new Rect(x + 20, y + 50, w - 40, 50), ArabicFixer.Fix("✅ شريتي آلة القهوة!\nسير للكونتوار باش تبدا تصاوب القهوة."));
            }

            if (GUI.Button(new Rect(x + 20, y + 165, w - 40, 35), ArabicFixer.Fix("سد (Close)")))
            {
                _isShopOpen = false;
            }
        }
    }
}
