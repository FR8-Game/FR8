using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8.Utility
{
    public static class Cursor
    {
        private static readonly List<CursorLockMode> LockStack = new();

        public static void Push(CursorLockMode element, ref int id)
        {
            LockStack.Add(element);
            UpdateLockState();
            id = LockStack.Count;
        }
        
        public static void Pop(ref int id)
        {
            if (id <= 0) return;
            
            LockStack.RemoveAt(id - 1);
            UpdateLockState();
            id = 0;
        }

        public static void Change(int id, CursorLockMode mode)
        {
            LockStack[id - 1] = mode;
            UpdateLockState();
        }

        public static void UpdateLockState()
        {
            UnityEngine.Cursor.lockState = LockStack.Count > 0 ? LockStack[^1] : CursorLockMode.None;
        }
    }
}