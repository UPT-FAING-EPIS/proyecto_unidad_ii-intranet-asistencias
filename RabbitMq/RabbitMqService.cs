using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Nest;
using System.Text.Json;
using RabbitMQToElasticsearch.Models;

namespace RabbitMQToElasticsearch.RabbitMq{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ConnectionFactory _connectionFactory = new ConnectionFactory();
        private IConnection? _connection;
        private IModel? _channel;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            CreateRabbitMqConnection(configuration);
            DeclareQueue();
        }

        public EventingBasicConsumer? ConsumeQueue()
        {
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: "logs", autoAck: true, consumer: consumer);
            return consumer;
        }

        private void CreateRabbitMqConnection(IConfiguration configuration)
        {
            try
            {
                _connectionFactory.HostName = GetRabbitMqHostName(configuration);
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to RabbitMQ");
                throw new ApplicationException("Error connecting to RabbitMQ", ex);
            }
        }

        private string? GetRabbitMqHostName(IConfiguration configuration)
        {
            return configuration.GetSection("RabbitMQ:HostName").Value;
        }

        private void DeclareQueue()
        {
            _channel.QueueDeclare(queue: "logs", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
    }
}