using UnityEngine;
using System.Collections.Generic;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Customer
{
    public class CustomerQueue : Singleton<CustomerQueue>
    {
        [Header("Counter Queue Waypoints")]
        [SerializeField] private List<Transform> _queuePoints = new List<Transform>();

        private readonly List<CustomerAI> _waitingCustomers = new List<CustomerAI>();

        public int MaxCapacity => _queuePoints.Count > 0 ? _queuePoints.Count : 4;
        public bool HasRoom => _waitingCustomers.Count < MaxCapacity;

        protected override void Awake()
        {
            base.Awake();
            if (_queuePoints.Count == 0)
            {
                // Auto generate queue points along the front of the counter
                for (int i = 0; i < 4; i++)
                {
                    var qp = new GameObject($"QueuePoint_{i}");
                    qp.transform.SetParent(transform);
                    // Counter is at (-2, 0.6, 4). Queue stands in front of counter around z = 2.4
                    qp.transform.position = new Vector3(-2f + (i * 1.3f), 0.1f, 2.4f);
                    _queuePoints.Add(qp.transform);
                }
            }
        }

        public Vector3 RegisterCustomer(CustomerAI customer)
        {
            if (!_waitingCustomers.Contains(customer))
            {
                _waitingCustomers.Add(customer);
            }

            int index = _waitingCustomers.IndexOf(customer);
            return GetWaypointPosition(index);
        }

        public void UnregisterCustomer(CustomerAI customer)
        {
            if (_waitingCustomers.Remove(customer))
            {
                // Advance remaining customers in queue
                for (int i = 0; i < _waitingCustomers.Count; i++)
                {
                    _waitingCustomers[i].MoveToQueueSpot(GetWaypointPosition(i), i == 0);
                }
            }
        }

        public Vector3 GetWaypointPosition(int index)
        {
            if (index >= 0 && index < _queuePoints.Count)
            {
                return _queuePoints[index].position;
            }
            return new Vector3(-2f, 0.1f, 2.4f);
        }

        public bool IsFirstInQueue(CustomerAI customer)
        {
            return _waitingCustomers.Count > 0 && _waitingCustomers[0] == customer;
        }
    }
}
