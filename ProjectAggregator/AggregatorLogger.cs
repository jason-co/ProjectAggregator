using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core;

namespace ProjectAggregator
{
    public class AggregatorLogger : ViewModelBase, ILogger
    {
        private readonly ObservableCollection<string> _logs;

        public AggregatorLogger()
        {
            _logs = new ObservableCollection<string>();
        }

        public IReadOnlyList<string> Logs { get { return new ReadOnlyObservableCollection<string>(_logs); } }

        public void Log(string format, params object[] args)
        {
            BeginUpdateUI(() => _logs.Add(String.Format(format, args)));
        }
    }
}
