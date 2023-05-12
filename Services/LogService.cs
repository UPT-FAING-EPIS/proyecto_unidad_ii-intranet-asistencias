using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Nest;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace RabbitMQToElasticsearch.Services
{
    
    public class LogService
    {
        
        private readonly ElasticClient _elasticClient;
        private readonly ConnectionFactory _factory = new ConnectionFactory();
        private IConnection _connection = default!;
        private IModel _channel = default!;
        private readonly ILogger<LogService> _logger;

        public LogService(IConfiguration configuration, ILogger<LogService> logger)
        {
            Console.WriteLine( "Si me ejecuto");
            _logger = logger;
            
            var elasticUrl = configuration.GetSection("Elasticsearch:Url").Value;
            if (string.IsNullOrEmpty(elasticUrl))
            {
                throw new ArgumentException("Elasticsearch URL is not configured.");
            }
            var settings = new ConnectionSettings(new Uri(elasticUrl)).DefaultIndex("logs");

            _elasticClient = new ElasticClient(settings);
            
            try
            {
                _factory = new ConnectionFactory() { HostName = configuration.GetSection("RabbitMQ:HostName").Value };
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: "logs",durable: false,exclusive: false,autoDelete: false,arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received log message: {message}", message);
                    var logEntry = JsonSerializer.Deserialize<LogEntry>(message);
                    var indexResponse = _elasticClient.IndexDocument(logEntry);

                    if (!indexResponse.IsValid)
                    {
                        _logger.LogError("Failed to index document: {message}", indexResponse.OriginalException.Message);
                    }
                };
                consumer.ConsumerCancelled += (model, ea) =>
                {
                    Console.WriteLine("Consumption has been cancelled by the server. Check your RabbitMQ configuration.");
                };
                _channel.BasicConsume(queue: "logs",
                                    autoAck: true,
                                    consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to RabbitMQ");
                Console.WriteLine(ex.ToString());
            }
        }
    }
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? Level { get; set; }
        public string? Message { get; set; }
    }

}
