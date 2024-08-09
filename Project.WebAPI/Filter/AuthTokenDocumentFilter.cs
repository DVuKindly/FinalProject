using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DemoAppWebAPI.Filter
{
    public class AuthTokenDocumentFilter : IDocumentFilter
    {
       
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = swaggerDoc.Tags.OrderBy(x =>
                x.Name).ToList();
        }
    }
}
