using System;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;
using Compass = FR8Runtime.UI.CustomControls.Compass;

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
        
        private PlayerAvatar avatar;
        private UIDocument hud;

        private ProgressBar shieldsFill;
        private Label shieldsText;
        private Compass compass;
        private VisualElement vignette;
        private VisualElement deathScreen;

        private const string VitalityRootPath = "UI/Vitality";
        private const string CoverPath = VitalityRootPath + "/Death";

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            avatar.vitality.IsAliveChangedEvent += UpdateDeathUI;
            avatar.vitality.HealthChangeEvent += UpdateBars;
            avatar.UpdateEvent += Update;

            hud = avatar.transform.Find("HUD").GetComponent<UIDocument>();
            var root = hud.rootVisualElement;

            shieldsFill = root.Q<ProgressBar>("shields-bar");
            shieldsText = root.Q<Label>("shields-text");
            compass = root.Q<Compass>("compass");
            vignette = root.Q("vignette");
            
            SetupDeathUI();
        }

        private void Update()
        {
            compass.FaceAngle = avatar.transform.eulerAngles.y;

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
    }
}