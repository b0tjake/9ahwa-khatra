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

        public string PromptMessage
        {
            get
            {
                if (CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.SellingJunk)
                {
                    return ArabicFixer.Fix("جمع الخردة وبِعها عاد خدم البيسي (Clear junk first!)");
                }
                return ArabicFixer.Fix("حل البيسي (Windows XP)");
            }
        }

        public void OnInteract(PlayerInteraction interactor)
        {
            // Block laptop interaction if junk still in garage
            if (CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.SellingJunk)
            {
                Debug.Log("[Laptop] Clear and sell the junk in the garage first before using the computer!");
                return;
            }

            _isOpen = !_isOpen;
            if (_isOpen)
            {
                // Trigger realistic XP auto-boot & login sequence!
                _osState = WindowsXPState.BootWelcome;
                _loginTimer = 0f;
                _startMenuOpen = false;
            }
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Auto-login sequence like genuine Windows XP
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
                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.AcquireMop();
                }
                Debug.Log("[Windows XP] Order Placed: Mop & Bucket (سطل وجفاف) for 30 DH!");
            }
        }

        public void BuyEspressoMachine()
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendDirhams(100f))
            {
                _hasBoughtEspresso = true;
                SpawnEspressoMachine();

                if (CleaningManager.Instance != null)
                {
                    CleaningManager.Instance.CompleteEspressoPurchase();
                }
                Debug.Log("[Windows XP] Order Placed: Basic Espresso Machine for 100 DH!");
            }
        }

        private void SpawnEspressoMachine()
        {
            var machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machine.name = "Espresso_Machine";
            machine.transform.position = new Vector3(-2f, 1.4f, 4f);
            machine.transform.localScale = new Vector3(0.9f, 0.8f, 0.7f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.75f, 0.15f, 0.15f);
            machine.GetComponent<Renderer>().material = mat;

            machine.AddComponent<QahwaKhatra.CoffeeCrafting.EspressoStation>();
        }

        private void OnGUI()
        {
            if (!_isOpen) return;

            // Authentic 4:3 Windows XP monitor resolution frame
            float w = Mathf.Min(640f, Screen.width - 40f);
            float h = Mathf.Min(480f, Screen.height - 40f);
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            // Monitor Bezel
            GUI.Box(new Rect(x - 8, y - 8, w + 16, h + 16), "");

            // 1. WELCOME / LOGIN SCREEN
            if (_osState == WindowsXPState.BootWelcome || _osState == WindowsXPState.LoggingIn)
            {
                // Iconic Windows XP Deep Blue background
                GUI.color = new Color(0.0f, 0.2f, 0.65f);
                GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Center banner
                float cx = x + w / 2f;
                float cy = y + h / 2f;

                GUI.Label(new Rect(cx - 150, cy - 60, 300, 35), "Microsoft Windows XP Professional");

                if (_osState == WindowsXPState.BootWelcome)
                {
                    GUI.Label(new Rect(cx - 100, cy - 10, 200, 30), "Welcome / مرحباً بك");
                }
                else
                {
                    GUI.Label(new Rect(cx - 140, cy - 10, 280, 30), "Loading your personal settings...");
                    GUI.Label(new Rect(cx - 140, cy + 20, 280, 30), "تسجيل الدخول التلقائي: Derb_Sultan_User");
                }

                if (GUI.Button(new Rect(x + w - 75, y + 10, 65, 25), "Turn Off"))
                {
                    _isOpen = false;
                }
                return;
            }

            // 2. WINDOWS XP DESKTOP (BLISS GREEN/BLUE)
            // Desktop background (Bliss grass green / cyan sky)
            GUI.color = new Color(0.25f, 0.65f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h - 35), Texture2D.whiteTexture);
            // Grass lower half
            GUI.color = new Color(0.2f, 0.68f, 0.22f);
            GUI.DrawTexture(new Rect(x, y + h * 0.55f, w, h * 0.45f - 35), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Desktop Icons
            // Icon 1: Recycle Bin
            if (GUI.Button(new Rect(x + 20, y + 25, 75, 60), "🗑️\nRecycle Bin"))
            {
                // Just for authentic XP vibe
            }

            // Icon 2: Internet Explorer (E-Shopping)
            if (GUI.Button(new Rect(x + 20, y + 95, 75, 65), "🌐\nInternet\nExplorer"))
            {
                _osState = WindowsXPState.InternetExplorer;
                _startMenuOpen = false;
            }

            // Icon 3: My Computer
            if (GUI.Button(new Rect(x + 20, y + 170, 75, 60), "💻\nMy Computer"))
            {
            }

            // 3. INTERNET EXPLORER WINDOW
            if (_osState == WindowsXPState.InternetExplorer)
            {
                float winW = w - 80f;
                float winH = h - 90f;
                float winX = x + 40f;
                float winY = y + 35f;

                // Window Title Bar (Windows XP Classic Luna Blue)
                GUI.color = new Color(0.05f, 0.35f, 0.9f);
                GUI.DrawTexture(new Rect(winX, winY, winW, 28), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(winX + 10, winY + 4, 300, 22), "🌐 Internet Explorer - www.derb-express.ma");

                // Red Close 'X' Button
                GUI.color = new Color(0.85f, 0.2f, 0.2f);
                if (GUI.Button(new Rect(winX + winW - 32, winY + 2, 28, 24), "X"))
                {
                    _osState = WindowsXPState.Desktop;
                }
                GUI.color = Color.white;

                // Browser Address Bar
                GUI.Box(new Rect(winX, winY + 28, winW, 30), "");
                GUI.Label(new Rect(winX + 10, winY + 34, 60, 20), "Address:");
                GUI.TextField(new Rect(winX + 70, winY + 32, winW - 80, 22), "http://www.derb-express.ma/store/clean_and_cafe");

                // Webpage Content Body
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(winX, winY + 58, winW, winH - 58), Texture2D.whiteTexture);

                float contentY = winY + 70;

                // Website Header banner
                GUI.color = new Color(0.85f, 0.15f, 0.15f);
                GUI.DrawTexture(new Rect(winX + 10, contentY, winW - 20, 35), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(winX + 20, contentY + 7, winW - 40, 25), ArabicFixer.Fix("🛒 درب إكسبريس — سوق التجهيزات بالدار البيضاء"));

                // ITEM 1: MOP & BUCKET (سطل وجفاف)
                float item1Y = contentY + 50;
                GUI.Box(new Rect(winX + 15, item1Y, winW - 30, 80), "");
                GUI.Label(new Rect(winX + 25, item1Y + 10, winW - 170, 25), ArabicFixer.Fix("🪣 سطل وجفاف احترافي (Mop & Bucket Set)"));
                GUI.Label(new Rect(winX + 25, item1Y + 35, winW - 170, 40), ArabicFixer.Fix("ضروري باش تسيق الغبرة وتوجد الكراج.\nالثمن: 30 درهم (30 DH)"));

                if (!_hasBoughtMop)
                {
                    if (GUI.Button(new Rect(winX + winW - 145, item1Y + 20, 120, 40), ArabicFixer.Fix("شري دابا (30 DH)")))
                    {
                        BuyMopAndBucket();
                    }
                }
                else
                {
                    GUI.Label(new Rect(winX + winW - 145, item1Y + 25, 120, 30), ArabicFixer.Fix("✅ تم الشراء!"));
                }

                // ITEM 2: ESPRESSO MACHINE (آلة القهوة)
                float item2Y = item1Y + 90;
                GUI.Box(new Rect(winX + 15, item2Y, winW - 30, 95), "");
                GUI.Label(new Rect(winX + 25, item2Y + 8, winW - 170, 25), ArabicFixer.Fix("☕ آلة إسبريسو كلاسيك (Classic Espresso Machine)"));
                GUI.Label(new Rect(winX + 25, item2Y + 32, winW - 170, 55), ArabicFixer.Fix("القلب النابض ديال المقهى لتحضير قهوة خاترة.\n(خاص الأرضية تكون مسيقة كاملة عاد تشريها!)\nالثمن: 100 درهم (100 DH)"));

                // Condition: Floor must be cleaned before buying coffee machine!
                bool canBuyEspresso = _hasBoughtMop && CleaningManager.Instance != null && CleaningManager.Instance.CurrentPhase == Day1Phase.FloorCleanedBuyMachine;

                if (!_hasBoughtEspresso)
                {
                    if (canBuyEspresso)
                    {
                        if (GUI.Button(new Rect(winX + winW - 145, item2Y + 25, 120, 45), ArabicFixer.Fix("شري دابا (100 DH)")))
                        {
                            BuyEspressoMachine();
                        }
                    }
                    else
                    {
                        string lockReason = !_hasBoughtMop ? "خاصك سطل وجفاف أولاً" : "سيق الغبرة كاملة أولاً!";
                        GUI.Label(new Rect(winX + winW - 145, item2Y + 30, 125, 40), ArabicFixer.Fix($"🔒 {lockReason}"));
                    }
                }
                else
                {
                    GUI.Label(new Rect(winX + winW - 145, item2Y + 30, 120, 30), ArabicFixer.Fix("✅ تم الشراء!"));
                }
            }

            // 4. TASKBAR (XP BLUE with GREEN START BUTTON)
            float tbY = y + h - 35f;
            GUI.color = new Color(0.12f, 0.38f, 0.88f); // Windows XP Luna taskbar blue
            GUI.DrawTexture(new Rect(x, tbY, w, 35), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Green "start" button
            GUI.color = new Color(0.24f, 0.65f, 0.18f); // Classic start green
            if (GUI.Button(new Rect(x, tbY, 95, 35), "🟢 start"))
            {
                _startMenuOpen = !_startMenuOpen;
            }
            GUI.color = Color.white;

            // Active Task in Taskbar
            if (_osState == WindowsXPState.InternetExplorer)
            {
                if (GUI.Button(new Rect(x + 105, tbY + 4, 160, 27), "🌐 Derb-Express.ma"))
                {
                    // Minimize / Focus
                }
            }

            // Clock on bottom-right
            GUI.Label(new Rect(x + w - 85, tbY + 8, 80, 20), "04:20 PM");

            // 5. XP START MENU POPUP
            if (_startMenuOpen)
            {
                float smW = 180f;
                float smH = 160f;
                float smX = x;
                float smY = tbY - smH;

                GUI.Box(new Rect(smX, smY, smW, smH), "Windows XP");

                if (GUI.Button(new Rect(smX + 10, smY + 30, smW - 20, 30), "🌐 Internet Explorer"))
                {
                    _osState = WindowsXPState.InternetExplorer;
                    _startMenuOpen = false;
                }

                if (GUI.Button(new Rect(smX + 10, smY + 68, smW - 20, 30), "💻 My Computer"))
                {
                    _startMenuOpen = false;
                }

                if (GUI.Button(new Rect(smX + 10, smY + 110, smW - 20, 35), "🔴 Log Off / Close"))
                {
                    _isOpen = false;
                    _startMenuOpen = false;
                }
            }
        }
    }
}
