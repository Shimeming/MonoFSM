using System.Collections.Generic;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Utilities
{
    public static class LayerMaskUtility
    {
        /// <summary>
        /// Returns a string array of layer names from a LayerMask.
        /// </summary>
        public static string[] MaskToNames(this LayerMask original)
        {
            var output = new List<string>();

            for (int i = 0; i < 32; ++i)
            {
                int shifted = 1 << i;
                if ((original & shifted) == shifted)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        output.Add(layerName);
                    }
                }
            }

            return output.ToArray();
        }
    }
}
