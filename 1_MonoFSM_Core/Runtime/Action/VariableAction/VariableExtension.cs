using System.Collections.Generic;
using MonoFSM.Runtime.Variable;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime.Action.VariableAction
{
    internal static class VariableExtension
    {
        public static IList<ValueDropdownItem<T>> GetVariableValueDropdownItems<T>(
            this MonoBehaviour self
        )
            where T : AbstractMonoVariable
        {
            var items = new List<ValueDropdownItem<T>>();
            var contexts = self.GetComponentsInParent<MonoBlackboard>(true);
            foreach (var context in contexts)
            {
                var vars = context.GetComponentsInChildren<T>(true);
                foreach (var var in vars)
                {
                    var owner = var.GetComponentInParent<MonoBlackboard>();
                    items.Add(new ValueDropdownItem<T>(owner.name + "/" + var.name, var));
                }
            }

            return items;
        }

        public static IList<ValueDropdownItem<VariableTag>> GetVariableTagDropdownItems<T>(
            this MonoBehaviour self
        )
            where T : AbstractMonoVariable
        {
            //FIXME: application play很討厭？
            var items = new List<ValueDropdownItem<VariableTag>>();
            var contexts = self.GetComponentsInParent<MonoBlackboard>(true);
            foreach (var context in contexts)
            {
                var vars = context.GetComponentsInChildren<T>(true);
                foreach (var var in vars)
                {
                    var owner = var.GetComponentInParent<MonoBlackboard>();
                    if (owner == null)
                    {
                        Debug.LogError("owner==null", var);
                    }

                    // if (var._varTag == null)
                    // {
                    //     Debug.LogError("varTag==null", var);
                    // }

                    items.Add(
                        new ValueDropdownItem<VariableTag>(owner.name + "/" + var.name, var._varTag)
                    );
                }
            }

            return items;
        }
    }
}
