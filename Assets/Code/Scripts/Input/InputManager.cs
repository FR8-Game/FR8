
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace FR8Runtime.Input
{
    public static class InputManager
    {
        private static List<ActionMap> registery = new();
        private static ActionMap active;

        public static void Register(InputActionAsset asset)
        {
            registery.Add(new ActionMap(asset, s =>
            {
                if (s) asset.Enable();
                else asset.Disable();
            }));
            asset.Disable();
        }

        public static void Deregister(object owner)
        {
            registery.RemoveAll(e => e.owner == owner);   
        }

        public static void SetActive(object newOwner)
        {
            var active = registery.Find(e => e.owner == newOwner);
            if (active == null) return;
            
            InputManager.active?.setEnabled(false);
            InputManager.active = active;
            InputManager.active.setEnabled(true);
        }
        
        public class ActionMap
        {
            public object owner;
            public Action<bool> setEnabled;

            public ActionMap(object owner, Action<bool> setEnabled)
            {
                this.owner = owner;
                this.setEnabled = setEnabled;
            }
        }
    }
}