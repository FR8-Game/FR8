using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FR8Editor
{
    public class VDirtCleanup
    {
        [MenuItem("Actions/VDirtCleanup")]
        public static void AHHHHHHHHHHHHHHH()
        {
            var meshes = Object.FindObjectsOfType<Mesh>(true);
            var sb = new StringBuilder();

            sb.AppendLine($"--- Mesh Dump [{meshes.Length} Found] ---");
            for (var i = 0; i < meshes.Length; i++)
            {
                var e = meshes[i];
                var path = AssetDatabase.GetAssetPath(e);
                sb.AppendLine($"-{i:N3}  {e.name} | {path}");
            }
            sb.AppendLine("-----------------------------------------");

            File.WriteAllText(Path.Combine(Application.dataPath, "VDirtCleanup.log"), sb.ToString());
        }
    }
}