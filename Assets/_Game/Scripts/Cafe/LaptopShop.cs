using UnityEngine;
using System;
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
        LoginTypingPassword, // Auto-typing password ••••••••
        LoginSubmitting,     // Button click + "Loading your personal settings..."
        Desktop,             // Bliss wallpaper, desktop icons, Luna taskbar with green start button
        InternetExplorer     // Genuine IE6 browser with "Derb Express" professional e-commerce shop
    }

    public class LaptopShop : MonoBehaviour, IInteractable
    {
        [Header("State")]
        [SerializeField] private WindowsXPState _osState = WindowsXPState.LoginTypingPassword;
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private bool _hasBoughtMop = false;
        [SerializeField] private bool _hasBoughtEspresso = false;

        // Auto-login typing variables
        private string _typedPassword = "";
        private float _passwordTimer = 0f;
        private int _targetPasswordLength = 8;
        private float _loginSubmittingTimer = 0f;
        private bool _startMenuOpen = false;

        // Delivery Toast state
        private string _toastTitle = "";
        private string _toastDesc = "";
        private float _toastTimer = 0f;

        // Audio Source & Procedural Clips
        private AudioSource _audioSource;
        private AudioClip _clickClip;
        private AudioClip _startupClip;
        private AudioClip _cashClip;

        // Procedural Textures
        private Texture2D _blissTex;
        private Texture2D _taskbarTex;
        private Texture2D _startBtnTex;
        private Texture2D _titleBarTex;
        private Texture2D _cardBgTex;
        private Texture2D _loginBgTex;

        public bool IsOpen => _isOpen;

        public string PromptMessage
        {
            get
            {
                if (CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.SellingJunk)
                {
                    return ArabicFixer.Fix("خوي الخردة عاد خدم البيسي (Clear junk first!)");
                }
                return ArabicFixer.Fix("حل البيسي (USE LAPTOP)");
            }
        }

        private void Awake()
        {
            SetupAudio();
            GenerateTextures();
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
                // Reset to Auto-Login Sequence
                _osState = WindowsXPState.LoginTypingPassword;
                _typedPassword = "";
                _passwordTimer = 0f;
                _loginSubmittingTimer = 0f;
                _startMenuOpen = false;
            }
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Allow quick exit with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isOpen = false;
                _startMenuOpen = false;
                return;
            }

            // 1. AUTO-LOGIN SEQUENCE
            if (_osState == WindowsXPState.LoginTypingPassword)
            {
                _passwordTimer += Time.deltaTime;
                if (_passwordTimer >= 0.14f && _typedPassword.Length < _targetPasswordLength)
                {
                    _passwordTimer = 0f;
                    _typedPassword += "•";
                    PlaySound(_clickClip);
                }
                else if (_typedPassword.Length >= _targetPasswordLength)
                {
                    _passwordTimer += Time.deltaTime;
                    if (_passwordTimer >= 0.4f)
                    {
                        // Submit password automatically!
                        _osState = WindowsXPState.LoginSubmitting;
                        _loginSubmittingTimer = 0f;
                        PlaySound(_clickClip);
                        PlaySound(_startupClip);
                    }
                }
            }
            else if (_osState == WindowsXPState.LoginSubmitting)
            {
                _loginSubmittingTimer += Time.deltaTime;
                if (_loginSubmittingTimer >= 1.4f)
                {
                    // Transition to Desktop!
                    _osState = WindowsXPState.Desktop;
                }
            }

            // Toast notification timer
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
            }
        }

        public void BuyMopAndBucket()
        {
            if (_hasBoughtMop) return;

            float currentCash = CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentDirhams : 100f;
            if (currentCash < 30f)
            {
                ShowToast("الرصيد غير كافٍ!", "خاصك 30 درهم باش تشري السطل والجفاف.");
                return;
            }

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(30f))
            {
                _hasBoughtMop = true;
                PlaySound(_cashClip);

                // Spawn Cardboard Delivery Box outside the garage door in the Derb!
                SpawnDeliveryBox(DeliveryItemType.MopAndBucket, "سطل وجفاف (Mop & Bucket)", new Vector3(-1f, 0.4f, -4.5f));

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.NotifyOrderedMop();
                }

                ShowToast("تم تأكيد الطلب (30 DH) 📦", "خرج الصندوق الكرتوني لباب الكراج! اخرج لفتحه وبدء التنظيف.");
                Debug.Log("[Windows XP] Order Placed: Mop & Bucket! Delivered outside garage.");
            }
        }

        public void BuyEspressoMachine()
        {
            if (_hasBoughtEspresso) return;

            bool canBuy = _hasBoughtMop && CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.FloorCleanedBuyMachine;
            if (!canBuy)
            {
                ShowToast("الأرضية مازال ممسيقاش!", "خاصك تسيق الغبرة كاملة عاد تقدر تشري الماكينة.");
                return;
            }

            float currentCash = CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentDirhams : 100f;
            if (currentCash < 100f)
            {
                ShowToast("الرصيد غير كافٍ!", "خاصك 100 درهم لشراء آلة الإسبريسو.");
                return;
            }

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(100f))
            {
                _hasBoughtEspresso = true;
                PlaySound(_cashClip);

                // Spawn Espresso Machine Delivery Box outside in the Derb!
                SpawnDeliveryBox(DeliveryItemType.EspressoMachine, "آلة إسبريسو (Espresso Machine)", new Vector3(1f, 0.5f, -4.5f));

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.NotifyOrderedEspresso();
                }

                ShowToast("مبروك! تم شراء ماكينة القهوة ☕", "تم إرسال الصندوق إلى باب الكراج! افتحه لتثبيت الماكينة فوق الكونتوار.");
                Debug.Log("[Windows XP] Order Placed: Espresso Machine! Delivered outside garage.");
            }
        }

        private void SpawnDeliveryBox(DeliveryItemType itemType, string itemName, Vector3 streetPos)
        {
            var boxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/models/box.fbx");
            GameObject box;
            if (boxPrefab != null)
            {
                box = Instantiate(boxPrefab);
                box.transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.transform.localScale = new Vector3(0.85f, 0.75f, 0.85f);
            }

            box.name = $"DeliveryBox_{itemType}";
            box.transform.position = streetPos;

            var col = box.GetComponent<Collider>();
            if (col == null)
            {
                var bCol = box.AddComponent<BoxCollider>();
                bCol.size = Vector3.one * 0.7f;
                bCol.center = new Vector3(0f, 0.35f, 0f);
            }

            var delivery = box.AddComponent<DeliveryBox>();
            delivery.Initialize(itemType, itemName);
        }

        private void ShowToast(string title, string desc)
        {
            _toastTitle = ArabicFixer.Fix(title);
            _toastDesc = ArabicFixer.Fix(desc);
            _toastTimer = 4.5f;
        }

        private void OnGUI()
        {
            if (!_isOpen) return;

            float w = Screen.width;
            float h = Screen.height;

            // Ensure textures exist
            if (_blissTex == null) GenerateTextures();

            // --------------------------------------------------------
            // 1. AUTO-LOGIN SCREEN
            // --------------------------------------------------------
            if (_osState == WindowsXPState.LoginTypingPassword || _osState == WindowsXPState.LoginSubmitting)
            {
                DrawLoginScreen(w, h);
                return;
            }

            // --------------------------------------------------------
            // 2. WINDOWS XP DESKTOP & BLISS WALLPAPER
            // --------------------------------------------------------
            GUI.DrawTexture(new Rect(0, 0, w, h - 36), _blissTex);

            // Desktop Icons
            DrawDesktopIcons();

            // --------------------------------------------------------
            // 3. INTERNET EXPLORER 6 WINDOW (PRO SHOP)
            // --------------------------------------------------------
            if (_osState == WindowsXPState.InternetExplorer)
            {
                DrawInternetExplorerWindow(w, h);
            }

            // --------------------------------------------------------
            // 4. LUNA BLUE TASKBAR & START BUTTON
            // --------------------------------------------------------
            DrawTaskbar(w, h);

            // --------------------------------------------------------
            // 5. START MENU POPUP
            // --------------------------------------------------------
            if (_startMenuOpen)
            {
                DrawStartMenu(w, h);
            }

            // --------------------------------------------------------
            // 6. LIVE DELIVERY TOAST NOTIFICATION
            // --------------------------------------------------------
            if (_toastTimer > 0f)
            {
                DrawToastNotification(w, h);
            }
        }

        // ========================================================
        // DRAW HELPERS
        // ========================================================

        private void DrawLoginScreen(float w, float h)
        {
            // Background
            GUI.DrawTexture(new Rect(0, 0, w, h), _loginBgTex);

            // Top Header Bar
            GUI.color = new Color(0.0f, 0.08f, 0.23f);
            GUI.DrawTexture(new Rect(0, 0, w, 75), Texture2D.whiteTexture);
            GUI.color = new Color(0.92f, 0.56f, 0.0f);
            GUI.DrawTexture(new Rect(0, 75, w, 3), Texture2D.whiteTexture);

            // Header Title
            GUI.color = Color.white;
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(40, 22, 500, 35), "Microsoft Windows XP Professional", headerStyle);

            // Bottom Footer Bar
            GUI.color = new Color(0.0f, 0.08f, 0.23f);
            GUI.DrawTexture(new Rect(0, h - 60, w, 60), Texture2D.whiteTexture);
            GUI.color = new Color(0.92f, 0.56f, 0.0f);
            GUI.DrawTexture(new Rect(0, h - 63, w, 3), Texture2D.whiteTexture);

            GUI.color = new Color(0.68f, 0.78f, 1.0f);
            GUI.Label(new Rect(40, h - 42, 600, 25), "Derb Sultan Station • Café Khatra Interactive Terminal");
            GUI.color = Color.white;

            if (GUI.Button(new Rect(w - 140, h - 48, 110, 34), "Turn Off ✕"))
            {
                _isOpen = false;
            }

            // Center Content
            float cx = w * 0.5f;
            float cy = h * 0.5f;

            // Left Instruction
            var leftInstStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Normal
            };
            leftInstStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cx - 480, cy - 60, 420, 80), "To begin, click your user name\n" + ArabicFixer.Fix("تسجيل الدخول التلقائي للمقهى..."), leftInstStyle);

            // Vertical Glow Divider
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            GUI.DrawTexture(new Rect(cx - 30, cy - 110, 2, 220), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // User Card Box
            float cardX = cx + 20;
            float cardY = cy - 75;
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(new Rect(cardX, cardY, 440, 140), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Avatar Frame
            GUI.color = new Color(0.1f, 0.45f, 0.8f);
            GUI.DrawTexture(new Rect(cardX + 16, cardY + 20, 72, 72), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.8f, 0.1f);
            GUI.Box(new Rect(cardX + 16, cardY + 20, 72, 72), "");
            GUI.color = Color.white;

            var avatarIconStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(cardX + 16, cardY + 20, 72, 72), "☕", avatarIconStyle);

            // Username Label
            var userStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            userStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 104, cardY + 16, 320, 28), ArabicFixer.Fix("Derb Sultan Barista (قهوجي الدرب)"), userStyle);

            // Subtitle
            var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            subStyle.normal.textColor = new Color(0.7f, 0.82f, 1f);
            GUI.Label(new Rect(cardX + 104, cardY + 44, 300, 20), ArabicFixer.Fix("Café Khatra • Derb Sultan, Casablanca"), subStyle);

            // Password Field & Auto-typing Display
            GUI.Label(new Rect(cardX + 104, cardY + 68, 80, 22), "Password:");
            GUI.color = Color.white;

            var passBoxStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            GUI.TextField(new Rect(cardX + 175, cardY + 66, 175, 26), _typedPassword, passBoxStyle);

            // Green submit button
            GUI.color = new Color(0.2f, 0.78f, 0.3f);
            if (GUI.Button(new Rect(cardX + 358, cardY + 65, 36, 28), "➔"))
            {
                _osState = WindowsXPState.LoginSubmitting;
                _loginSubmittingTimer = 0f;
                PlaySound(_startupClip);
            }
            GUI.color = Color.white;

            // Status message
            var statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic
            };
            statusStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

            if (_osState == WindowsXPState.LoginTypingPassword)
            {
                GUI.Label(new Rect(cardX + 104, cardY + 98, 320, 22), "Auto-typing credentials (" + _typedPassword.Length + "/8)...", statusStyle);
            }
            else
            {
                GUI.Label(new Rect(cardX + 104, cardY + 98, 320, 22), ArabicFixer.Fix("Loading your personal settings (جاري تحميل الإعدادات)..."), statusStyle);
            }
        }

        private void DrawDesktopIcons()
        {
            // Desktop Icon 1: Internet Explorer
            if (GUI.Button(new Rect(30, 30, 85, 80), "🌐\nInternet\nExplorer"))
            {
                PlaySound(_clickClip);
                _osState = WindowsXPState.InternetExplorer;
                _startMenuOpen = false;
            }

            // Desktop Icon 2: My Computer
            if (GUI.Button(new Rect(30, 125, 85, 75), "💻\nMy\nComputer"))
            {
                PlaySound(_clickClip);
            }

            // Desktop Icon 3: My Documents
            if (GUI.Button(new Rect(30, 215, 85, 75), "📁\nMy\nDocuments"))
            {
                PlaySound(_clickClip);
            }

            // Desktop Icon 4: Recycle Bin
            if (GUI.Button(new Rect(30, 305, 85, 75), "🗑️\nRecycle\nBin"))
            {
                PlaySound(_clickClip);
            }

            // Desktop Icon 5: Café Register
            if (GUI.Button(new Rect(30, 395, 85, 75), "☕\nCafé\nRegister"))
            {
                PlaySound(_clickClip);
            }
        }

        private void DrawInternetExplorerWindow(float w, float h)
        {
            float winW = Mathf.Min(980f, w - 80f);
            float winH = Mathf.Min(640f, h - 80f);
            float winX = (w - winW) / 2f;
            float winY = 24f;

            // 1. Title Bar
            GUI.DrawTexture(new Rect(winX, winY, winW, 28), _titleBarTex);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(winX + 10, winY + 4, winW - 120, 20), "🌐 Derb Express - سوق التجهيزات بالدار البيضاء - Microsoft Internet Explorer", titleStyle);

            // Window Controls: Minimize, Maximize, Close
            if (GUI.Button(new Rect(winX + winW - 75, winY + 3, 22, 22), "_"))
            {
                PlaySound(_clickClip);
                _osState = WindowsXPState.Desktop;
            }
            if (GUI.Button(new Rect(winX + winW - 50, winY + 3, 22, 22), "□"))
            {
                PlaySound(_clickClip);
            }

            GUI.color = new Color(0.85f, 0.2f, 0.15f);
            if (GUI.Button(new Rect(winX + winW - 26, winY + 3, 22, 22), "✕"))
            {
                PlaySound(_clickClip);
                _osState = WindowsXPState.Desktop;
            }
            GUI.color = Color.white;

            // 2. Menu Bar
            GUI.color = new Color(0.92f, 0.91f, 0.85f);
            GUI.DrawTexture(new Rect(winX, winY + 28, winW, 22), Texture2D.whiteTexture);
            GUI.color = Color.black;
            GUI.Label(new Rect(winX + 10, winY + 30, 400, 18), "File       Edit       View       Favorites       Tools       Help");
            GUI.color = Color.white;

            // 3. Navigation Toolbar
            GUI.color = new Color(0.92f, 0.91f, 0.85f);
            GUI.DrawTexture(new Rect(winX, winY + 50, winW, 36), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(winX + 8, winY + 54, 60, 26), "⬅ Back")) PlaySound(_clickClip);
            if (GUI.Button(new Rect(winX + 72, winY + 54, 65, 26), "Forward ➡")) PlaySound(_clickClip);
            if (GUI.Button(new Rect(winX + 141, winY + 54, 55, 26), "🛑 Stop")) PlaySound(_clickClip);
            if (GUI.Button(new Rect(winX + 200, winY + 54, 65, 26), "🔄 Refresh")) PlaySound(_clickClip);
            if (GUI.Button(new Rect(winX + 269, winY + 54, 55, 26), "🏠 Home")) PlaySound(_clickClip);

            // 4. Address Bar
            GUI.color = new Color(0.88f, 0.87f, 0.82f);
            GUI.DrawTexture(new Rect(winX, winY + 86, winW, 30), Texture2D.whiteTexture);
            GUI.color = Color.black;
            GUI.Label(new Rect(winX + 10, winY + 92, 60, 20), "Address:");
            GUI.color = Color.white;
            GUI.TextField(new Rect(winX + 70, winY + 90, winW - 145, 22), "http://www.derb-express.ma/store/cafe_equipment");
            if (GUI.Button(new Rect(winX + winW - 70, winY + 90, 60, 22), "➡ Go")) PlaySound(_clickClip);

            // 5. STORE BODY (DERB EXPRESS)
            float bodyY = winY + 116;
            float bodyH = winH - 116;
            GUI.DrawTexture(new Rect(winX, bodyY, winW, bodyH), _cardBgTex);

            // Red Announcement Ribbon
            GUI.color = new Color(0.79f, 0.1f, 0.1f);
            GUI.DrawTexture(new Rect(winX, bodyY, winW, 26), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            bannerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(winX, bodyY + 3, winW, 20), ArabicFixer.Fix("🚚 التوصيل مجاني وفوري إلى باب الكراج بدرب السلطان، الدار البيضاء"), bannerStyle);

            // Shop Header with Brand & Live Wallet
            float hdrY = bodyY + 32;
            var brandStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            brandStyle.normal.textColor = new Color(0.1f, 0.15f, 0.2f);
            GUI.Label(new Rect(winX + 20, hdrY, 350, 26), "🛒 DERB EXPRESS • درب إكسبريس", brandStyle);

            float currentCash = CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentDirhams : 100f;
            GUI.color = new Color(0.06f, 0.55f, 0.28f);
            GUI.DrawTexture(new Rect(winX + winW - 220, hdrY - 2, 200, 32), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var walletStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            walletStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(winX + winW - 220, hdrY + 3, 200, 24), ArabicFixer.Fix($"💰 الرصيد المتوفر: {currentCash:F0} DH"), walletStyle);

            // Day 1 Objective Banner
            float objY = hdrY + 40;
            GUI.color = new Color(0.12f, 0.24f, 0.55f);
            GUI.DrawTexture(new Rect(winX + 20, objY, winW - 40, 48), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var objStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            objStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(winX + 32, objY + 6, winW - 64, 40),
                ArabicFixer.Fix("📋 خطة اليوم الأول: 1. اشترِ سطل التنظيف والجفاف (30 DH) وسيِّق الغبرة كاملة.\n2. بعد تنظيف الأرضية، اطلب آلة الإسبريسو الإيطالية (100 DH) لتوضع فوق الكونتوار مباشرة!"),
                objStyle
            );

            // PRODUCT CARDS GRID
            float cardTop = objY + 58;
            float cardWidth = (winW - 60) * 0.5f;

            // CARD 1: MOP & BUCKET (30 DH)
            DrawProductCard(
                winX + 20, cardTop, cardWidth, 175,
                "🪣 سطل وعصارة + جفاف احترافي",
                "Pack Seau Essoreur + Balai Microfibre (Day 1)",
                "سعة 15L • عصارة ميكانيكية • جفاف مايكروفايبر 360°\nتوصيل فوري لباب الكراج خلال ثوانٍ.",
                "30 DH",
                _hasBoughtMop,
                true,
                "طلب دابا (30 DH)",
                () => BuyMopAndBucket()
            );

            // CARD 2: CLASSIC ESPRESSO MACHINE (100 DH)
            bool canBuyEspresso = _hasBoughtMop && CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.FloorCleanedBuyMachine;
            string espressoBtnText = canBuyEspresso ? "طلب دابا (100 DH)" : "🔒 سيق الأرضية أولاً";

            DrawProductCard(
                winX + 30 + cardWidth, cardTop, cardWidth, 175,
                "☕ آلة إسبريسو كلاسيكية 15 بار",
                "Machine Espresso Traditionnelle Commerciale",
                "مضخة 15 بار إيطالية • عصا بخار قوية للحليب وقهوة نص نص\n(خاص الأرضية تكون مسيقة كاملة عاد تقدر تشريها!)",
                "100 DH",
                _hasBoughtEspresso,
                canBuyEspresso,
                espressoBtnText,
                () => BuyEspressoMachine()
            );

            // TEASERS FOR UPCOMING DAYS (Card 3 & 4)
            float teaserTop = cardTop + 185;
            DrawTeaserCard(winX + 20, teaserTop, cardWidth, 65, "⚙️ مطحنة بن أوتوماتيكية", "75 DH", "🔒 كيتفتح فاليوم 2 (Day 2)");
            DrawTeaserCard(winX + 30 + cardWidth, teaserTop, cardWidth, 65, "🍵 براد أتاي مغربي أصيل وصينية فاسية", "50 DH", "🔒 كيتفتح فاليوم 2 (Day 2)");
        }

        private void DrawProductCard(float x, float y, float w, float h, string title, string sub, string specs, string price, bool isBought, bool isUnlocked, string btnLabel, Action onBuy)
        {
            // Background
            GUI.color = new Color(0.96f, 0.97f, 0.99f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = new Color(0.85f, 0.88f, 0.92f);
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = Color.white;

            // Title
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(0.08f, 0.12f, 0.18f);
            GUI.Label(new Rect(x + 12, y + 8, w - 24, 20), ArabicFixer.Fix(title), titleStyle);

            // Subtitle
            var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
            subStyle.normal.textColor = new Color(0.45f, 0.5f, 0.58f);
            GUI.Label(new Rect(x + 12, y + 26, w - 24, 18), sub, subStyle);

            // Specs
            var specStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            specStyle.normal.textColor = new Color(0.2f, 0.25f, 0.32f);
            GUI.Label(new Rect(x + 12, y + 46, w - 24, 45), ArabicFixer.Fix(specs), specStyle);

            // Price & Button Row
            float botY = y + h - 52;
            var priceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            priceStyle.normal.textColor = new Color(0.8f, 0.12f, 0.12f);
            GUI.Label(new Rect(x + 14, botY + 4, 120, 26), price, priceStyle);

            if (isBought)
            {
                GUI.color = new Color(0.1f, 0.6f, 0.3f);
                GUI.Label(new Rect(x + w - 190, botY + 6, 180, 28), ArabicFixer.Fix("✅ تم الطلب - وصل لباب الكراج!"));
                GUI.color = Color.white;
            }
            else
            {
                if (isUnlocked)
                {
                    GUI.color = new Color(0.12f, 0.7f, 0.35f);
                    if (GUI.Button(new Rect(x + w - 170, botY, 155, 36), ArabicFixer.Fix(btnLabel)))
                    {
                        onBuy?.Invoke();
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.8f, 0.8f, 0.8f);
                    GUI.Button(new Rect(x + w - 170, botY, 155, 36), ArabicFixer.Fix(btnLabel));
                    GUI.color = Color.white;
                }
            }
        }

        private void DrawTeaserCard(float x, float y, float w, float h, string title, string price, string badge)
        {
            GUI.color = new Color(0.92f, 0.93f, 0.95f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = new Color(0.82f, 0.84f, 0.88f);
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = Color.white;

            var tStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold };
            tStyle.normal.textColor = new Color(0.3f, 0.35f, 0.4f);
            GUI.Label(new Rect(x + 12, y + 8, w - 120, 20), ArabicFixer.Fix(title), tStyle);

            GUI.Label(new Rect(x + 12, y + 30, 80, 20), price);

            GUI.color = new Color(0.45f, 0.5f, 0.58f);
            GUI.Label(new Rect(x + w - 160, y + 20, 150, 20), ArabicFixer.Fix(badge));
            GUI.color = Color.white;
        }

        private void DrawTaskbar(float w, float h)
        {
            float tbY = h - 34f;
            GUI.DrawTexture(new Rect(0, tbY, w, 34), _taskbarTex);

            // Green Start Button
            GUI.DrawTexture(new Rect(0, tbY, 115, 34), _startBtnTex);
            var startStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.BoldAndItalic
            };
            startStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(38, tbY + 6, 70, 22), "start", startStyle);

            // Flag icon inside start button
            GUI.Label(new Rect(12, tbY + 6, 25, 22), "🪟");

            if (GUI.Button(new Rect(0, tbY, 115, 34), "", GUIStyle.none))
            {
                PlaySound(_clickClip);
                _startMenuOpen = !_startMenuOpen;
            }

            // Taskbar item: Internet Explorer
            if (_osState == WindowsXPState.InternetExplorer)
            {
                GUI.color = new Color(0.12f, 0.35f, 0.75f);
                GUI.DrawTexture(new Rect(125, tbY + 3, 210, 28), Texture2D.whiteTexture);
                GUI.color = Color.white;

                if (GUI.Button(new Rect(125, tbY + 3, 210, 28), "🌐 Derb Express - سوق..."))
                {
                    PlaySound(_clickClip);
                }
            }

            // System Tray
            float trayW = 160f;
            float trayX = w - trayW;
            GUI.color = new Color(0.08f, 0.42f, 0.85f);
            GUI.DrawTexture(new Rect(trayX, tbY, trayW, 34), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(trayX + 10, tbY + 8, 75, 20), "🇲🇦  🔊  💻");

            string timeStr = DateTime.Now.ToString("hh:mm tt");
            GUI.Label(new Rect(trayX + 85, tbY + 8, 70, 20), timeStr);
        }

        private void DrawStartMenu(float w, float h)
        {
            float smW = 280f;
            float smH = 260f;
            float smX = 0f;
            float smY = h - 34f - smH;

            // Start Menu Header
            GUI.color = new Color(0.11f, 0.38f, 0.85f);
            GUI.DrawTexture(new Rect(smX, smY, smW, 46), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(smX + 14, smY + 12, 30, 30), "☕");
            var smUserStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            smUserStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(smX + 46, smY + 14, 220, 20), "Derb Sultan Barista", smUserStyle);

            // Start Menu Body
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(smX, smY + 46, smW, smH - 86), Texture2D.whiteTexture);

            if (GUI.Button(new Rect(smX + 10, smY + 54, smW - 20, 34), "🌐 Internet Explorer"))
            {
                PlaySound(_clickClip);
                _osState = WindowsXPState.InternetExplorer;
                _startMenuOpen = false;
            }

            if (GUI.Button(new Rect(smX + 10, smY + 94, smW - 20, 34), "💻 My Computer"))
            {
                PlaySound(_clickClip);
                _startMenuOpen = false;
            }

            if (GUI.Button(new Rect(smX + 10, smY + 134, smW - 20, 34), "📁 My Documents"))
            {
                PlaySound(_clickClip);
                _startMenuOpen = false;
            }

            if (GUI.Button(new Rect(smX + 10, smY + 174, smW - 20, 34), "📝 Notes"))
            {
                PlaySound(_clickClip);
                _startMenuOpen = false;
            }

            // Start Menu Footer
            float footY = smY + smH - 40;
            GUI.color = new Color(0.18f, 0.44f, 0.85f);
            GUI.DrawTexture(new Rect(smX, footY, smW, 40), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(smX + 10, footY + 6, 120, 28), "🔑 Log Off"))
            {
                PlaySound(_clickClip);
                _osState = WindowsXPState.LoginTypingPassword;
                _typedPassword = "";
                _passwordTimer = 0f;
                _startMenuOpen = false;
            }

            if (GUI.Button(new Rect(smX + 140, footY + 6, 130, 28), "🔴 Turn Off PC"))
            {
                PlaySound(_clickClip);
                _isOpen = false;
                _startMenuOpen = false;
            }
        }

        private void DrawToastNotification(float w, float h)
        {
            float tw = 360f;
            float th = 70f;
            float tx = w - tw - 20f;
            float ty = h - 34f - th - 15f;

            GUI.color = new Color(0.1f, 0.14f, 0.22f, 0.95f);
            GUI.DrawTexture(new Rect(tx, ty, tw, th), Texture2D.whiteTexture);
            GUI.color = new Color(0.06f, 0.75f, 0.4f);
            GUI.DrawTexture(new Rect(tx, ty, 5, th), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var toastTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            toastTitleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(tx + 16, ty + 8, tw - 24, 22), _toastTitle, toastTitleStyle);

            var toastDescStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            toastDescStyle.normal.textColor = new Color(0.7f, 0.78f, 0.88f);
            GUI.Label(new Rect(tx + 16, ty + 30, tw - 24, 34), _toastDesc, toastDescStyle);
        }

        // ========================================================
        // PROCEDURAL AUDIO & TEXTURES
        // ========================================================

        private void SetupAudio()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            // Generate synthetic Click sound
            _clickClip = CreateTone(1200f, 400f, 0.04f, 0.2f);

            // Generate synthetic Windows XP Startup chord
            _startupClip = CreateXpStartupChime();

            // Generate synthetic Cash register chime
            _cashClip = CreateTone(1900f, 2400f, 0.4f, 0.35f);
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private AudioClip CreateTone(float startFreq, float endFreq, float duration, float gain)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = 1f - t;
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * (float)i / sampleRate) * env * gain;
            }

            AudioClip clip = AudioClip.Create("SynthTone", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateXpStartupChime()
        {
            int sampleRate = 44100;
            float duration = 2.5f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] freqs = new float[] { 277.18f, 415.30f, 622.25f, 830.61f, 932.33f, 1396.91f };
            float[] delays = new float[] { 0.0f, 0.12f, 0.28f, 0.45f, 0.62f, 0.80f };

            for (int f = 0; f < freqs.Length; f++)
            {
                float freq = freqs[f];
                int startSample = (int)(delays[f] * sampleRate);

                for (int i = startSample; i < totalSamples; i++)
                {
                    float t = (float)(i - startSample) / (totalSamples - startSample);
                    float env = Mathf.Exp(-t * 3.5f);
                    samples[i] += Mathf.Sin(2 * Mathf.PI * freq * (float)(i - startSample) / sampleRate) * env * 0.12f;
                }
            }

            AudioClip clip = AudioClip.Create("XPStartup", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void GenerateTextures()
        {
            // 1. Bliss Wallpaper (Sky + Rolling Green Hills)
            _blissTex = new Texture2D(32, 32, TextureFormat.RGB24, false);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float v = (float)y / 32f;
                    Color col;
                    if (v < 0.4f)
                    {
                        // Green rolling hills
                        float hill = Mathf.Sin(x * 0.3f) * 0.05f;
                        float gVal = (v + hill) / 0.4f;
                        col = Color.Lerp(new Color(0.18f, 0.48f, 0.05f), new Color(0.42f, 0.72f, 0.12f), gVal);
                    }
                    else
                    {
                        // Sky
                        float sVal = (v - 0.4f) / 0.6f;
                        col = Color.Lerp(new Color(0.72f, 0.88f, 0.98f), new Color(0.16f, 0.44f, 0.85f), sVal);
                    }
                    _blissTex.SetPixel(x, y, col);
                }
            }
            _blissTex.Apply();

            // 2. Luna Blue Title Bar
            _titleBarTex = new Texture2D(1, 16, TextureFormat.RGB24, false);
            for (int y = 0; y < 16; y++)
            {
                float t = (float)y / 16f;
                Color col = Color.Lerp(new Color(0.0f, 0.24f, 0.84f), new Color(0.04f, 0.58f, 1.0f), t);
                _titleBarTex.SetPixel(0, y, col);
            }
            _titleBarTex.Apply();

            // 3. Luna Blue Taskbar
            _taskbarTex = new Texture2D(1, 16, TextureFormat.RGB24, false);
            for (int y = 0; y < 16; y++)
            {
                float t = (float)y / 16f;
                Color col = Color.Lerp(new Color(0.08f, 0.18f, 0.45f), new Color(0.18f, 0.42f, 0.85f), t);
                _taskbarTex.SetPixel(0, y, col);
            }
            _taskbarTex.Apply();

            // 4. Green Start Button
            _startBtnTex = new Texture2D(1, 16, TextureFormat.RGB24, false);
            for (int y = 0; y < 16; y++)
            {
                float t = (float)y / 16f;
                Color col = Color.Lerp(new Color(0.12f, 0.38f, 0.12f), new Color(0.32f, 0.72f, 0.28f), t);
                _startBtnTex.SetPixel(0, y, col);
            }
            _startBtnTex.Apply();

            // 5. Card & Login Backgrounds
            _cardBgTex = new Texture2D(1, 1, TextureFormat.RGB24, false);
            _cardBgTex.SetPixel(0, 0, new Color(0.96f, 0.97f, 0.98f));
            _cardBgTex.Apply();

            _loginBgTex = new Texture2D(1, 16, TextureFormat.RGB24, false);
            for (int y = 0; y < 16; y++)
            {
                float t = (float)y / 16f;
                Color col = Color.Lerp(new Color(0.0f, 0.1f, 0.35f), new Color(0.0f, 0.22f, 0.55f), t);
                _loginBgTex.SetPixel(0, y, col);
            }
            _loginBgTex.Apply();
        }
    }
}
