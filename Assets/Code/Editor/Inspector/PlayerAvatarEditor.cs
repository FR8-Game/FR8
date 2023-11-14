using System;
using FR8.Runtime.Player;
using FR8.Runtime.Player.Submodules;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(PlayerAvatar))]
    public class PlayerAvatarEditor : Editor
    {
        private PlayerAvatar Player => target as PlayerAvatar;

        private const float Margin = 0.0f;
        private const float EdgePadding = 15.0f;
        private const float ElementPadding = 10.0f;
        private const float Indent = 18.0f;

        private int damage;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var health = (float)Player.vitality.CurrentHealth / Player.vitality.maxHealth;

            Box("Info", true,
                r => EditorGUI.Slider(r, "Current Health", health, 0.0f, 1.0f)
            );

            if (Application.isPlaying)
            {
                damage = EditorGUILayout.IntField("Damage", damage);
                if (GUILayout.Button("Damage Player"))
                {
                    ((PlayerAvatar)target).vitality.Damage(new PlayerVitality.DamageInstance(damage));
                }
            }
        }

        public void Box(string title, bool enabled, params Action<Rect>[] callbacks)
        {
            if (callbacks.Length == 0) return;

            var prefKey = $"{GetType().FullName}.{title}";
            var foldoutState = EditorPrefs.GetBool(prefKey, false);

            var lines = foldoutState ? callbacks.Length + 1 : 1;
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * lines + ElementPadding * (lines - 1) + (EdgePadding + Margin) * 2.0f);

            rect.min += Vector2.one * Margin;
            rect.max -= Vector2.one * Margin;

            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.1f));

            rect.min += Vector2.one * EdgePadding;
            rect.max -= Vector2.one * EdgePadding;

            rect.height = EditorGUIUtility.singleLineHeight;

            rect.xMin += 15;
            foldoutState = EditorGUI.Foldout(rect, foldoutState, title, true);
            rect.xMin -= 15;

            EditorPrefs.SetBool(prefKey, foldoutState);

            if (!foldoutState) return;

            next();
            rect.xMin += Indent;

            using (new EditorGUI.DisabledScope(enabled))
            {
                for (var i = 0; i < callbacks.Length; i++)
                {
                    callbacks[i](rect);
                    next();
                }
            }

            void next()
            {
                rect.y += rect.height + ElementPadding;
            }
        }
    }
}