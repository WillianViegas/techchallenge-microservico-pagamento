using Domain.Entities;
using techchallenge_microservico_pagamento.Models;

namespace techchallenge_microservico_pagamento.Repositories.Interfaces
{
    public interface ICarrinhoRepository
    {
        public Task<Carrinho> GetCarrinhoById(string id);
        public Task UpdateCarrinho(string id, Carrinho carrinho);
    }
}
