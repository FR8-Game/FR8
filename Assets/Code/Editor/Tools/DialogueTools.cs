using FR8.Runtime.Dialogue;
using UnityEditor;

namespace FR8Editor.Tools
{
    public static class DialogueTools
    {
        private static DialogueChain testChainCache;
        
        private static void UpdateTestingChainCache()
        {
            if (testChainCache) return;

            var filename = "Assets/Content/Dialogue/DialogueTest.asset";

            testChainCache = AssetDatabase.LoadAssetAtPath<DialogueChain>(filename);
        }
        
        //[MenuItem("Actions/Testing/Dialogue Test")]
        public static void TestDialogue()
        {
            UpdateTestingChainCache();
            testChainCache.Queue();
        }
    }
}
