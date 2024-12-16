namespace WebApi.Extensions;

public static class ISendEndpointProviderExtensions
{
    public static async Task Send(this ISendEndpointProvider endpointProvider, string queueName, object payload)
    {
        var uri = new Uri($"queue:{queueName}");

        var endpoint = await endpointProvider.GetSendEndpoint(uri);
        await endpoint.Send(payload);
    }
}
