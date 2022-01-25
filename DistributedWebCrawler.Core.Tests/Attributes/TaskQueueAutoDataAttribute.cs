using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Tests.Customizations;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class TaskQueueAutoDataAttribute : MoqAutoDataAttribute
    {
        public TaskQueueAutoDataAttribute(
            QueuedItemStatus resultStatus = QueuedItemStatus.Success,
            bool throwsException = false,
            int numberOfItemsToDequeue = 0,
            int cancelAfterMilliseconds = 1000)
            : base(new TaskQueueCustomization(
                resultStatus, 
                throwsException, 
                numberOfItemsToDequeue,
                cancelAfterMilliseconds), 
            configureMembers: true)
        {

        }
    }
}
