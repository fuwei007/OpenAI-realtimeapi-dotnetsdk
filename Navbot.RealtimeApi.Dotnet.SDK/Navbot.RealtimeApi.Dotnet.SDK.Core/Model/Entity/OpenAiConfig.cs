using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;

internal class OpenAiConfig
{
    public bool UseAzure { get; set; }
    public string? AzureEndpoint { get; set; }
    public string? AzureDeployment { get; set; }

    public string OpenApiUrl { get; set; }
    public string OpenApiRtcUrl { get; set; }
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public string Voice { get; set; }
    public Dictionary<string, string> RequestHeaderOptions { get; set; }

    public OpenAiConfig(string apiKey, bool useAzure = false)
    {
        ApiKey = apiKey;
        UseAzure = true; //useAzure;

        if (UseAzure)
        {
            // Load Azure-specific values (for example, from environment variables)
            ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_EASTUS2_API_KEY")
                ?? throw new InvalidOperationException("AZURE_OPENAI_EASTUS2_API_KEY is not set.");
            AzureEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_EASTUS2_ENDPOINT") // https://eastus2openaistrawberry.openai.azure.com/
                ?? throw new InvalidOperationException("AZURE_OPENAI_EASTUS2_ENDPOINT is not set.");
            AzureDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EASTUS2_DEPLOYMENT")
                ?? throw new InvalidOperationException("AZURE_OPENAI_EASTUS2_DEPLOYMENT is not set.");

            // For WebSocket connection (OpenApiUrl), ensure the scheme is "wss://"
            string wsEndpoint = AzureEndpoint;
            if (wsEndpoint.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                wsEndpoint = "wss://" + wsEndpoint.Substring("https://".Length);
            }

            // we aim to build : wss://eastus2openaistrawberry.openai.azure.com/realtime?model=gpt-4o-realtime-preview
            OpenApiUrl = $"{wsEndpoint.TrimEnd('/')}/realtime";

            // For RTC, we still need to use the secure HTTP scheme.
            OpenApiRtcUrl = $"{AzureEndpoint.TrimEnd('/')}/openai/realtime?api-version=2024-10-01-preview&deployment={AzureDeployment}";
            // https://eastus2openaistrawberry.openai.azure.com/openai/realtime?api-version=2024-10-01-preview&deployment=gpt-4o-realtime-preview-2

            Model = "gpt-4o-realtime-preview-2024-10-01"; // adjust if necessary
                                                          // For Azure, authentication is usually done with the header "api-key"
            Voice = "ash";

            RequestHeaderOptions = new Dictionary<string, string>
                {
                    { "api-key", apiKey },
                    { "openai-beta", "realtime=v1" }
                };
        }
        else
        {
            OpenApiUrl = "wss://api.openai.com/v1/realtime";
            OpenApiRtcUrl = "https://api.openai.com/v1/realtime";
            Model = "gpt-4o-realtime-preview-2024-12-17";
            Voice = "ash";
            RequestHeaderOptions = new Dictionary<string, string>
                {
                    { "openai-beta", "realtime=v1" }
                };
        }
    }
}
