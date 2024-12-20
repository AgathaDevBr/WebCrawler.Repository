using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Entities
{
    public class ExecutionInfo
    {
        public Guid? Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalPages { get; set; }
        public int TotalProxies { get; set; }
        public string JsonFilePath { get; set; }
    }
}
