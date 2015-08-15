using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvDTE.Helpers
{
    public enum VisualStudioVersion
    {
        [Description("VisualStudio.DTE.12.0")]
        VisualStudio2013,
        [Description("VisualStudio.DTE.14.0")]
        VisualStudio2015
    }
}
