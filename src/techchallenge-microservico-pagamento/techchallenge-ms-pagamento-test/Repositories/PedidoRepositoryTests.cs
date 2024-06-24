using Moq;
using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Services.Interfaces;

namespace techchallenge_ms_pagamento_test.Repositories
{
    public class PedidoRepositoryTests
    {
        [Fact]
        public async Task FinalizarPedido()
        {
            //arrange
            var pedido = GetPedidoObj();
            var pedidoService = new Mock<IPedidoService>().Object;

            Mock.Get(pedidoService)
                .Setup(service => service.FinalizarPedido(It.IsAny<string>()))
            .ReturnsAsync(pedido);

            //act
            var result = await pedidoService.FinalizarPedido("difogidgjdg");

            //assert
            Assert.NotNull(result);
        }


        [Fact]
        public async Task CreatePedido()
        {
            var pedidoRepository = new Mock<IPedidoRepository>().Object;
            var pedido = GetPedidoObj();

            Mock.Get(pedidoRepository)
              .Setup(rep => rep.CreatePedido(pedido))
                .ReturnsAsync(pedido);

            //act
            var result = await pedidoRepository.CreatePedido(pedido);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAllPedidos()
        {
            var pedidoRepository = new Mock<IPedidoRepository>().Object;
            var pedido = GetPedidoObj();
            var pedido2 = GetPedidoObj();
            var pedido3 = GetPedidoObj();

            var listaPedidos = new List<Pedido>();
            listaPedidos.Add(pedido);
            listaPedidos.Add(pedido2);
            listaPedidos.Add(pedido3);

            Mock.Get(pedidoRepository)
              .Setup(rep => rep.GetAllPedidos())
                .ReturnsAsync(listaPedidos);

            //act
            var result = await pedidoRepository.GetAllPedidos();

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPedidoById()
        {
            var pedidoRepository = new Mock<IPedidoRepository>().Object;
            var pedido = GetPedidoObj();
            var idPedidoOrigem = Guid.NewGuid().ToString();
            pedido.IdPedidoOrigem = idPedidoOrigem;

            Mock.Get(pedidoRepository)
              .Setup(rep => rep.GetPedidoByIdOrigem(idPedidoOrigem))
                .ReturnsAsync(pedido);

            //act
            var result = await pedidoRepository.GetPedidoByIdOrigem(idPedidoOrigem);

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdatePedido()
        {
            var pedidoRepository = new Mock<IPedidoRepository>().Object;
            var pedido = GetPedidoObj();
            var idPedido = Guid.NewGuid().ToString();
            pedido.Id = idPedido;

            Mock.Get(pedidoRepository)
              .Setup(rep => rep.UpdatePedido(idPedido, pedido))
                .Verifiable();
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