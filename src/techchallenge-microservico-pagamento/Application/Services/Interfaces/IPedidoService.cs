using techchallenge_microservico_pagamento.Models;

namespace techchallenge_microservico_pagamento.Services.Interfaces
{
    public interface IPedidoService
    {
        public Task<Pedido> FinalizarPedido(string id);
        public Task<Pedido> CreatePedido(Pedido pedido);
        public Task<Pedido> GetPedidoById(string id);
    }
}
