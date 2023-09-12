using System;
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

        [SerializeField] private VisualTreeAsset contractAsset;

        private PlayerAvatar avatar;
        private PlayerContractManager contractManager;
        private UIDocument hud;

        private ProgressBar shieldsFill;
        private Label shieldsText;
        private Compass compass;
        private VisualElement vignette;
        private VisualElement deathScreen;
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
            vignette = root.Q("vignette");
            lookingAt = root.Q<Label>("looking-at");
            contractContainer = root.Q("contracts").Q("content");

            SetupDeathUI();
        }

        private void Update()
        {
            RebuildContracts();

            compass.FaceAngle = avatar.transform.eulerAngles.y;

            var lookingAt = avatar.interactionManager.HighlightedObject;
            this.lookingAt.text = (Object)lookingAt ? $"{lookingAt.DisplayName}\n{lookingAt.DisplayValue}" : string.Empty;

            vignette.style.opacity = Mathf.Max
            (
                GetVignetteOpacity(),
                GetDamageFlash()
            );
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
            shieldsText.text = $"<size=50%>SHIELDS</size>\n[ {(avatar.vitality.Exposed ? "ACTIVE" : " IDLE ")} | {Mathf.Max(0, Mathf.FloorToInt(avatar.vitality.CurrentShields)),2:N0} s ]";
        }

        private void SetupDeathUI()
        {
            var root = avatar.transform.Find("Death Screen").GetComponent<UIDocument>().rootVisualElement;
            deathScreen = root;

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
        }

        private void RebuildContracts()
        {
            contractContainer.Clear();

            if (!contractManager) return;

            foreach (var contract in contractManager.ActiveContracts)
            {
                if (!contract) return;
                BuildContract(contract);
            }
        }

        private void BuildContract(Contract contract)
        {
            var root = contractAsset.Instantiate();
            contractContainer.Add(root);

            var header = root.Q<Label>("header");
            header.text = contract.name.ToUpper();

            var predicateContainer = root.Q("predicates");
            predicateContainer.Clear();
            foreach (var e in contract.predicates)
            {
                var progressBar = new ProgressBar();
                predicateContainer.Add(progressBar);
                progressBar.title = e.ToString().ToUpper();
                progressBar.value = e.Progress * 100.0f;
            }
        }
    }
}