using System;
using System.Collections;
using System.Collections.Generic;
using FR8.Runtime.CodeUtility;
using FR8.Runtime.Gamemodes;
using FR8.Runtime.Interactions.Drivers.Submodules;
using FR8.Runtime.UI;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Compass = FR8.Runtime.UI.CustomControls.Compass;
using Cursor = UnityEngine.Cursor;
using Object = UnityEngine.Object;
using SceneUtility = FR8.Runtime.CodeUtility.SceneUtility;

namespace FR8.Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerUI
    {
        [SerializeField] private AnimationCurve healthVignetteCurve = new
        (
            new Keyframe(0.0f, 1.0f),
            new Keyframe(0.1f, 1.0f),
            new Keyframe(0.4f, 0.2f),
            new Keyframe(0.8f, 0.0f),
            new Keyframe(1.0f, 0.0f)
        );

        [SerializeField] private float damageFlashTime;

        [Space]
        [SerializeField] private Color baseColor = Color.white;

        [SerializeField] private Color shieldColor = Color.cyan;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashTiming = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float flashSplit = 0.5f;

        [Space]
        [SerializeField] private Texture2D scrollIcon;
        [SerializeField] private Texture2D pressIcon;

        private PlayerAvatar avatar;
        private UIDocument hud;
        private Renderer hudRenderer;
        private MaterialPropertyBlock hudRendererProperties;

        private ProgressBar shieldsFill;
        private Label shieldsText;
        private Compass compass;
        private Label longitude;
        private Label latitude;
        private VisualElement vignette;
        private VisualElement deathScreen;
        private VisualElement endScreen;
        private VisualElement lookingAtRoot;
        private VisualElement lookingAtIcon;
        private Label lookingAtText;
        private Label contractText;
        private Label toastLabel;
        
        private List<Toast> toasts = new();

        private float spawnTime;

        private const string VitalityRootPath = "UI/Vitality";
        private const string CoverPath = VitalityRootPath + "/Death";

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            avatar.vitality.IsAliveChangedEvent += UpdateDeathUI;
            avatar.vitality.HealthChangeEvent += UpdateBars;
            avatar.UpdateEvent += Update;
            avatar.EnableEvent += OnEnable;
            avatar.DisableEvent += OnDisable;
            avatar.StartEvent += Start;
        }

        private void Start()
        {
            hud = avatar.transform.Find("HUD").GetComponent<UIDocument>();
            var root = hud.rootVisualElement;

            shieldsFill = root.Q<ProgressBar>("shields-bar");
            shieldsText = root.Q<Label>("shields-text");
            compass = root.Q<Compass>("compass");
            longitude = root.Q<Label>("long");
            latitude = root.Q<Label>("lat");
            lookingAtRoot = root.Q("looking-at");
            lookingAtIcon = root.Q("looking-at-icon");
            lookingAtText = root.Q<Label>("looking-at-text");
            contractText = root.Q<Label>("contracts");
            toastLabel = root.Q<Label>("toast");

            hudRenderer = avatar.transform.Find("Head/HUD Quad/Quad").GetComponent<Renderer>();
            hudRendererProperties = new MaterialPropertyBlock();
            hudRenderer.SetPropertyBlock(hudRendererProperties);

            vignette = avatar.transform.Find("Vignette").GetComponent<UIDocument>().rootVisualElement.Q("vignette");

            spawnTime = Time.time;

            SetupDeathUI();
            HideEndgameUI();
        }

        private void OnEnable()
        {
            Contract.ContractCompleteEvent += OnContractCompleted;
            Toast.ShowToastEvent += toasts.Add;
        }

        private void OnDisable()
        {
            Contract.ContractCompleteEvent -= OnContractCompleted;
            Toast.ShowToastEvent -= toasts.Add;
        }

        private void OnContractCompleted(Contract contract)
        {
            if (Contract.ActiveContracts.Count > 0) return;
            ShowEndgameUI();
        }

        private void Update()
        {
            if (Keyboard.current.f2Key.wasPressedThisFrame) ShowEndgameUI();
            if (Keyboard.current.f3Key.wasPressedThisFrame) HideEndgameUI();

            if (compass != null) compass.FaceAngle = avatar.transform.eulerAngles.y;

            var lookingAt = avatar.interactionManager.HighlightedObject;
            if (lookingAt != null)
            {
                lookingAtRoot.visible = true;
                lookingAtIcon.style.backgroundImage = lookingAt.Type switch
                {
                    IInteractable.InteractionType.Scroll => scrollIcon,
                    IInteractable.InteractionType.Press => pressIcon,
                    _ => throw new ArgumentOutOfRangeException()
                };
                lookingAtText.text = (Object)lookingAt ? $"{lookingAt.DisplayName}\n{lookingAt.DisplayValue}".ToUpper() : string.Empty;
            }
            else
            {
                lookingAtRoot.visible = false;
            }

            vignette.style.opacity = Mathf.Max
            (
                GetVignetteOpacity(),
                GetDamageFlash()
            );

            var hudColor = baseColor;
            if (avatar.vitality.Exposed)
            {
                hudColor = shieldColor;
            }

            if (avatar.vitality.CurrentShields <= 5.0f)
            {
                hudColor = Time.time / GetFlashTiming() % 1.0f > flashSplit ? hudColor : flashColor;
            }

            hudRendererProperties.SetColor("_Color", hudColor);
            hudRenderer.SetPropertyBlock(hudRendererProperties);

            longitude.text = $"LONG: {avatar.transform.position.x / 80000.0f + 23.6f:N5}";
            latitude.text = $"LAT: {avatar.transform.position.z / 80000.0f + 94.2f:N5}";

            BuildContractUI();
            UpdateToasts();
        }

        private void UpdateToasts()
        {
            if (toastLabel == null) return;
            
            toasts.RemoveAll(e => e.Expired());
            var toastText = string.Empty;
            for (var i = toasts.Count; i > 0; i--)
            {
                var toast = toasts[i - 1];
                toastText += $"{toast.Build()}\n";
            }
            toastLabel.text = toastText;
        }

        private float GetFlashTiming()
        {
            if (avatar.vitality.CurrentHealth / (float)avatar.vitality.maxHealth < 0.5f) return flashTiming * 0.2f;
            if (avatar.vitality.CurrentShields <= 0.1f) return flashTiming * 0.5f;

            return flashTiming;
        }

        private float GetVignetteOpacity()
        {
            return healthVignetteCurve.Evaluate(avatar.vitality.CurrentHealth / (float)avatar.vitality.maxHealth);
        }

        private float GetDamageFlash()
        {
            var t = Time.time - avatar.vitality.LastDamageTime;
            return Mathf.Exp(-4.0f * t / damageFlashTime);
        }

        private void UpdateBars()
        {
            shieldsFill.value = avatar.vitality.CurrentShields / avatar.vitality.shieldDuration;

            var shieldStatus = string.Empty;
            if (avatar.vitality.Exposed)
            {
                shieldStatus = avatar.vitality.CurrentShields >= 0.0f ? "ACTIVE" : "DEPLETED";
            }
            else
            {
                shieldStatus = avatar.vitality.CurrentShields <= (avatar.vitality.shieldDuration - 0.1f) ? "REGENERATING" : "IDLE";
            }

            shieldsText.text = $"[ {shieldStatus} | {Mathf.Max(0, Mathf.FloorToInt(avatar.vitality.CurrentShields)),2:N0} s ]";
        }

        private void SetupDeathUI()
        {
            var root = avatar.transform.Find("Death Screen").GetComponent<UIDocument>().rootVisualElement;
            deathScreen = root;

            var respawn = root.Q<Button>("respawn");
            respawn.clickable.clicked += UIActions.Load(SceneUtility.Scene.Reload);
            var exit = root.Q<Button>("exit");
            exit.clickable.clicked += () =>
            {
                avatar.vitality.Revive();
                SceneUtility.LoadScene(SceneUtility.Scene.Menu);
            };

            UpdateDeathUI();
        }

        private void UpdateDeathUI()
        {
            deathScreen.visible = !avatar.IsAlive;

            if (deathScreen.visible)
            {
                avatar.StartCoroutine(routine());
            }

            IEnumerator routine()
            {
                var root = deathScreen;

                var typewritterElements = new List<TextElement>();
                typewritterElements.Add(root.Q<Label>("subtitle"));
                typewritterElements.Add(root.Q<Label>("title"));

                var buttonContainer = root.Q("buttons");
                foreach (var child in buttonContainer.Children())
                {
                    if (child is TextElement textElement)
                    {
                        typewritterElements.Add(textElement);
                        textElement.visible = false;
                    }
                }

                const float typewriterCps = 25.0f;
                const float postDelay = 0.5f;

                yield return UitkUtility.Typewriter(typewriterCps, postDelay, typewritterElements.ToArray());
            }
        }

        private VisualElement GetEndgameUI()
        {
            if (endScreen == null) endScreen = avatar.transform.Find("End Screen").GetComponent<UIDocument>().rootVisualElement;
            return endScreen;
        }

        private void HideEndgameUI()
        {
            var root = GetEndgameUI();
            root.visible = false;
        }

        private void ShowEndgameUI()
        {
            Pause.SetPaused(true);
            Cursor.lockState = CursorLockMode.None;
            var root = GetEndgameUI();
            root.visible = true;

            var subtitle = root.Q<Label>("subtitle");
            var title = root.Q<Label>("title");
            var content = root.Q<Label>("content");
            var retry = root.Q<Button>("retry");
            var exit = root.Q<Button>("exit");

            retry.clickable.clicked += UIActions.Load(SceneUtility.Scene.Reload);
            exit.clickable.clicked += UIActions.Load(SceneUtility.Scene.Menu);

            var ratio = 1.45f / content.resolvedStyle.fontSize;

            var time = TimeSpan.FromSeconds(Time.time - spawnTime);
            content.text = "";
            appendContent("time", time.ToString("m\\:ss"));

            avatar.StartCoroutine(routine());

            void appendContent(object key, object value)
            {
                var keyStr = key.ToString().ToUpper();
                var valueStr = value.ToString().ToUpper();

                var textWidth = (keyStr.Length + valueStr.Length + 3) / ratio;
                var padding = Mathf.FloorToInt((content.layout.width - textWidth) * ratio);

                content.text += $">{keyStr} {new string('-', padding)} {valueStr}\n";
            }

            IEnumerator routine()
            {
                var routines = new[]
                {
                    UitkUtility.Typewriter(25.0f, 0.5f, subtitle, title),
                    UitkUtility.Typewriter(100.0f, 0.5f, content),
                    UitkUtility.Typewriter(25.0f, 0.5f, retry),
                    UitkUtility.Typewriter(25.0f, 0.5f, exit)
                };

                foreach (var r in routines) yield return avatar.StartCoroutine(r);
            }
        }

        private void BuildContractUI()
        {
            contractText.Clear();

            contractText.text = string.Empty;
            switch (Contract.ActiveContracts.Count)
            {
                case 0:
                {
                    contractText.text += "No Contracts Currently Active";
                    break;
                }
                case 1:
                {
                    var contract = Contract.ActiveContracts[0];
                    if (!contract) return;
                    contractText.text = contract.BuildUI();

                    break;
                }
                default:
                {
                    contractText.text = $"Active Contract{(Contract.ActiveContracts.Count > 1 ? "s" : "")}\n";
                    foreach (var contract in Contract.ActiveContracts)
                    {
                        if (!contract) return;
                        contractText.text += contract.BuildUI();
                    }

                    break;
                }
            }

            contractText.text = contractText.text.ToUpper();
        }
    }
}