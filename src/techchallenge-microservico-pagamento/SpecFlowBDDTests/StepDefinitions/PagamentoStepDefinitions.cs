using Moq;
using NUnit.Framework;
using System;
using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Services.Interfaces;
using TechTalk.SpecFlow;

namespace SpecFlowBDDTests.StepDefinitions
{
    [Binding]
    public class PagamentoStepDefinitions
    {
        private string _idPedido;
        private string _pedidoJson;
        private Pedido _pedido;

        #region FinalizarPedido
        
        [Given(@"Que devo finalizar e gerar o pagamento de um pedido")]
        public void GivenQueDevoFinalizarEGerarOPagamentoDeUmPedido()
        {
            _pedidoJson = "{\r\n    \"Produtos\": [\r\n        {\r\n            \"id\": \"65a315a4db1f522d916d935a\",\r\n            \"nome\": \"Hamburguer especial da casa\",\r\n            \"descricao\": \"Hamburguer artesanal da casa com maionese caseira e molho secreto\",\r\n            \"preco\": 35.99,\r\n            \"categoriaId\": \"65a315a4db1f522d916d9357\"\r\n        }\r\n    ],\r\n    \"Usuario\": {\r\n        \"id\": \"65a315a4db1f522d916d9355\",\r\n        \"nome\": \"Marcos\",\r\n        \"email\": \"marcao@gmail.com\",\r\n        \"cpf\": \"65139370000\"\r\n    },\r\n    \"Total\": 35.99,\r\n    \"Pagamento\":{}\r\n}";
        }

        [When(@"Recebo o id de referencia do pedido")]
        public void WhenReceboOIdDeReferenciaDoPedido()
        {
            _idPedido = Guid.NewGuid().ToString();
            var pedido = Newtonsoft.Json.JsonConvert.DeserializeObject<Pedido>(_pedidoJson);
            _pedido = pedido;
        }

        [Then(@"Crio o objeto de pagamento e associo ao pedido")]
        public async Task ThenCrioOObjetoDePagamentoEAssocioAoPedido()
        {
            //arrange
            var pagamento = new Pagamento()
            {
                OrdemDePagamento = Guid.NewGuid().ToString(),
                QRCodeUrl = "www.qrCode.com.br"
            };

            _pedido.Pagamento = pagamento;
            _pedido.Status = EPedidoStatus.PendentePagamento;

            var pedidoService = new Mock<IPedidoService>().Object;
            Mock.Get(pedidoService)
                .Setup(service => service.FinalizarPedido(_idPedido))
            .ReturnsAsync(_pedido);

            //act
            var result = await pedidoService.FinalizarPedido(_idPedido);

            //assert
            Assert.NotNull(result);
        }
        #endregion

        #region CreatePedido
        [Given(@"Que devo criar um pedido")]
        public void GivenQueDevoCriarUmPedido()
        {
            _pedidoJson = "{\r\n    \"Produtos\": [\r\n        {\r\n            \"id\": \"65a315a4db1f522d916d935a\",\r\n            \"nome\": \"Hamburguer especial da casa\",\r\n            \"descricao\": \"Hamburguer artesanal da casa com maionese caseira e molho secreto\",\r\n            \"preco\": 35.99,\r\n            \"categoriaId\": \"65a315a4db1f522d916d9357\"\r\n        }\r\n    ],\r\n    \"Usuario\": {\r\n        \"id\": \"65a315a4db1f522d916d9355\",\r\n        \"nome\": \"Marcos\",\r\n        \"email\": \"marcao@gmail.com\",\r\n        \"cpf\": \"65139370000\"\r\n    },\r\n    \"Total\": 35.99,\r\n    \"Pagamento\":{}\r\n}";
        }

        [When(@"Recebo as informacoes do meu pedido")]
        public void WhenReceboAsInformacoesDoMeuPedido()
        {
            var pedido = Newtonsoft.Json.JsonConvert.DeserializeObject<Pedido>(_pedidoJson);
            _pedido = pedido;
        }

        [Then(@"Valido o objeto")]
        public void ThenValidoOObjeto()
        {
            ValidarPedido(_pedido);
        }

        [Then(@"Crio o pedido")]
        public void ThenCrioOPedido()
        {
            //arrange
            var pedidoService = new Mock<IPedidoService>().Object;

            Mock.Get(pedidoService)
                .Setup(service => service.CreatePedido(It.IsAny<Pedido>()))
                .ReturnsAsync(_pedido);

            //act
            var result = pedidoService.CreatePedido(_pedido);

            //assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetPedidoById
        
        [Given(@"Que devo buscar um pedido pelo id")]
        public void GivenQueDevoBuscarUmPedidoPeloId()
        {
            _pedido = GetPedidoObj();
        }

        [When(@"Recebo o id de referencia para buscar o pedido")]
        public void WhenReceboOIdDeReferenciaParaBuscarPedido()
        {
            _idPedido = Guid.NewGuid().ToString();
            _pedido.Id = _idPedido;
        }

        [Then(@"Busco e retorno o respectivo pedido")]
        public async Task ThenBuscoERetornoORespectivoPedido()
        {
            //arrange
            var pedidoService = new Mock<IPedidoService>().Object;

            Mock.Get(pedidoService)
                .Setup(service => service.GetPedidoById(_idPedido))
            .ReturnsAsync(_pedido);

            //act
            var result = await pedidoService.GetPedidoById(_idPedido);

            //assert
            Assert.NotNull(result);
        }
        #endregion

        private bool ValidarPedido(Pedido pedido)
        {
            if (pedido.Total <= 0)
                return false;

            if (!pedido.Produtos.Any())
                return false;

            return true;
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
