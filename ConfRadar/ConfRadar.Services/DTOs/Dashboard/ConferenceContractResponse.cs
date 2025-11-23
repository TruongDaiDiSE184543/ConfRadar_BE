using ConfRadar.Services.DTOs.Conference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.DTOs.Dashboard
{
    public class ConferenceContractResponse
    {
       
        public ConferenceResponseDTO ConferenceResponse { get; set; }
        
        public int? Commission { get; set; }      
        public string? ContractUrl { get; set; }  
        public string? TargetAudience { get; set; } 
    }
}
