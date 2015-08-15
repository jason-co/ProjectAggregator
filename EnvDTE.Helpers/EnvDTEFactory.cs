using System;
using Core;

namespace EnvDTE.Helpers
{
    internal static class EnvDTEFactory
    {
        internal static DTE Create(VisualStudioVersion visualStudioVersion)
        {
            var vsProgID = visualStudioVersion.ToDescription();
            var type = Type.GetTypeFromProgID(vsProgID, true);
            var obj = Activator.CreateInstance(type, true);

            return obj as DTE;
        }
    }
}
