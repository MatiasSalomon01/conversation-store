namespace WebApi.Extensions;

public static class IBusExtensions
{
    public static async Task<TResponse> GetResponse<TRequest, TResponse>(this IBus bus, string queueName, TRequest payload) 
        where TRequest : class
        where TResponse : class
    {
        var uri = new Uri($"queue:{queueName}");

        var client = bus.CreateRequestClient<TRequest>(uri);

        var response = await client.GetResponse<TResponse>(payload);

        return response.Message;
    }
}
