using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.AuditLog
{
    public class AuditInternalConferenceCountDetailResponse
    {
        public int TotalInternal { get; set; }
        public int TotalActiveInternalResearch { get; set; }
        public int TotalActiveInternalTech { get; set; }
    }
    public class AuditExternalConferenceCountDetailResponse
    {
        public int TotalExternal { get; set; }
        public int TotalActiveExternalResearch { get; set; }
        public int TotalActiveExternalTech { get; set; }
    }
}
