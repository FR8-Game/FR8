using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using FR8Runtime.CodeUtility;
using FR8Runtime.References;
using FR8Runtime.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerVitality
    {
        public int maxHealth = 100;
        public float shieldDuration = 100;
        public float shieldRegenRate = 3.0f;

        [Space]
        public DamageInstance exposureDamageInstance = new(25);

        [Space]
        [SerializeField] private EventReference shieldBreakSound;
        [SerializeField] private EventReference shieldRegenSound;
        [SerializeField] private EventReference damageSound;

        public float exposureDamageFrequency = 4.0f;

        private int currentHealth;
        private float currentShields;

        private IVitalityBooster vitalityBooster;
        private float shieldRegenBuffer;

        private EventInstance shield;

        public int CurrentHealth => currentHealth;
        public float CurrentShields => currentShields;
        public bool IsAlive { get; private set; }
        public float LastDamageTime { get; private set; }
        public bool Exposed { get; private set; }

        private float exposureTimer;

        private PlayerAvatar avatar;

        public event Action<DamageInstance> DamageEvent;
        public event Action<int> RegenerateEvent;
        public event Action ReviveEvent;
        public event Action HealthChangeEvent;
        public event Action IsAliveChangedEvent;

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;

            avatar.FixedUpdateEvent += FixedUpdate;

            LastDamageTime = float.MinValue;
            Revive();
            
            shield = SoundReference.PlayerShields.InstanceAndStart();
        }

        public void FixedUpdate()
        {
            GetExposed();

            var exposed = !(Object)vitalityBooster;
            shield.set3DAttributes(avatar.gameObject.To3DAttributes());
            shield.setParameterByName("ShieldPercent", currentShields / shieldDuration);
            shield.setParameterByName("Exposed", exposed ? 1 : 0);
            
            Exposed = exposed;
            if (Exposed)
            {
                if (currentShields >= 0.0f)
                {
                    currentShields -= Time.deltaTime;
                    HealthChangeEvent?.Invoke();
                }
                else
                {
                    currentShields = -1.0f;
                    
                    exposureTimer += Time.deltaTime;
                    var exposureTime = 1.0f / exposureDamageFrequency;
                    while (exposureTimer > exposureTime)
                    {
                        Damage(exposureDamageInstance);
                        exposureTimer -= exposureTime;
                    }
                }
            }
            else
            {
                shieldRegenBuffer += shieldRegenRate * Time.deltaTime;
                var shieldRegen = Mathf.FloorToInt(shieldRegenBuffer);
                shieldRegenBuffer -= shieldRegen;
                RegenerateShields();
            }
        }

        private void GetExposed()
        {
            if (checkType(PlayerSafeZone.All)) return;

            if ((Object)vitalityBooster)
            {
                vitalityBooster.Unbind(avatar);
                vitalityBooster = null;
            }

            bool checkType<T>(IEnumerable<T> list) where T : IVitalityBooster
            {
                foreach (var e in list)
                {
                    if (!e.CanUse(avatar)) continue;
                    if ((IVitalityBooster)e == vitalityBooster) return true;

                    if ((Object)vitalityBooster)
                    {
                        vitalityBooster.Unbind(avatar);
                    }

                    vitalityBooster = e;
                    vitalityBooster.Bind(avatar);
                    return true;
                }

                return false;
            }
        }

        public void Damage(DamageInstance damageInstance)
        {
            if (!IsAlive) return;

            var damage = CalculateDamage(damageInstance);
            LastDamageTime = Time.time;

            currentHealth -= damage;

            if (currentShields <= 0) currentShields = -1.0f;
            if (currentHealth < 0) currentHealth = 0;

            DamageEvent?.Invoke(damageInstance);
            HealthChangeEvent?.Invoke();
            
            SoundReference.PlayerDamage.PlayOneShot(avatar.gameObject);

            if (currentHealth == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!IsAlive) return;

            IsAlive = false;
            Cursor.lockState = CursorLockMode.None;
            IsAliveChangedEvent?.Invoke();

            Pause.SetPaused(true);
        }

        private int CalculateDamage(DamageInstance damageInstance)
        {
            return Mathf.FloorToInt(damageInstance.amount);
        }

        public void RegenerateShields()
        {
            currentShields += (shieldDuration * 1.1f - currentShields) * shieldRegenRate * Time.deltaTime;
            currentShields = Mathf.Min(currentShields, shieldDuration);
            HealthChangeEvent?.Invoke();
        }

        public void RegenerateHealth(float fAmount)
        {
            if (!IsAlive) return;

            var amount = Mathf.FloorToInt(fAmount);

            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            RegenerateEvent?.Invoke(amount);
            HealthChangeEvent?.Invoke();
        }

        public void Revive()
        {
            if (IsAlive) return;

            currentHealth = maxHealth;
            currentShields = shieldDuration;
            IsAlive = true;
            LastDamageTime = float.MinValue;

            ReviveEvent?.Invoke();
            HealthChangeEvent?.Invoke();
            IsAliveChangedEvent?.Invoke();

            Pause.SetPaused(false);
        }

        [Serializable]
        public class DamageInstance
        {
            public float amount;

            public DamageInstance() { }

            public DamageInstance(int amount)
            {
                this.amount = amount;
            }
        }

        public void SetShields(float percent)
        {
            currentShields = percent * shieldDuration;
            HealthChangeEvent?.Invoke();
        }

        public void SetHealth(int health)
        {
            currentHealth = health;
            HealthChangeEvent?.Invoke();
        }
    }
}