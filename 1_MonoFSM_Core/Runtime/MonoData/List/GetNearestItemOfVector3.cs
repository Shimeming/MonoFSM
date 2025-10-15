using MonoFSM.Foundation;
using MonoFSM.Runtime;
using MonoFSM.Runtime.Mono;
using MonoFSM.Variable;
using UnityEngine;

namespace MonoFSM.Core.Variable.Providers
{
    public class GetNearestItemOfVector3 : AbstractEntitySource
    {
        public VarVector3 _sourcePosition;
        public VarListEntity _varList;

        MonoEntity FindNearestEntity()
        {
            if (_varList == null || _sourcePosition == null)
                return null;
            var list = _varList.GetList();
            if (list == null || list.Count == 0)
                return null;
            MonoEntity nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 sourcePos = _sourcePosition.Value;
            foreach (var item in list)
            {
                if (item == null)
                    continue;
                float dist = Vector3.Distance(sourcePos, item.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = item;
                }
            }

            return nearest;
        }

        public override MonoEntity Value => FindNearestEntity();
        public override MonoEntityTag entityTag { get; }
    }
}
