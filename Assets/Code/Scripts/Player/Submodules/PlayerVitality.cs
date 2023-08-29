using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FR8Runtime.UI;
using UnityEngine;
using UnityEngine.Rendering;
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

        public float exposureDamageFrequency = 4.0f;

        private List<PlayerSpawnPoint> spawnPointStack = new();
        private int currentHealth;
        private float currentShields;

        private IVitalityBooster vitalityBooster;
        private float shieldRegenBuffer;

        public int CurrentHealth => currentHealth;
        public float CurrentShields => currentShields;
        public bool IsAlive { get; private set; }
        public float LastDamageTime { get; private set; }
        public bool Exposed => !(Object)vitalityBooster;

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
        }

        public void FixedUpdate()
        {
            GetExposed();

            if (Exposed)
            {
                if (currentShields >= 0.0f)
                {
                    currentShields -= Time.deltaTime;
                    HealthChangeEvent?.Invoke();
                }
                else
                {
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
            if (checkType(PlayerTetherPoint.All.OrderBy(e => (e.transform.position - avatar.Center).magnitude))) return;

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

            if (currentShields < 0) currentShields = 0;
            if (currentHealth < 0) currentHealth = 0;

            DamageEvent?.Invoke(damageInstance);
            HealthChangeEvent?.Invoke();

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

            var (spawnPosition, spawnOrientation) = GetSpawnPoint();
            var rb = avatar.Rigidbody;

            rb.transform.position = spawnPosition;
            rb.transform.rotation = spawnOrientation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            ReviveEvent?.Invoke();
            HealthChangeEvent?.Invoke();
            IsAliveChangedEvent?.Invoke();

            Pause.SetPaused(false);
        }

        public (Vector3, Quaternion) GetSpawnPoint()
        {
            for (var i = spawnPointStack.Count; i > 0; i--)
            {
                var p = spawnPointStack[i - 1];
                if (p.enabled) return (p.Position, p.Orientation);
            }

            if (PlayerSpawnPoint.Default)
            {
                return (PlayerSpawnPoint.Default.Position, PlayerSpawnPoint.Default.Orientation);
            }

            return (Vector3.zero, Quaternion.identity);
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
    }
}