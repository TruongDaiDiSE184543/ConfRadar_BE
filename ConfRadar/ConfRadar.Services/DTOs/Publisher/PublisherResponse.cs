using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.DTOs.Publisher
{
    public class PublisherResponse
    {
        public string PublisherId { get; set; } = null!;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? LogoUrl { get; set; }

        public static PublisherResponse FromModel(Publisher publisher)
        {
            return new PublisherResponse
            {
                PublisherId = publisher.PublisherId,
                Name = publisher.Name,
                Description = publisher.Description,
                WebsiteUrl = publisher.WebsiteUrl,
                LogoUrl = publisher.LogoUrl
            };
        }
    }
    
    public static class PublisherExtensions
    {
        public static PublisherResponse FromModel(this Publisher publisher)
        {
            return PublisherResponse.FromModel(publisher);
        }
    }
}