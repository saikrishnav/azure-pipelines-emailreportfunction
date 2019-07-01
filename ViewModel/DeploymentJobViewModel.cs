using EmailReportFunction.Config.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class DeploymentJobViewModel
    {
        [DataMember]
        public List<TaskResultViewModel> Tasks { get; set; }

        [DataMember]
        public DateTime? MinTaskStartTime { get; set; }

        [DataMember]
        public DateTime? MaxTaskFinishTime { get; set; }

        public DeploymentJobViewModel()
        {

        }

        public DeploymentJobViewModel(IList<JobData> jobs)
        {
            Tasks = new List<TaskResultViewModel>();

            if (jobs.Count > 0)
            {
                int taskIndex = 0;
                List<TaskData> releaseTasks;
                do
                {
                    releaseTasks = new List<TaskData>();
                    foreach (var job in jobs)
                    {
                        // Not all jobs have same set of tasks
                        if (taskIndex < job.Tasks.Count)
                        {
                            releaseTasks.Add(job.Tasks[taskIndex]);
                            MinTaskStartTime = GetMinTime(MinTaskStartTime, job.Tasks[taskIndex].StartTime);
                            MaxTaskFinishTime = GetMaxTime(MaxTaskFinishTime, job.Tasks[taskIndex].FinishTime);
                        }
                    }

                    if (releaseTasks.Any())
                    {
                        Tasks.Add(new TaskResultViewModel(releaseTasks));
                    }

                    taskIndex++;

                } while (releaseTasks.Any());
            }
        }

        private DateTime? GetMinTime(DateTime? time1, DateTime? time2)
        {
            if (time1 == null)
            {
                return time2;
            }
            else if (time2.HasValue && time2 < time1)
            {
                return time2;
            }

            return time1;
        }

        private DateTime? GetMaxTime(DateTime? time1, DateTime? time2)
        {
            if (time1 == null)
            {
                return time2;
            }
            else if (time2.HasValue && time2 > time1)
            {
                return time2;
            }

            return time1;
        }
    }
}
