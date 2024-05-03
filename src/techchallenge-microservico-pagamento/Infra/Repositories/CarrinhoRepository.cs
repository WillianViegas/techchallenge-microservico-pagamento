using Domain.Entities;
using Infra.Configurations.Database;
using MongoDB.Driver;
using techchallenge_microservico_pagamento.Repositories.Interfaces;

namespace techchallenge_microservico_pagamento.Repositories
{
    public class CarrinhoRepository : ICarrinhoRepository
    {
        private readonly IMongoCollection<Carrinho> _collection;

        public CarrinhoRepository(IDatabaseConfig databaseConfig)
        {
            var connectionString = databaseConfig.ConnectionString.Replace("user", databaseConfig.User).Replace("password", databaseConfig.Password);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseConfig.DatabaseName);
            _collection = database.GetCollection<Carrinho>("Carrinho");
        }

        public async Task<Carrinho> GetCarrinhoById(string id)
        {
            return await _collection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public async Task UpdateCarrinho(string id, Carrinho carrinho)
        {
            await _collection.ReplaceOneAsync(x => x.Id.ToString() == id, carrinho);
        }
    }
}
