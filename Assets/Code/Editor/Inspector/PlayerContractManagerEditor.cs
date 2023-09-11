using FR8Runtime.Player;
using UnityEditor;
using UnityEngine.UIElements;

namespace FR8Editor.Inspector
{
    [CustomEditor(typeof(PlayerContractManager))]
    public class PlayerContractManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new IMGUIContainer(() => base.OnInspectorGUI()));

            var player = target as PlayerContractManager;
            var contract = player.ActiveContract;

            var helpBox = new HelpBox();
            root.Add(helpBox);
            helpBox.messageType = HelpBoxMessageType.Info;

            if (contract)
            {
                var str = $"Active Contract\n- {contract.name}\n- Predicates\n";
                foreach (var e in contract.predicates)
                {
                    str += $"  - {e.BuildText()} [{e.Progress*100.0f,3:N0}%]\n";
                }
                helpBox.text = str;
            }
            else
            {
                helpBox.text = "This player has no contracts active";
            }

            return root;
        }
    }
}