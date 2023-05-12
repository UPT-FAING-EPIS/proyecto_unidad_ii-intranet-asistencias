using RabbitMQToElasticsearch.Models;

namespace RabbitMQToElasticsearch.Services{
    public interface ILogService
    {
        void ConsumeQueue();
    }
}
