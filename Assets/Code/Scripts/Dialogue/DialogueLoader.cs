using System;
using System.Collections.Generic;
using UnityEngine;

namespace FR8.Dialogue
{
    public class DialogueLoader : MonoBehaviour
    {
        [SerializeField] private TextAsset targetFile;
        [SerializeField] private LoadMethod loadMethod = LoadMethod.CSV;
        [SerializeField] private string columnSeparator = ":";
        [SerializeField] private string rowSeparator = ";";
        
        public List<DialogueChain> LoadedChains { get; } = new();

        public void Reload()
        {
            foreach (var e in LoadedChains)
            {
                Destroy(e);
            }
            LoadedChains.Clear();

            switch (loadMethod)
            {
                case LoadMethod.CSV:
                    LoadCSV();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void LoadCSV()
        {
            var text = targetFile.text;

            var rows = text.Split(rowSeparator);
            var grid = new string[rows.Length][];
            for (var i = 0; i < rows.Length; i++)
            {
                grid[i] = rows[i].Split(columnSeparator);
            }
            
            LoadStringGrid(grid);
        }

        private void LoadStringGrid(string[][] grid)
        {
            
        }

        private enum LoadMethod
        {
            CSV,
        }
    }
}
