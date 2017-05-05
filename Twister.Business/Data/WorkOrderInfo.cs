using Twister.Business.Database;

namespace Twister.Business.Data
{
    public class WorkOrderInfo
    {
        public WorkOrderInfo(string workId)
        {
            WorkId = workId;
        }

        public string WorkId { get; }
        public string PartNumber { get; set; }
        public string Revision { get; set; }

        /// <summary>
        ///     Load the part number and revision for a particular work order.
        /// </summary>
        public void Load()
        {
            ShopfloorWorkOrderDb.GetWorkOrderInfo(this);
        }
    }
}