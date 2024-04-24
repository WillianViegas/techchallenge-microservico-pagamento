using Domain.Entities;
using Moq;
using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Services;
using techchallenge_microservico_pagamento.Services.Interfaces;

namespace techchallenge_ms_pagamento_test
{
    public class PagamentoTests
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