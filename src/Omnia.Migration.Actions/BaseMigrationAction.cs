using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Omnia.Migration.Actions
{
    public abstract class BaseMigrationAction
    {
        public abstract Task StartAsync(IProgressManager progressManager);
    }

    public abstract class ParallelizableMigrationAction : BaseMigrationAction
    {
        protected void RunInParallel<T>(List<T> inputData, int numberOfThreads, Func<List<T>, Task> delegateAction)
        {            
            if (numberOfThreads <= 0)
                numberOfThreads = 1;

            var partitionedInput = inputData.Split(numberOfThreads);
            var tasks = new List<Task>();
            for (int i = 0; i < partitionedInput.Count; i++)
            {
                tasks.Add(delegateAction(partitionedInput[i]));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
