using UnityEngine;

namespace FR8.Player.Submodules
{
    [System.Serializable]
    public class PlayerUrination
    {
        private PlayerAvatar avatar;
        private ParticleSystem fx;

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;
            
            fx = avatar.transform.Find("Pee").GetComponent<ParticleSystem>();

            avatar.UpdateEvent += Update;
        }

        private void Update()
        {
            SyncParticleState(avatar.input.Pee);
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