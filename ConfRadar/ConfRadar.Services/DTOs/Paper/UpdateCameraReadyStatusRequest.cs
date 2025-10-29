using ConfRadar.Services.Common;
using System.ComponentModel.DataAnnotations;

namespace ConfRadar.Services.DTOs.Paper
{
    public class UpdateCameraReadyStatusRequest
    {
        [Required(ErrorMessage = "CameraReadyId is required")]
        public string CameraReadyId { get; set; }

        [Required(ErrorMessage = "GlobalStatus is required")]
        [EnumDataType(typeof(GlobalStatusEnum), ErrorMessage = "Global status is required")]
        public GlobalStatusEnum GlobalStatus { get; set; }
        [Required(ErrorMessage ="Cần có paperid của paper mà thuộc cameraready thuộc về")]
        public string Paperid { get; set; }
    }
}