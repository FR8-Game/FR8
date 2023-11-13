using FR8.Runtime.Save;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

namespace FR8.Runtime.Player.Submodules
{
    [System.Serializable]
    public class PlayerUrination
    {
        public Gradient pissColor;
        
        private PlayerAvatar avatar;
        private ParticleSystem fx;
        
        private bool pissing;

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;
            
            fx = avatar.transform.Find("Pee").GetComponent<ParticleSystem>();

            avatar.UpdateEvent += Update;
        }

        private void Update()
        {
            var settings = SaveManager.SettingsSave.GetOrLoad();

            if (settings.togglePiss)
            {
                if (avatar.input.peeAction.action.WasPressedThisFrame()) pissing = !pissing;
            }
            else
            {
                pissing = avatar.input.Pee;
            }

            var normalizedHealth = avatar.vitality.CurrentHealth / (float)avatar.vitality.maxHealth;
            var main = fx.main;
            main.startColor = pissColor.Evaluate(normalizedHealth);
            
            SyncParticleState(pissing);
            AimParticles();
        }

        public void SyncParticleState(bool state)
        {
            if (state && !fx.isEmitting)
            {
                fx.Play();
            }

            if (!state && fx.isEmitting)
            {
                fx.Stop();
            }
        }

        public void AimParticles()
        {
            var yaw = avatar.cameraController.Yaw;
            fx.transform.localRotation = Quaternion.Euler(-yaw * 0.5f, 0.0f, 0.0f);
        }
    }
}