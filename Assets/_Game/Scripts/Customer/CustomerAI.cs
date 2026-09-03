using UnityEngine;
using UnityEngine.AI;
using QahwaKhatra.Player;
using QahwaKhatra.Data;
using QahwaKhatra.Core;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Customer
{
    public enum CustomerState
    {
        WalkingIn,
        WaitingInQueue,
        WaitingForDrink,
        ServedAndLeaving,
        LeavingAngry,
        LeavingDisappointed
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerAI : MonoBehaviour, IInteractable
    {
        [Header("Profile & Order")]
        [SerializeField] private CustomerTypeSO _customerType;
        [SerializeField] private CustomerState _currentState = CustomerState.WalkingIn;

        [Header("Patience Timer")]
        [SerializeField] private float _currentPatience = 45f;
        [SerializeField] private float _maxPatience = 45f;

        private NavMeshAgent _agent;
        private Vector3 _exitPoint;
        private bool _isOrderTaken = false;

        public CustomerState CurrentState => _currentState;
        public CustomerTypeSO CustomerType => _customerType;
        public float PatiencePercent => Mathf.Clamp01(_currentPatience / _maxPatience);
        public string PromptMessage => _isOrderTaken ? ArabicFixer.Fix("عطيه القهوة (Serve Drink)") : ArabicFixer.Fix("خُد الطلب (Take Order)");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _exitPoint = new Vector3(0f, 0.5f, -14f); // Down the street
        }

        public void Initialize(CustomerTypeSO type, Vector3 targetQueueSpot)
        {
            _customerType = type;
            _maxPatience = type != null ? type.basePatience : 45f;
            _currentPatience = _maxPatience;

            if (TryGetComponent<Renderer>(out var rend) && type != null)
            {
                rend.material.color = type.clothingColor;
            }

            _currentState = CustomerState.WalkingIn;
            _agent.SetDestination(targetQueueSpot);
        }

        private void Update()
        {
            // Patience decay while waiting in queue or waiting for drink
            if (_currentState == CustomerState.WaitingInQueue || _currentState == CustomerState.WaitingForDrink)
            {
                _currentPatience -= Time.deltaTime;

                if (_currentPatience <= 0f)
                {
                    LeaveAngry();
                }
            }

            // Check arrival at queue point
            if (_currentState == CustomerState.WalkingIn && !_agent.pathPending && _agent.remainingDistance <= 0.3f)
            {
                _currentState = CustomerState.WaitingInQueue;
            }

            // Check arrival at exit point to despawn
            if ((_currentState == CustomerState.ServedAndLeaving || _currentState == CustomerState.LeavingAngry || _currentState == CustomerState.LeavingDisappointed)
                && !_agent.pathPending && _agent.remainingDistance <= 0.6f)
            {
                Destroy(gameObject);
            }
        }

        public void MoveToQueueSpot(Vector3 spot, bool isFront)
        {
            if (_currentState == CustomerState.WaitingInQueue || _currentState == CustomerState.WalkingIn)
            {
                _agent.SetDestination(spot);
            }
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            // Check if player is interacting with first in line
            if (CustomerQueue.Instance != null && !CustomerQueue.Instance.IsFirstInQueue(this))
            {
                return;
            }

            // Step 1: Take Order
            if (!_isOrderTaken && _currentState == CustomerState.WaitingInQueue)
            {
                TakeOrder();
            }
            // Step 2: Serve Drink
            else if (_isOrderTaken && _currentState == CustomerState.WaitingForDrink)
            {
                ServeDrink();
            }
        }

        private void TakeOrder()
        {
            _isOrderTaken = true;
            _currentState = CustomerState.WaitingForDrink;
            _currentPatience = Mathf.Min(_currentPatience + 25f, _maxPatience); // Bonus patience after taking order
            Debug.Log($"[CustomerAI] Took order: {_customerType?.preferredDrink?.drinkName ?? "Espresso"}!");
            EventBus.TriggerCustomerOrdered(_customerType?.preferredDrink?.drinkName ?? "Espresso");
        }

        private void ServeDrink()
        {
            _currentState = CustomerState.ServedAndLeaving;
            CustomerQueue.Instance?.UnregisterCustomer(this);

            float payout = 12f;
            if (_customerType != null && _customerType.preferredDrink != null)
            {
                payout = _customerType.preferredDrink.basePrice * _customerType.tipMultiplier;
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddDirhams(payout, "Customer Served (زبون خلص)");
            }

            EventBus.TriggerCustomerServed(95f, (int)(payout * 0.25f));
            Debug.Log($"[CustomerAI] Customer served! Paid {payout} DH.");

            _agent.SetDestination(_exitPoint);
        }

        private void LeaveAngry()
        {
            _currentState = CustomerState.LeavingAngry;
            CustomerQueue.Instance?.UnregisterCustomer(this);
            EventBus.TriggerCustomerLeftAngry();
            Debug.Log("[CustomerAI] Customer waited too long and left angry! (الزبون طلع غضبان)");
            _agent.SetDestination(_exitPoint);
        }

        private void OnGUI()
        {
            // Display thought bubble above customer if waiting
            if (_currentState == CustomerState.WaitingInQueue || _currentState == CustomerState.WaitingForDrink)
            {
                Camera cam = Camera.main;
                if (cam == null) return;

                Vector3 screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
                if (screenPos.z > 0)
                {
                    float bw = 170f;
                    float bh = 55f;
                    float bx = screenPos.x - bw / 2f;
                    float by = Screen.height - screenPos.y - bh;

                    GUI.Box(new Rect(bx, by, bw, bh), "");

                    string orderText = _currentState == CustomerState.WaitingInQueue
                        ? ArabicFixer.Fix("باغي قهوة خاترة")
                        : ArabicFixer.Fix("كنتسنى القهوة...");

                    GUI.Label(new Rect(bx + 5, by + 4, bw - 10, 24), orderText);

                    // Patience bar
                    float barW = (bw - 20) * PatiencePercent;
                    GUI.color = PatiencePercent > 0.4f ? Color.green : Color.red;
                    GUI.DrawTexture(new Rect(bx + 10, by + 32, barW, 10), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
        }
    }
}
