using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class ModifierScriptableObject : CommandScriptableObject
    {
        public virtual bool TryMergeModifiers(ModifierScriptableObject inputModifier)
        {
            return false;
        }

        public virtual float GetCurrentProgress()
        {
            return 0;
        }
    }
}
