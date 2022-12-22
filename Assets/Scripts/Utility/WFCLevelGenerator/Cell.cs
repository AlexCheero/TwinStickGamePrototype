using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public class Cell
    {
        private enum ECollapseState
        {
            NotCollapsed,
            Collapsed,
            CollapsedManually
        }
        
        public readonly List<ProbableEntry> ProbableEntries;
        
        private ECollapseState _collapseState;
        
#if DEBUG
        public bool FallbackUsed;
#endif

        public PatternEntry Entry => ProbableEntries.Count > 0 ? ProbableEntries[0].Entry : new PatternEntry();

        public bool IsCollapsed => _collapseState == ECollapseState.Collapsed ||
                                   _collapseState == ECollapseState.CollapsedManually;

        public bool IsCollapsedManually => _collapseState == ECollapseState.CollapsedManually;
        
        public Cell(IEnumerable<PatternEntry> entries)
        {
            ProbableEntries = new List<ProbableEntry>();
            foreach (var entry in entries)
            {
                if (TileAnalyzer.EntryComparer.Equals(entry, PatternEntry.PseudoEntry))
                    continue;
                ProbableEntries.Add(new ProbableEntry(entry, 1));
            }
            _collapseState = ECollapseState.NotCollapsed;
        }

        public bool TryCollapse(bool useRandom)
        {
            if (ProbableEntries.Count == 0)
            {
                _collapseState = ECollapseState.NotCollapsed;
                return false;
            }
            
            var chance = 0.0f;
            ProbableEntry selectedEntry = default;
            foreach (var probableEntry in ProbableEntries)
            {
                var newChance = Random.value * probableEntry.Weight;
                if (newChance > chance || Mathf.Abs(chance - newChance) < float.Epsilon && Random.value > 0.5f)
                {
                    chance = Mathf.Max(newChance, chance);
                    selectedEntry = probableEntry;
                }
            }

            ProbableEntries.Clear();
            ProbableEntries.Add(selectedEntry);
            _collapseState = ECollapseState.Collapsed;
            return true;
        }
        
        public bool TryCollapseWithGlobalWeights(bool useRandom, Dictionary<int, float> globalWeights)
        {
            if (ProbableEntries.Count == 0)
            {
                _collapseState = ECollapseState.NotCollapsed;
                return false;
            }
            
            var chance = 0.0f;
            ProbableEntry selectedEntry = default;
            foreach (var probableEntry in ProbableEntries)
            {
                var newChance = Random.value * globalWeights[probableEntry.Entry.Id];
                if (newChance > chance || Mathf.Abs(chance - newChance) < float.Epsilon && Random.value > 0.5f)
                {
                    chance = Mathf.Max(newChance, chance);
                    selectedEntry = probableEntry;
                }
            }

            ProbableEntries.Clear();
            ProbableEntries.Add(selectedEntry);
            _collapseState = ECollapseState.Collapsed;
            return true;
        }

        public void CollapseManually(int id, float rotation)
        {
            ProbableEntries.Clear();
            ProbableEntries.Add(new ProbableEntry(new PatternEntry { Id = id, YRotation = rotation }, 1));
            _collapseState = ECollapseState.CollapsedManually;
        }

        //public int Entropy => ProbableEntries.Count;
        
        //TODO: recheck the Shannon entropy formula
        public float GetEntropy()
        {
            var totalWeight = ProbableEntries.Sum(entry => entry.Weight);
            var entropy = 0.0f;
            for (int i = 0; i < ProbableEntries.Count; i++)
            {
                var weight = ProbableEntries[i].Weight;
                if (weight > 0)
                {
                    var p = weight / totalWeight;
                    entropy -= p * Mathf.Log(p) / Mathf.Log(2);
                }
            }
                
            return entropy;
        }
        
        public float GetGlobalEntropy(Dictionary<int, float> globalWeights)
        {
            var totalWeight = globalWeights.Values.Sum();
            var entropy = 0.0f;
            for (int i = 0; i < ProbableEntries.Count; i++)
            {
                var weight = globalWeights[ProbableEntries[i].Entry.Id];
                if (weight > 0)
                {
                    var p = weight / totalWeight;
                    entropy -= p * Mathf.Log(p) / Mathf.Log(2);
                }
            }
                
            return entropy;
        }
    }
}