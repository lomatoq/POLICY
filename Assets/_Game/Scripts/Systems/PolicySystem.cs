using System;
using System.Collections.Generic;
using UnityEngine;
using Policy.Core;

namespace Policy.Systems
{
    [Serializable]
    public class PolicyEntry
    {
        public string text;
        public int    applicationCount;
        public float  createdAt; // Time.time
    }

    /// <summary>
    /// Stores active policies and ticks their application counts.
    /// </summary>
    public class PolicySystem : MonoBehaviour
    {
        public static PolicySystem Instance { get; private set; }

        public IReadOnlyList<PolicyEntry> Policies => _policies;
        private readonly List<PolicyEntry> _policies = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            GameEvents.OnPolicyAdded += AddPolicy;
            InvokeRepeating(nameof(TickPolicies), 4f, 4f);
        }

        private void OnDestroy()
        {
            GameEvents.OnPolicyAdded -= AddPolicy;
        }

        private void AddPolicy(string text)
        {
            _policies.Add(new PolicyEntry { text = text, createdAt = Time.time });
            GameEvents.StateChanged();
        }

        private void TickPolicies()
        {
            foreach (var p in _policies)
                p.applicationCount += UnityEngine.Random.Range(1, 5);
            GameEvents.StateChanged();
        }

        public int TotalApplications()
        {
            int t = 0;
            foreach (var p in _policies) t += p.applicationCount;
            return t;
        }
    }
}
