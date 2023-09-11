using System.Collections.Generic;
using FR8Runtime.Shapes;
using UnityEngine;

namespace FR8Runtime.Player
{
    public class PlayerSafeZone : MonoBehaviour, IVitalityBooster
    {
        public static readonly List<PlayerSafeZone> All = new();

        private List<Shape> shapes = new();

        private void OnEnable()
        {
            All.Add(this);
            GetShapes();
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void GetShapes()
        {
            shapes.Clear();
            shapes.AddRange(GetComponentsInChildren<Shape>());
        }

        public bool CanUse(PlayerAvatar avatar)
        {
            var center = avatar.getCenter();
            foreach (var e in shapes)
            {
                if (e.ContainsPoint(center)) return true;
            }

            return false;
        }

        public void Bind(PlayerAvatar avatar) { }
        public void Unbind(PlayerAvatar avatar) { }
    }
}