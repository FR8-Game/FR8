using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FR8Editor.BuildMage
{
    [CreateAssetMenu(menuName = "Tools/Build Mage")]
    public sealed class BuildMage : ScriptableObject
    {
        [SerializeField] [TextArea] private string discordMessageContent;
        [SerializeField] public bool rebuild;
        [SerializeField] public bool pushToItch;
        [SerializeField] public bool notifyDiscord;

        private static HttpClient client = new();
        
        public void Build()
        {
            if (!EditorUtility.DisplayDialog("BuildMage", "Are you sure you want to perform this action?", "Yes", "Cancel")) return;

            if (rebuild)
            {
                var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/Windows/FR8.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

                Action<string> log;
                var cancel = false;
                switch (report.summary.result)
                {
                    case BuildResult.Unknown:
                        log = Debug.LogWarning;
                        cancel = true;
                        break;
                    case BuildResult.Succeeded:
                        log = Debug.Log;
                        break;
                    case BuildResult.Failed:
                        log = Debug.LogError;
                        cancel = true;
                        break;
                    case BuildResult.Cancelled:
                        log = Debug.Log;
                        cancel = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                log($"Build finished with result {report.summary.result}");
                if (cancel) return;
            }

            if (pushToItch)
            { 
                var butlerCommand = $"C:/Butler/butler.exe push --ignore *DoNotShip* \"Builds/Windows\" boschingmachine/FR8:win --userversion {Application.version}";
                System.Diagnostics.Process.Start("cmd.exe", $"/K {butlerCommand}");
            }

            if (notifyDiscord)
            {
                var values = new Dictionary<string, string>()
                {
                    { "content", $"<@&1123458688489885748>\n## New Build has been published on itch!\n- [Itch.io Page](<https://boschingmachine.itch.io/fr8>)\n{discordMessageContent}"},
                };

                //client.DeleteAsync("https://discordapp.com/api/webhooks/1133172706897047613/_niHeSH-_Lpd_xi8sjxpDp4avgBK0itxXFhSkAQXOeAETeCfhT96rd7Iz3Zg2Sq65w0F/messages/1133177381281071244");

                var content = new FormUrlEncodedContent(values);
                var response = client.PostAsync("https://discordapp.com/api/webhooks/1133172706897047613/_niHeSH-_Lpd_xi8sjxpDp4avgBK0itxXFhSkAQXOeAETeCfhT96rd7Iz3Zg2Sq65w0F", content);
                Debug.Log(response.Result.Content.ToString());
            }

            discordMessageContent = string.Empty;
            rebuild = false;
            pushToItch = false;
            notifyDiscord = false;
        }
    }
}