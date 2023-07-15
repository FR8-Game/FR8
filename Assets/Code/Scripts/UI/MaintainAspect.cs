using System;
using UnityEngine;
using UnityEngine.UI;

namespace FR8.UI
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MaintainAspect : MonoBehaviour, ILayoutElement
    {
        [SerializeField] private RectTransform.Axis maintainAxis;
        [SerializeField] private int referenceWidth = 1;
        [SerializeField] private int referenceHeight = 1;
        
        // ReSharper disable once InconsistentNaming
        public new RectTransform transform => base.transform as RectTransform;

        public float minWidth => maintainAxis switch
        {
            RectTransform.Axis.Horizontal => ParentWidth,
            RectTransform.Axis.Vertical => ParentHeight * (Mathf.Max(referenceWidth, 1.0f) / Mathf.Max(referenceHeight, 1.0f)),
            _ => 0.0f,
        };
        public float minHeight => maintainAxis switch
        {
            RectTransform.Axis.Horizontal => ParentWidth * (Mathf.Max(referenceHeight, 1.0f) / Mathf.Max(referenceWidth, 1.0f)),
            RectTransform.Axis.Vertical => ParentHeight,
            _ => 0.0f,
        };

        public float ParentWidth => ((RectTransform)transform.parent).rect.width;
        public float ParentHeight => ((RectTransform)transform.parent).rect.height;
        
        public float preferredWidth => minWidth;
        public float preferredHeight => minHeight;
        
        public float flexibleWidth => 0.0f;
        public float flexibleHeight => 0.0f;
        
        public int layoutPriority => 1;

        private void SetTransform()
        {
            return;
            Func<Vector2, Vector2> remap = maintainAxis switch
            {
                RectTransform.Axis.Horizontal => v => v,
                RectTransform.Axis.Vertical => v => new Vector2(v.y, v.x),
                _ => throw new Exception(),
            };

            transform.anchorMin = remap(new Vector2(0.0f, 0.5f));
            transform.anchorMax = remap(new Vector2(1.0f, 0.5f));

            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = remap(Vector2.up) * new Vector2(minWidth, minHeight);
        }
        
        private void OnValidate()
        {
            SetTransform();
        }

        public void CalculateLayoutInputHorizontal()
        {
            SetTransform();
        }

        public void CalculateLayoutInputVertical()
        {
            SetTransform();
        }
    }
}
