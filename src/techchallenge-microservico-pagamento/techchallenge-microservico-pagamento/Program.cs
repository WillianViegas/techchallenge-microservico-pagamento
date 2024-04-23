using Domain.Entities;
using Infra.Configurations.Database;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Repositories;
using techchallenge_microservico_pagamento.Services.Interfaces;
using techchallenge_microservico_pagamento.Services;

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

builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection(nameof(DatabaseConfig)));
builder.Services.AddSingleton<IDatabaseConfig>(sp => sp.GetRequiredService<IOptions<DatabaseConfig>>().Value);

builder.Services.AddControllers();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);


var app = builder.Build();

app.UseCors("AllowAll");
app.UsePathBase(builder.Configuration["App:Pathbase"]);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
