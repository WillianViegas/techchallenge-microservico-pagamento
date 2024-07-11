using Infra.Configurations.Database;
using MongoDB.Driver;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;

namespace techchallenge_microservico_pagamento.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly IMongoCollection<Pedido> _collection;

        public PedidoRepository(IDatabaseConfig databaseConfig)
        {
            var connectionString = databaseConfig.ConnectionString.Replace("user", databaseConfig.User).Replace("password", databaseConfig.Password);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseConfig.DatabaseName);
            _collection = database.GetCollection<Pedido>("Pedido");
        }

        public async Task<Pedido> CreatePedido(Pedido pedido)
        {
            await _collection.InsertOneAsync(pedido);
            return pedido;
        }

        public async Task<IList<Pedido>> GetAllPedidos()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Pedido> GetPedidoByIdOrigem(string id)
        {
            var pedido = await _collection.Find(x => x.IdPedidoOrigem.ToString() == id).FirstOrDefaultAsync();
            return pedido;
        }

        public async Task UpdatePedido(string id, Pedido pedidoInput)
        {
            await _collection.ReplaceOneAsync(x => x.IdPedidoOrigem.ToString() == id, pedidoInput);
        }

        public async Task DeletePedido(string pedidoId)
        {
            await _collection.DeleteOneAsync(x => x.Id == pedidoId);
        }
    }
}
