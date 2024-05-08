using Domain.Entities;
using Moq;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;

namespace techchallenge_ms_pagamento_test.Repositories
{
    public class CarrinhoRepositoryTests
    {
        [Fact]
        public async Task GetCarrinhoById()
        {
            var carrinhoRepository = new Mock<ICarrinhoRepository>().Object;
            var carrinho = GetCarrinhoObj();
            var idCarrinho = Guid.NewGuid().ToString();
            carrinho.Id = idCarrinho;

            Mock.Get(carrinhoRepository)
              .Setup(rep => rep.GetCarrinhoById(idCarrinho))
                .ReturnsAsync(carrinho);

            //act
            var result = await carrinhoRepository.GetCarrinhoById(idCarrinho);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateCarrinho()
        {
            var carrinhoRepository = new Mock<ICarrinhoRepository>().Object;
            var carrinho = GetCarrinhoObj();
            var idCarrinho = Guid.NewGuid().ToString();
            carrinho.Id = idCarrinho;

            Mock.Get(carrinhoRepository)
              .Setup(rep => rep.UpdateCarrinho(idCarrinho, carrinho))
                .Verifiable();
        }

        private Carrinho GetCarrinhoObj()
        {
            var produtos = new List<Produto>();
            var produto = new Produto()
            {
                Id = "",
                Nome = "",
                Descricao = "",
                CategoriaId = "",
                Preco = 10.00m
            };

            produtos.Add(produto);


            var carrinho = new Carrinho()
            {
                Id = "",
                Usuario = null,
                Ativo = true,
                Produtos = produtos,
                Total = 10.00m
            };

            return carrinho;
        }
    }
}