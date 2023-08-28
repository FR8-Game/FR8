using System;
using UnityEngine;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class PlayerVitality
    {
        public int maxShields;
        public int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentShields;

        public int CurrentHealth => currentHealth;
        public int CurrentShields => currentShields;
        public bool Alive { get; private set; }

        public event Action<HealthType, DamageInstance> DamageEvent;
        public event Action<HealthType, int> RegenerateEvent;

        public void Init()
        {
            Alive = true;
            currentHealth = maxHealth;
            currentShields = maxShields;
        }
        
        public void Damage(DamageInstance damageInstance)
        {
            DamageEvent?.Invoke(currentShields > 0 ? HealthType.Shields : HealthType.Health, damageInstance);
                
            var damage = CalculateDamage(damageInstance);
            
            if (currentShields > 0)
            {
                currentShields -= damage;
            }
            else
            {
                currentHealth -= damage;
            }

            if (currentShields < 0) currentShields = 0;
            if (currentHealth < 0) currentHealth = 0;

            if (currentHealth == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Alive = false;
        }

        private int CalculateDamage(DamageInstance damageInstance)
        {
            return Mathf.FloorToInt(damageInstance.amount);
        }

        public void RegenerateShields(float fAmount)
        {
            currentHealth = Regenerate(fAmount, currentHealth, maxHealth, v => RegenerateEvent?.Invoke(HealthType.Shields, v));
        }

        public void RegenerateHealth(float fAmount)
        {
            currentHealth = Regenerate(fAmount, currentHealth, maxHealth, v => RegenerateEvent?.Invoke(HealthType.Health, v));
        }
        
        private static int Regenerate(float fAmount, int current, int max, Action<int> eventCallback)
        {
            var amount = Mathf.FloorToInt(fAmount);
            
            eventCallback?.Invoke(amount);
            
            current += amount;
            if (current > max) current = max;
            return current;
        }

        public enum HealthType
        {
            Health, Shields,
        }
        
        [Serializable]
        public class DamageInstance
        {
            public float amount;
        }
    }
}