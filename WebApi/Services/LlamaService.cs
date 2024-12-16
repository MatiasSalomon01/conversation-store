using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using WebApi.Constants;

namespace WebApi.Services;

public interface ILlamaService
{
    Task SendAIAnswer(string body, Conversation anonymousCustomer);
}

public class LlamaResponse
{
    public string Response { get; set; }
}


public class LlamaService(IHttpClientFactory httpClientFactory, ISendEndpointProvider endpointProvider) : ILlamaService
{
    public async Task SendAIAnswer(string body, Conversation anonymousCustomer)
    {
        var httpClient = httpClientFactory.CreateClient(Constant.LlamaAI);

        var obj = new
        {
            model = "llama3.1",
            prompt = body,
            stream = false
        };

        var stringContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(string.Empty, stringContent);

        var content = await response.Content.ReadAsStringAsync();

        var responseContent = JsonConvert.DeserializeObject<LlamaResponse>(content);

        var random = new Random();
        var asd =  random.Next(2) == 0;

        var aiCount = anonymousCustomer.logs.Where(x => x.ticket_id == null).OfType<AIAgentTextLog>().Count();

        var command = new AIAgentAnswerGenerated
        {
            uuid = Guid.NewGuid(),
            timestamp = DateTime.Now,
            ticket_id = anonymousCustomer.current_ticket_id,
            conversation_id = anonymousCustomer.id,
            body = responseContent?.Response ?? "No hubo respuesta de la AI",
            open_ticket = anonymousCustomer.current_ticket_id is null ?  aiCount > 1 : false
        };

        await endpointProvider.Send(nameof(AIAgentAnswerGenerated), command);
    }
}
