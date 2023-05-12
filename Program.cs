using RabbitMQToElasticsearch.Services;
using RabbitMQToElasticsearch.ElasticSearch;
using RabbitMQToElasticsearch.RabbitMq;
using RabbitMQToElasticsearch.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Nest;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<IElasticSearchService, ElasticSearchService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logService = app.Services.GetRequiredService<ILogService>();
logService.ConsumeQueue();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
