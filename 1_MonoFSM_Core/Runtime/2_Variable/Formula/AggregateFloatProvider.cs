using System;
using System.Collections.Generic;
using System.Linq;
using MonoFSM.Core.Attributes;
using MonoFSM.Core.DataProvider;
using MonoFSM.Core.Variable;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Attributes;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonoFSM.Core.Formula
{
    //不同種valueProvider的聚合計算
    public class AggregateFloatProvider : MonoBehaviour, IFloatProvider
    {
        public enum AggregationType
        {
            Sum,
            Average,
            Min,
            Max,
            Count,
        }

        // [AutoChildren] [CompRef] [Required] [Tooltip("The component that provides the list of objects to process.")]
        // private IMonoDescriptableListProvider _inputProvider;

        // [ValueTypeValidate(typeof(List<MonoEntity>))] //var -> VarListEntity, value-> MonoEntity
        // // [Auto]
        // // [CompRef]
        // [Tooltip("The MonoEntity list provider to use for aggregation.")]
        // [SerializeField]
        // private ValueProvider _monoEntityListProvider; //FIXME: 應該改用VarList?

        public VarList<MonoEntity> _monoEntityList;

        //用VarListEntity?

        [SerializeField]
        [Required]
        [Tooltip("The variable tag to look for on each object to get the float value.")]
        [SOConfig("VariableType")]
        private VariableTag _variableToAggregate;

        [SerializeField]
        private AggregationType _operation = AggregationType.Sum;

        [ShowInPlayMode]
        public float Value => GetValue();

        public float GetValue()
        {
            // if (_monoEntityListProvider == null || _variableToAggregate == null)
            //     return 0f;
            // var values = _monoEntityListProvider
            //     .GetVar<VarListEntity>()
            //     .GetList()
            //     .Select(GetFloatFromDescriptable)
            //     .ToList();
            if (_monoEntityList == null)
            {
                Debug.LogError("MonoEntity list is not assigned.", this);
                return 0f;
            }

            if (_monoEntityList.Value == null)
                return 0f;
            //FIXME: 會GC
            var values = _monoEntityList.Value.Select(GetFloatFromDescriptable)
                .ToList();


            if (values.Count == 0)
                return 0f;

            switch (_operation)
            {
                case AggregationType.Sum:
                    return values.Sum();
                case AggregationType.Average:
                    return values.Average();
                case AggregationType.Min:
                    return values.Min();
                case AggregationType.Max:
                    return values.Max();
                case AggregationType.Count:
                    return values.Count;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetFloatFromDescriptable(MonoEntity entity)
        {
            // return 0;
            if (entity == null)
            {
                Debug.LogError("Entity is null, cannot get variable value.", this);
                return 0f;
            }

            var variable = entity.VariableFolder.GetVariable(_variableToAggregate);
            if (variable == null)
            {
                Debug.LogError(
                    $"Variable '{_variableToAggregate.name}' not found on '{entity.name}'.",
                    entity
                );
                return 0f;
            }

            // if (variable is IFloatProvider floatProvider)
            //     return floatProvider.Value;
            if (variable is VarFloat varFloat)
                return varFloat.Value;
            if (variable is VarInt varInt)
                return varInt.Value;


            // Fallback for variables that are not IFloatProvider but can be converted
            // if (variable.ValueType == typeof(float))
            //     return variable.Get<float>();
            //
            // if (variable.ValueType == typeof(int))
            //     return variable.Get<int>();

            Debug.LogWarning(
                $"Variable '{_variableToAggregate.name}' on '{entity.name}' is not a float provider or a convertible type.",
                entity
            );
            return 0f;
        }

        public string Description =>
            $"{_operation} of '{_variableToAggregate?.name}' from '{_monoEntityList?.name}'";
    }
}
