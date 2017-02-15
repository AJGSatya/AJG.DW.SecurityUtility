using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJG.DW.SecurityUtility.Web.Models
{
    public class BranchDWAccess
    {
        public int Id { get; set; }
        public string RegionName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public bool HasAccess { get; set; }
    }
}
