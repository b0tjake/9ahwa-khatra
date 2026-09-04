using UnityEngine;
using System.Collections;
using QahwaKhatra.Player;
using QahwaKhatra.Core;
using QahwaKhatra.Data;
using QahwaKhatra.Cleaning;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Cafe
{
    public enum WindowsXPState
    {
        BootWelcome,    // "Welcome / Bienvenue" Windows XP sound & screen
        LoggingIn,      // Spinner / "Loading your personal settings..."
        Desktop,        // Bliss wallpaper, Recycle Bin, Internet Explorer icon, Taskbar with green Start button
        InternetExplorer// Browser window with "Derb-Express.ma" shopping catalog
    }

    public class LaptopShop : MonoBehaviour, IInteractable
    {
        [Header("State")]
        [SerializeField] private WindowsXPState _osState = WindowsXPState.BootWelcome;
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private bool _hasBoughtMop = false;
        [SerializeField] private bool _hasBoughtEspresso = false;

        private float _loginTimer = 0f;
        private bool _startMenuOpen = false;

        public bool IsOpen => _isOpen;

        public string PromptMessage
        {
            get
            {
                if (CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.SellingJunk)
                {
                    return ArabicFixer.Fix("خوي الخردة عاد خدم البيسي (Clear junk first!)");
                }
                return "USE LAPTOP (حل البيسي)";
            }
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            if (CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.SellingJunk)
            {
                Debug.Log("[Laptop] Clear and sell the junk in the garage first before using the computer!");
                return;
            }

            _isOpen = !_isOpen;
            if (_isOpen)
            {
                _osState = WindowsXPState.BootWelcome;
                _loginTimer = 0f;
                _startMenuOpen = false;
            }
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (_osState == WindowsXPState.BootWelcome)
            {
                _loginTimer += Time.deltaTime;
                if (_loginTimer > 1.2f)
                {
                    _osState = WindowsXPState.LoggingIn;
                }
            }
            else if (_osState == WindowsXPState.LoggingIn)
            {
                _loginTimer += Time.deltaTime;
                if (_loginTimer > 2.4f)
                {
                    _osState = WindowsXPState.Desktop;
                }
            }
        }

        public void BuyMopAndBucket()
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(30f))
            {
                _hasBoughtMop = true;
                
                // Spawn Cardboard Delivery Box outside the garage door in the Derb!
                SpawnDeliveryBox(DeliveryItemType.MopAndBucket, "سطل وجفاف (Mop & Bucket)", new Vector3(-1f, 0.4f, -4.5f));

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.NotifyOrderedMop();
                }
                Debug.Log("[Windows XP] Order Placed: Mop & Bucket! Delivered outside garage.");
            }
        }

        public void BuyEspressoMachine()
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(100f))
            {
                _hasBoughtEspresso = true;

                // Spawn Espresso Machine Delivery Box outside in the Derb!
                SpawnDeliveryBox(DeliveryItemType.EspressoMachine, "آلة إسبريسو (Espresso Machine)", new Vector3(1f, 0.5f, -4.5f));

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.NotifyOrderedEspresso();
                }
                Debug.Log("[Windows XP] Order Placed: Espresso Machine! Delivered outside garage.");
            }
        }

        private void SpawnDeliveryBox(DeliveryItemType itemType, string itemName, Vector3 streetPos)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = $"DeliveryBox_{itemType}";
            box.transform.position = streetPos;
            box.transform.localScale = new Vector3(0.85f, 0.75f, 0.85f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.72f, 0.53f, 0.35f); // Realistic cardboard brown
            box.GetComponent<Renderer>().material = mat;

            var delivery = box.AddComponent<DeliveryBox>();
            delivery.Initialize(itemType, itemName);
        }

        private void OnGUI()
        {
            if (!_isOpen) return;

            // TRUE FULL SCREEN WINDOWS XP DISPLAY
            float w = Screen.width;
            float h = Screen.height;
            float x = 0f;
            float y = 0f;

            // 1. WELCOME / LOGIN SCREEN
            if (_osState == WindowsXPState.BootWelcome || _osState == WindowsXPState.LoggingIn)
            {
                GUI.color = new Color(0.0f, 0.2f, 0.65f);
                GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
                GUI.color = Color.white;

                float cx = w / 2f;
                float cy = h / 2f;

                GUI.Label(new Rect(cx - 160, cy - 80, 320, 35), "Microsoft Windows XP Professional");

                if (_osState == WindowsXPState.BootWelcome)
                {
                    GUI.Label(new Rect(cx - 100, cy - 20, 200, 30), "Welcome / مرحباً بك");
                }
                else
                {
                    GUI.Label(new Rect(cx - 150, cy - 20, 300, 30), "Loading your personal settings...");
                    GUI.Label(new Rect(cx - 150, cy + 15, 300, 30), "تسجيل الدخول: Derb_Sultan_User");
                }

                if (GUI.Button(new Rect(w - 110, 20, 90, 35), "Turn Off ❌"))
                {
                    _isOpen = false;
                }
                return;
            }

            // 2. WINDOWS XP FULLSCREEN DESKTOP (BLISS GREEN/BLUE)
            GUI.color = new Color(0.25f, 0.65f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, w, h - 45), Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.68f, 0.22f);
            GUI.DrawTexture(new Rect(0, h * 0.5f, w, h * 0.5f - 45), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Desktop Icons
            if (GUI.Button(new Rect(30, 35, 90, 75), "🗑️\nRecycle Bin"))
            {
            }

            if (GUI.Button(new Rect(30, 130, 90, 80), "🌐\nInternet\nExplorer"))
            {
                _osState = WindowsXPState.InternetExplorer;
                _startMenuOpen = false;
            }

            if (GUI.Button(new Rect(30, 230, 90, 75), "💻\nMy Computer"))
            {
            }

            // 3. INTERNET EXPLORER WINDOW
            if (_osState == WindowsXPState.InternetExplorer)
            {
                float winW = Mathf.Min(800f, w - 80f);
                float winH = Mathf.Min(560f, h - 90f);
                float winX = (w - winW) / 2f;
                float winY = 25f;

                // Title Bar
                GUI.color = new Color(0.05f, 0.35f, 0.9f);
                GUI.DrawTexture(new Rect(winX, winY, winW, 32), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(winX + 12, winY + 6, 350, 24), "🌐 Internet Explorer - www.derb-express.ma");

                GUI.color = new Color(0.85f, 0.2f, 0.2f);
                if (GUI.Button(new Rect(winX + winW - 36, winY + 3, 32, 26), "X"))
                {
                    _osState = WindowsXPState.Desktop;
                }
                GUI.color = Color.white;

                // Address Bar
                GUI.Box(new Rect(winX, winY + 32, winW, 32), "");
                GUI.Label(new Rect(winX + 12, winY + 38, 65, 20), "Address:");
                GUI.TextField(new Rect(winX + 80, winY + 36, winW - 95, 24), "http://www.derb-express.ma/store/clean_and_cafe");

                // Content Body
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(winX, winY + 64, winW, winH - 64), Texture2D.whiteTexture);

                float contentY = winY + 80;

                // Website Header banner
                GUI.color = new Color(0.85f, 0.15f, 0.15f);
                GUI.DrawTexture(new Rect(winX + 15, contentY, winW - 30, 40), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(winX + 25, contentY + 10, winW - 50, 25), ArabicFixer.Fix("🛒 درب إكسبريس — سوق التجهيزات بالدار البيضاء"));

                // ITEM 1: MOP & BUCKET (سطل وجفاف)
                float item1Y = contentY + 55;
                GUI.Box(new Rect(winX + 15, item1Y, winW - 30, 95), "");
                GUI.Label(new Rect(winX + 25, item1Y + 10, winW - 190, 25), ArabicFixer.Fix("🪣 سطل وجفاف احترافي (Mop & Bucket Set)"));
                GUI.Label(new Rect(winX + 25, item1Y + 38, winW - 190, 45), ArabicFixer.Fix("ضروري باش تسيق الغبرة وتوجد الكراج.\nالثمن: 30 درهم (التوصيل لباب الكراج)"));

                if (!_hasBoughtMop)
                {
                    if (GUI.Button(new Rect(winX + winW - 165, item1Y + 25, 140, 45), ArabicFixer.Fix("طلب دابا (30 DH)")))
                    {
                        BuyMopAndBucket();
                    }
                }
                else
                {
                    GUI.Label(new Rect(winX + winW - 165, item1Y + 30, 140, 30), ArabicFixer.Fix("📦 جاري التوصيل / وصل!"));
                }

                // ITEM 2: ESPRESSO MACHINE (آلة القهوة)
                float item2Y = item1Y + 110;
                GUI.Box(new Rect(winX + 15, item2Y, winW - 30, 110), "");
                GUI.Label(new Rect(winX + 25, item2Y + 8, winW - 190, 25), ArabicFixer.Fix("☕ آلة إسبريسو كلاسيك (Classic Espresso Machine)"));
                GUI.Label(new Rect(winX + 25, item2Y + 34, winW - 190, 65), ArabicFixer.Fix("القلب النابض ديال المقهى لتحضير قهوة خاترة.\n(خاص الأرضية تكون مسيقة كاملة عاد تشريها!)\nالثمن: 100 درهم (التوصيل لباب الكراج)"));

                bool canBuyEspresso = _hasBoughtMop && CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.FloorCleanedBuyMachine;

                if (!_hasBoughtEspresso)
                {
                    if (canBuyEspresso)
                    {
                        if (GUI.Button(new Rect(winX + winW - 165, item2Y + 30, 140, 50), ArabicFixer.Fix("طلب دابا (100 DH)")))
                        {
                            BuyEspressoMachine();
                        }
                    }
                    else
                    {
                        string lockReason = !_hasBoughtMop ? "خاصك سطل وجفاف أولاً" : "سيق الغبرة كاملة أولاً!";
                        GUI.Label(new Rect(winX + winW - 165, item2Y + 35, 145, 45), ArabicFixer.Fix($"🔒 {lockReason}"));
                    }
                }
                else
                {
                    GUI.Label(new Rect(winX + winW - 165, item2Y + 35, 140, 30), ArabicFixer.Fix("📦 جاري التوصيل / وصل!"));
                }
            }

            // 4. FULL WIDTH TASKBAR (XP LUNA BLUE)
            float tbY = h - 45f;
            GUI.color = new Color(0.12f, 0.38f, 0.88f);
            GUI.DrawTexture(new Rect(0, tbY, w, 45), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Green "start" button
            GUI.color = new Color(0.24f, 0.65f, 0.18f);
            if (GUI.Button(new Rect(0, tbY, 120, 45), "🟢 start"))
            {
                _startMenuOpen = !_startMenuOpen;
            }
            GUI.color = Color.white;

            if (_osState == WindowsXPState.InternetExplorer)
            {
                if (GUI.Button(new Rect(130, tbY + 5, 200, 35), "🌐 Derb-Express.ma"))
                {
                }
            }

            // Clock & Close PC button
            GUI.Label(new Rect(w - 190, tbY + 12, 85, 25), "04:20 PM");
            if (GUI.Button(new Rect(w - 95, tbY + 5, 85, 35), "Close ❌"))
            {
                _isOpen = false;
                _startMenuOpen = false;
            }

            // 5. XP START MENU POPUP
            if (_startMenuOpen)
            {
                float smW = 220f;
                float smH = 180f;
                float smX = 0f;
                float smY = tbY - smH;

                GUI.Box(new Rect(smX, smY, smW, smH), "Windows XP");

                if (GUI.Button(new Rect(smX + 10, smY + 35, smW - 20, 35), "🌐 Internet Explorer"))
                {
                    _osState = WindowsXPState.InternetExplorer;
                    _startMenuOpen = false;
                }

                if (GUI.Button(new Rect(smX + 10, smY + 75, smW - 20, 35), "💻 My Computer"))
                {
                    _startMenuOpen = false;
                }

                if (GUI.Button(new Rect(smX + 10, smY + 120, smW - 20, 45), "🔴 Log Off / Exit PC"))
                {
                    _isOpen = false;
                    _startMenuOpen = false;
                }
            }
        }
    }
}
