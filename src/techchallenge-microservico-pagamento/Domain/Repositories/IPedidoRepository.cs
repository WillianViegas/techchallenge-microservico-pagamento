using techchallenge_microservico_pagamento.Models;

namespace techchallenge_microservico_pagamento.Repositories.Interfaces
{
    public interface IPedidoRepository
    {
        public Task<Pedido> CreatePedido(Pedido pedido);
        public Task<IList<Pedido>> GetAllPedidos();
        public Task<Pedido> GetPedidoById(string id);
        public Task UpdatePedido(string id, Pedido pedidoInput);
    }
}
