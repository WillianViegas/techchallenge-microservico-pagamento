using Microsoft.Extensions.Logging;
using Moq;
using techchallenge_microservico_pagamento.Controllers;
using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Services.Interfaces;

namespace techchallenge_ms_pagamento_test.Controllers
{
    public class PagamentoControllerTests
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

            var mock = new Mock<ILogger<PagamentoController>>();
            ILogger<PagamentoController> logger = mock.Object;

            var controller = new PagamentoController(logger, pedidoService);

            //act
            var result = await controller.FinalizarPedido(idPedido);

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