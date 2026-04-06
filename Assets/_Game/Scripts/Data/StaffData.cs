using System;
using UnityEngine;

namespace Policy.Data
{
    [Serializable]
    public class StaffData
    {
        public string role;
        public int    count;
        public int    hireCost;
        public bool   locked;
    }
}
