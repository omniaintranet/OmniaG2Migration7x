using Omnia.Migration.Core.Helpers;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.App.Helpers
{
    public class ProgressManager : IProgressManager
    {
        ProgressBar _progressBar;
        Progress<int> _progress;
        string _initialMsg;
        string _tickMsg;

        public ProgressManager(string initialMsg, string tickMsg)
        {
            _initialMsg = initialMsg;
            _tickMsg = tickMsg;
        }

        public void Dispose()
        {
            if (_progressBar != null)
                _progressBar.Dispose();            
        }

        public void ReportProgress(int tickCount)
        {
            (_progress as IProgress<int>).Report(tickCount);
        }

        public void Start(int maxTicks)
        {
            _progressBar = new ProgressBar(maxTicks, _initialMsg, Console.ForegroundColor);
            _progress = new Progress<int>();
            _progress.ProgressChanged += (sender, value) => {
                _progressBar.Tick($"{_tickMsg} {_progressBar.CurrentTick + 1}/{_progressBar.MaxTicks}");
            };
        }
    }
}
