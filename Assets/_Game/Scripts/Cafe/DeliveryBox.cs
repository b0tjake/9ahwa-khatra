using UnityEngine;
using System;
using QahwaKhatra.Player;
using QahwaKhatra.Cleaning;
using QahwaKhatra.CoffeeCrafting;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cafe
{
    public enum DeliveryItemType
    {
        MopAndBucket,
        EspressoMachine
    }

    public class DeliveryBox : MonoBehaviour, IInteractable
    {
        [Header("Delivery Data")]
        [SerializeField] private DeliveryItemType _itemType;
        [SerializeField] private string _itemName = "Delivery Package";

        public DeliveryItemType ItemType => _itemType;
        public string PromptMessage => ArabicFixer.Fix($"حل كرتونة: {_itemName} (Open Box)");

        public void Initialize(DeliveryItemType itemType, string itemName)
        {
            _itemType = itemType;
            _itemName = itemName;
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            Debug.Log($"[DeliveryBox] Unboxed {_itemName}!");

            if (_itemType == DeliveryItemType.MopAndBucket)
            {
                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.AcquireMop();
                }

                // Spawn a visual mop & bucket next to the garage entrance!
                var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bucket.name = "Mop_Bucket_Prop";
                bucket.transform.position = new Vector3(-3.8f, 0.25f, 0f);
                bucket.transform.localScale = new Vector3(0.45f, 0.25f, 0.45f);
                var bMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                bMat.color = new Color(0.15f, 0.5f, 0.85f); // Classic blue Moroccan cleaning bucket
                bucket.GetComponent<Renderer>().material = bMat;

                // Mop stick
                var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = "Mop_Stick";
                stick.transform.position = new Vector3(-3.8f, 0.8f, 0f);
                stick.transform.localScale = new Vector3(0.06f, 0.65f, 0.06f);
                stick.transform.rotation = Quaternion.Euler(15f, 0f, 10f);
                var sMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                sMat.color = new Color(0.85f, 0.2f, 0.2f); // Red mop handle
                stick.GetComponent<Renderer>().material = sMat;
            }
            else if (_itemType == DeliveryItemType.EspressoMachine)
            {
                var machinePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/models/coffee machine.fbx");
                GameObject machine;
                if (machinePrefab != null)
                {
                    machine = Instantiate(machinePrefab);
                    machine.transform.localScale = Vector3.one * 1.6f;
                    machine.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else
                {
                    machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    machine.transform.localScale = new Vector3(0.9f, 0.8f, 0.7f);
                }

                machine.name = "Espresso_Machine";
                machine.transform.position = new Vector3(-2f, 1.25f, 4f);

                var col = machine.GetComponent<Collider>();
                if (col == null)
                {
                    var bCol = machine.AddComponent<BoxCollider>();
                    bCol.size = Vector3.one * 0.6f;
                    bCol.center = new Vector3(0f, 0.25f, 0f);
                }

                machine.AddComponent<EspressoStation>();

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.CompleteEspressoPurchase();
                }
            }

            // Destroy the cardboard box
            Destroy(gameObject);
        }
    }
}
