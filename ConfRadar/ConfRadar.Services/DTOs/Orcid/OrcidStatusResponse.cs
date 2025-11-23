using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Orcid
{
    public class OrcidStatusResponse
    {
        public bool IsLinked { get; set; }
        public string OrcidId { get; set; }
        public string UserName { get; set; }
        public List<string> GrantedScopes { get; set; }
    }
}
