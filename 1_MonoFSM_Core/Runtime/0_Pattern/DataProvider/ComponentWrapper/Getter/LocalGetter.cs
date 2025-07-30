using MonoFSM.Core.DataProvider;
using UnityEngine;

namespace _1_MonoFSM_Core.Runtime._0_Pattern.DataProvider.ComponentWrapper.Getter
{
    //FIXME: 還需要多一個class嗎？ 可以自己命名？ bool 勾勾就好？
    public class LocalGetter : MonoBehaviour
    {
        [Auto] private ValueProvider _valueProvider;
    }
}