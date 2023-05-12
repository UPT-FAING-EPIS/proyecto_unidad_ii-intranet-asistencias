using RabbitMQ.Client.Events;
namespace RabbitMQToElasticsearch.RabbitMq
{
    public interface IRabbitMqService
    {
        EventingBasicConsumer? ConsumeQueue();
    }
}
