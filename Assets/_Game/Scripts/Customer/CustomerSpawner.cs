using UnityEngine;
using System.Collections.Generic;
using QahwaKhatra.Data;
using QahwaKhatra.Core;

namespace QahwaKhatra.Customer
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Vector3 _spawnPoint = new Vector3(0f, 0.5f, -14f); // Derb entrance
        [SerializeField] private float _spawnInterval = 8f;
        [SerializeField] private List<CustomerTypeSO> _customerTypes = new List<CustomerTypeSO>();

        private float _timer = 0f;

        private void Update()
        {
            // Only spawn if cafe is open or in cafe mode
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.TutorialCleaning)
            {
                // Can start spawning once Day 1 objective is achieved
            }

            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                TrySpawnCustomer();
            }
        }

        public void TrySpawnCustomer()
        {
            if (CustomerQueue.Instance == null || !CustomerQueue.Instance.HasRoom)
            {
                return;
            }

            // Create customer capsule
            var customerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            customerGO.name = "Customer_NPC";
            customerGO.transform.position = _spawnPoint;
            customerGO.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            var col = customerGO.GetComponent<CapsuleCollider>();
            col.isTrigger = true;

            var agent = customerGO.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 3.2f;
            agent.radius = 0.4f;

            var ai = customerGO.AddComponent<CustomerAI>();

            // Pick random customer profile
            CustomerTypeSO profile = null;
            if (_customerTypes.Count > 0)
            {
                profile = _customerTypes[Random.Range(0, _customerTypes.Count)];
            }

            Vector3 queueSpot = CustomerQueue.Instance.RegisterCustomer(ai);
            ai.Initialize(profile, queueSpot);
        }
    }
}
