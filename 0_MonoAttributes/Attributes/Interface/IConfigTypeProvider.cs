using System;

namespace MonoFSM.CustomAttributes
{
    public interface IConfigTypeProvider
    {
        Type GetRestrictType();
    }
}
