using System;
using FR8Runtime.CodeUtility;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Compass = FR8Runtime.UI.CustomControls.Compass;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerUI
    {
        private PlayerAvatar avatar;
        private UIDocument hud;

        private ProgressBar shieldsFill;
        private Label shieldsText;
        private Compass compass;

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
            
            SetupDeathUI();
        }

        private void Update()
        {
            compass.FaceAngle = avatar.transform.eulerAngles.y;
        }
        
        private void UpdateBars()
        {
            shieldsFill.value = avatar.vitality.CurrentShields / avatar.vitality.shieldDuration;
            shieldsText.text = $"<size=50%>SHIELDS</size>\n[ {(avatar.vitality.Exposed ? "ACTIVE" : " IDLE ")} | {Mathf.FloorToInt(avatar.vitality.CurrentShields),2:N0} s ]";
        }

        private void SetupDeathUI()
        {
            
            
            UpdateDeathUI();
        }

        private void UpdateDeathUI()
        {
            
        }
    }
}