using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    public interface IProgressManager: IDisposable
    {
        void Start(int totalCount);

        void ReportProgress(int count);
    }
}
