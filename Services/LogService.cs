using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQToElasticsearch.Models;
using RabbitMQToElasticsearch.RabbitMq;
using RabbitMQToElasticsearch.ElasticSearch;

namespace RabbitMQToElasticsearch.Services{
    public class LogService : ILogService
    {
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<LogService> _logger;

        

        public LogService(IConfiguration configuration, ILogger<LogService> logger, IElasticSearchService elasticSearchService, IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _elasticSearchService = elasticSearchService;
            _rabbitMqService = rabbitMqService;
        }

        public void ConsumeQueue()
        {
            var consumer = _rabbitMqService.ConsumeQueue();
            consumer.Received += OnMessageReceived;
            consumer.ConsumerCancelled += OnConsumerCancelled;
        }

        private void OnMessageReceived(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received log message: {message}", message);

            var logEntry = JsonSerializer.Deserialize<LogEntry>(message);
            _elasticSearchService.IndexDocument(logEntry);
        }

        private void OnConsumerCancelled(object model, ConsumerEventArgs ea)
        {
            Console.WriteLine("Consumption has been cancelled by the server. Check your RabbitMQ configuration.");
        }
    }
}