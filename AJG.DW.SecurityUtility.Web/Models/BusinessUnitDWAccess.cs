using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJG.DW.SecurityUtility.Web.Models
{
    public class BusinessUnitDWAccess
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string BusinessUnitDescription { get; set; }
        public int BusinessUnitCode { get; set; }
        public bool HasAccess { get; set; }
    }
}
