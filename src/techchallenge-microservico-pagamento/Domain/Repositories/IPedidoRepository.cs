﻿using techchallenge_microservico_pagamento.Models;

namespace techchallenge_microservico_pagamento.Repositories.Interfaces
{
    public interface IPedidoRepository
    {
        public Task<Pedido> CreatePedido(Pedido pedido);
        public Task<IList<Pedido>> GetAllPedidos();
        public Task<Pedido> GetPedidoByIdOrigem(string id);
        public Task<Pedido> GetPedidoByOrdemDePagamento(string ordemDePagamento);
        public Task UpdatePedido(string id, Pedido pedidoInput);
        public Task DeletePedido(string pedidoId);
    }
}
