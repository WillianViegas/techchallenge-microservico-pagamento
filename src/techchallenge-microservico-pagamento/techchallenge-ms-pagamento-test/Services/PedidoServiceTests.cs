using Moq;
using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Services.Interfaces;

namespace techchallenge_ms_pagamento_test.Services
{
    public class PedidoServiceTests
    {
        [Fact]
        public async Task FinalizarPedido()
        {
            //arrange
            var pedido = GetPedidoObj();
            var pedidoService = new Mock<IPedidoService>().Object;
            var idPedido = Guid.NewGuid().ToString();
            pedido.Id = idPedido;

            Mock.Get(pedidoService)
                .Setup(service => service.FinalizarPedido(idPedido))
            .ReturnsAsync(pedido);

            //act
            var result = await pedidoService.FinalizarPedido(idPedido);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreatePedido()
        {
            //arrange
            var pedido = GetPedidoObj();
            var pedidoService = new Mock<IPedidoService>().Object;

            Mock.Get(pedidoService)
                .Setup(service => service.CreatePedido(pedido))
            .ReturnsAsync(pedido);

            //act
            var result = await pedidoService.CreatePedido(pedido);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPedidoById()
        {
            //arrange
            var pedido = GetPedidoObj();
            var pedidoService = new Mock<IPedidoService>().Object;
            var idPedido = Guid.NewGuid().ToString();
            pedido.Id = idPedido;

            Mock.Get(pedidoService)
                .Setup(service => service.GetPedidoById(idPedido))
            .ReturnsAsync(pedido);

            //act
            var result = await pedidoService.GetPedidoById(idPedido);

            //assert
            Assert.NotNull(result);
        }

        private Pedido GetPedidoObj()
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


            var pedido = new Pedido();
            pedido.Id = "";
            pedido.IdCarrinho = pedido.IdCarrinho;
            pedido.Numero = 1;
            pedido.DataCriacao = DateTime.Now;
            pedido.Produtos = produtos;
            pedido.Status = EPedidoStatus.Novo;

            return pedido;
        }
    }
}