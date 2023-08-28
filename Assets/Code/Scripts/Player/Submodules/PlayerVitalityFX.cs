using System;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UI;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerVitalityFX
    {
        [SerializeField] private Color healthColor = Color.red;
        [SerializeField] private Color shieldColor = Color.cyan;

        [Space]
        [SerializeField] private float fadeTime = 0.4f;

        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

        [Space]
        [Range(0.0f, 1.0f)] [SerializeField] private float vignetteMaxAlpha = 0.3f;

        private GameObject deathMenu;
        private Image vignette;
        private Image healthBar;
        private Image shieldBar;
        private PlayerAvatar avatar;

        private const string RootPath = "UI/Vitality";
        private const string CoverPath = RootPath + "/Death";
        private const string VignettePath = RootPath + "/Vignette";
        private const string HealthBarPath = RootPath + "/Stats/Health";
        private const string ShieldBarPath = RootPath + "/Stats/Shields";

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            avatar.UpdateEvent += Update;
            avatar.vitality.HealthChangeEvent += UpdateBars;
            avatar.vitality.IsAliveChangedEvent += ShowDeathUI;
            
            deathMenu = FindUtility.Find(avatar.transform, CoverPath).gameObject;
            vignette = FindUtility.Find<Image>(avatar.transform, VignettePath);
            healthBar = FindUtility.Find<Image>(avatar.transform, $"{HealthBarPath}/Fill");
            shieldBar = FindUtility.Find<Image>(avatar.transform, $"{ShieldBarPath}/Fill");

            SetupDeathUI();

            OnValidate(avatar);
            UpdateBars();
        }

        private void SetupDeathUI()
        {
            var button = FindUtility.Find<Button>(deathMenu.transform, "Button");
            UIUtility.MakeButtonList(button,
                ("Reconstitute", avatar.vitality.Revive)
            );
            
            ShowDeathUI();
        }

        private void UpdateBars()
        {
            var v = avatar.vitality;

            healthBar.fillAmount = Mathf.Clamp01((float)v.CurrentHealth / v.maxHealth);
            shieldBar.fillAmount = Mathf.Clamp01((float)v.CurrentShields / v.shieldDuration);
        }

        private void ShowDeathUI()
        {
            deathMenu.SetActive(!avatar.IsAlive);
        }

        public void OnValidate(PlayerAvatar avatar)
        {
            const float bgDarkness = 0.2f;

            var h = FindUtility.Find<Image>(avatar.transform, HealthBarPath);
            var hf = FindUtility.Find<Image>(avatar.transform, $"{HealthBarPath}/Fill");

            if (h) h.color = new Color(healthColor.r * bgDarkness, healthColor.g * bgDarkness, healthColor.b * bgDarkness, healthColor.a);
            if (hf) hf.color = healthColor;

            var s = FindUtility.Find<Image>(avatar.transform, ShieldBarPath);
            var sf = FindUtility.Find<Image>(avatar.transform, $"{ShieldBarPath}/Fill");

            if (s) s.color = new Color(shieldColor.r * bgDarkness, shieldColor.g * bgDarkness, shieldColor.b * bgDarkness, shieldColor.a);
            if (sf) sf.color = shieldColor;
        }

        private void Update()
        {
            var t = Mathf.Clamp01((Time.time - avatar.vitality.LastDamageTime) / fadeTime);
            var alpha = fadeCurve.Evaluate(t);

            if (vignette)
            {
                vignette.color = new Color(healthColor.r, healthColor.g, healthColor.b, alpha * vignetteMaxAlpha);
            }
        }
    }
}