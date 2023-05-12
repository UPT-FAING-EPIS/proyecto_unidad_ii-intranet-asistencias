using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Nest;
using System.Text.Json;
using RabbitMQToElasticsearch.Models;
namespace RabbitMQToElasticsearch.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticClient _elasticClient;
        private readonly ILogger<ElasticSearchService> _logger;

        public ElasticSearchService(IConfiguration configuration, ILogger<ElasticSearchService> logger)
        {
            _logger = logger;
            _elasticClient = CreateElasticClient(configuration);
        }

        public void IndexDocument(LogEntry logEntry)
        {
            var indexResponse = _elasticClient.IndexDocument(logEntry);

            if (!indexResponse.IsValid)
            {
                _logger.LogError("Failed to index document: {message}", indexResponse.OriginalException.Message);
            }
        }

        private ElasticClient CreateElasticClient(IConfiguration configuration)
        {
            var elasticUrl = configuration.GetSection("Elasticsearch:Url").Value;
            if (string.IsNullOrEmpty(elasticUrl))
            {
                throw new ArgumentException("Elasticsearch URL is not configured.");
            }

            var settings = new ConnectionSettings(new Uri(elasticUrl)).DefaultIndex("logs");
            var elasticClient = new ElasticClient(settings);
            ValidateElasticClientConnection(elasticClient);
            return elasticClient;
        }

        private void ValidateElasticClientConnection(ElasticClient client)
        {
            var response = client.Ping();
            if (!response.IsValid)
            {
                _logger.LogError("Failed to connect to Elasticsearch: {message}", response.DebugInformation);
                throw new ApplicationException("Failed to connect to Elasticsearch");
            }
        }
    }
}