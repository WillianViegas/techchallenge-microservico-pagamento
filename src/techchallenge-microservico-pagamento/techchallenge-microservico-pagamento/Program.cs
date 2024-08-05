using Domain.Entities;
using Infra.Configurations.Database;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Repositories;
using techchallenge_microservico_pagamento.Services.Interfaces;
using techchallenge_microservico_pagamento.Services;
using Amazon.S3;
using Amazon.SQS;
using Infra.SQS;
using LocalStack.Client.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());
});

builder.Services.AddSingleton<MongoClient>(_ => new MongoClient());
builder.Services.AddSingleton<IMongoDatabase>(provider => provider.GetRequiredService<MongoClient>().GetDatabase("LanchoneteTotem"));
builder.Services.AddSingleton<IMongoCollection<Carrinho>>(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<Carrinho>("Carrinho"));
builder.Services.AddSingleton<IMongoCollection<Pedido>>(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<Pedido>("Pedido"));

builder.Services.AddTransient<ICarrinhoRepository, CarrinhoRepository>();
builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
builder.Services.AddTransient<IPedidoService, PedidoService>();

builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddAWSServiceLocalStack<IAmazonSQS>();
builder.Services.AddAWSServiceLocalStack<IAmazonS3>();
builder.Services.AddTransient<ISQSConfiguration, SQSConfiguration>();
builder.Services.AddHostedService<SqsListenerService>();


builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection(nameof(DatabaseConfig)));
builder.Services.AddSingleton<IDatabaseConfig>(sp => sp.GetRequiredService<IOptions<DatabaseConfig>>().Value);

builder.Services.AddControllers();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddSwaggerGen(opts => opts.EnableAnnotations());

var app = builder.Build();

app.Use(async (context, next) =>
{
    // Content Security Policy (CSP) Header
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self'; connect-src 'self'; font-src 'self'; object-src 'none'; frame-src 'none';");

    // Anti-clickjacking Header
    context.Response.Headers.Add("X-Frame-Options", "DENY");

    // Strict-Transport-Security Header
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

    // X-Content-Type-Options Header
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    // Cache-Control Headers
    context.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
    context.Response.Headers.Add("Pragma", "no-cache");
    context.Response.Headers.Add("Expires", "0");

    // Remove unnecessary headers
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("Server");

    // Remove Date header
    context.Response.Headers.Remove("Date");

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UsePathBase(builder.Configuration["App:Pathbase"]);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();
