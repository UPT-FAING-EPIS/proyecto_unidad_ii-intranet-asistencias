using RabbitMQToElasticsearch.Models;

namespace RabbitMQToElasticsearch.ElasticSearch
{
    public interface IElasticSearchService
    {
        void IndexDocument(LogEntry logEntry);
    }
}
