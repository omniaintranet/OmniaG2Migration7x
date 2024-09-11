using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    //TODO More styling support should be added as time go on
    public interface BorderEnabledBlockSettings
    {
        int borderRadius
        {
            get;
            set;
        }

        int borderWidth
        {
            get;
            set;
        }

        int elevation
        {
            get;
            set;
        }
        string borderColor
        {
            get;
            set;
        }
    }
}
