using System;
using System.Collections;
using FR8Runtime.CodeUtility;
using FR8Runtime.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using Compass = FR8Runtime.UI.CustomControls.Compass;
using Object = UnityEngine.Object;

namespace FR8Runtime.Player.Submodules
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

        private PlayerAvatar avatar;
        private PlayerContractManager contractManager;
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
        private VisualElement deathCover;
        private Label lookingAt;
        private VisualElement contractContainer;

        private const string VitalityRootPath = "UI/Vitality";
        private const string CoverPath = VitalityRootPath + "/Death";

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;
            contractManager = avatar.GetComponent<PlayerContractManager>();

            avatar.vitality.IsAliveChangedEvent += UpdateDeathUI;
            avatar.vitality.HealthChangeEvent += UpdateBars;
            avatar.UpdateEvent += Update;

            hud = avatar.transform.Find("HUD").GetComponent<UIDocument>();
            var root = hud.rootVisualElement;

            shieldsFill = root.Q<ProgressBar>("shields-bar");
            shieldsText = root.Q<Label>("shields-text");
            compass = root.Q<Compass>("compass");
            longitude = root.Q<Label>("long");
            latitude = root.Q<Label>("lat");
            lookingAt = root.Q<Label>("looking-at");
            contractContainer = root.Q("contracts").Q("content");

            hudRenderer = avatar.transform.Find("Head/HUD Quad/Quad").GetComponent<Renderer>();
            hudRendererProperties = new MaterialPropertyBlock();
            hudRenderer.SetPropertyBlock(hudRendererProperties);
            
            vignette = avatar.transform.Find("Vignette").GetComponent<UIDocument>().rootVisualElement.Q("vignette");

            SetupDeathUI();
            BuildContractUI();
        }

        private void Update()
        {
            compass.FaceAngle = avatar.transform.eulerAngles.y;

            var lookingAt = avatar.interactionManager.HighlightedObject;
            this.lookingAt.text = (Object)lookingAt ? $"{lookingAt.DisplayName}\n{lookingAt.DisplayValue}".ToUpper() : string.Empty;
            this.lookingAt.EnableInClassList("active", (Object)lookingAt);

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
            deathCover = root.Q("cover");

            var respawn = root.Q<Button>("respawn");
            respawn.clickable.clicked += avatar.vitality.Revive;
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
                yield return new WaitForSecondsRealtime(1.0f);
                deathCover.style.opacity = 1.0f;
                
                var p = 0.0f;
                while (p < 1.0f)
                {
                    deathCover.style.opacity = 1.0f - p;
                    p += Time.unscaledDeltaTime / 2.0f;
                    yield return null;
                }
                
                deathCover.visible = false;
            }
        }

        private void BuildContractUI()
        {
            contractContainer.Clear();

            if (!contractManager) return;

            foreach (var contract in contractManager.ActiveContracts)
            {
                if (!contract) return;
                contractContainer.Add(contract.BuildUI());
            }
        }
    }
}