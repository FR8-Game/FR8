using System.Collections.Generic;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.BuildMage
{
    [CreateAssetMenu(menuName = "Tools/Build Mage")]
    public sealed class BuildMage : ScriptableObject
    {
        [SerializeField] [TextArea] private string discordMessageContent;
        [SerializeField] private bool rebuild;
        [SerializeField] private bool pushToItch;
        [SerializeField] private bool notifyDiscord;

        private static HttpClient client = new();
        
        public void Build()
        {
            if (rebuild)
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/Windows/FR8.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
            }

            if (pushToItch)
            {
                var butlerCommand = $"C:/Butler/butler.exe push --ignore *DoNotShip* \"Builds/Windows\" boschingmachine/FR8:win --userversion {Application.version}";
                System.Diagnostics.Process.Start("cmd.exe", $"/C {butlerCommand}");
            }

            if (notifyDiscord)
            {
                var values = new Dictionary<string, string>()
                {
                    { "content", $"<@1123458688489885748>\n## New Build has been published on itch!\n{discordMessageContent}\n- [Itch.io Page](<https://boschingmachine.itch.io/fr8>)"},
                };

                var content = new FormUrlEncodedContent(values);
                var response = client.PostAsync("https://discord.com/api/webhooks/1133017187100131480/xZ996IkjVH3n6mAhwkuhVT9gE0uWDSl8-THgjRnMMjyNeTrdrPCvkNygWv2Y9rUq14oE", content);
                Debug.Log(response.Result.Content.ToString());
            }
        }
    }
}