
using System.Collections.Generic;
using FR8.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace FR8.Runtime.Player
{
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class FPSInputModule : StandaloneInputModule
    {
        private InputSystemUIInputModule defaultModule;

        protected override void Awake()
        {
            defaultModule = GetComponent<InputSystemUIInputModule>();
            Pause.PausedStateChangedEvent += OnPauseStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Pause.PausedStateChangedEvent -= OnPauseStateChanged;
        }

        private void OnPauseStateChanged()
        {
            enabled = !Pause.Paused;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            defaultModule.enabled = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            defaultModule.enabled = true;
        }

        protected override MouseState GetMousePointerEventData(int id)
        {
            var reticlePosition = new Vector2(Screen.width, Screen.height) * 0.5f;
            var mouseState = new MouseState();

            GetPointerData(kMouseLeftId, out var leftData, true);
            
            leftData.Reset();

            leftData.delta = reticlePosition  - leftData.position;
            leftData.position = reticlePosition ;
            leftData.scrollDelta = Mouse.current.scroll.ReadValue();
            leftData.button = PointerEventData.InputButton.Left;
            
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycastWorldSpace(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            GetPointerData(kMouseRightId, out var rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;
            
            GetPointerData(kMouseRightId, out var middleData, true);
            CopyFromTo(leftData, middleData);
            rightData.button = PointerEventData.InputButton.Middle;
            
            mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
            mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);
            
            return mouseState;
        }

        private static RaycastResult FindFirstRaycastWorldSpace(List<RaycastResult> candidates)
        {
            var candidatesCount = candidates.Count;
            for (var i = 0; i < candidatesCount; ++i)
            {
                if (candidates[i].gameObject == null) continue;
                
                var canvas = candidates[i].gameObject.GetComponentInParent<Canvas>();
                if (!canvas) continue;
                if (canvas.renderMode != RenderMode.WorldSpace) continue;

                return candidates[i];
            }
            return new RaycastResult();
        }
    }
}