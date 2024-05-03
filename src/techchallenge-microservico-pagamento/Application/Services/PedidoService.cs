using techchallenge_microservico_pagamento.Enums;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using techchallenge_microservico_pagamento.Services.Interfaces;
using techchallenge_microservico_pagamento.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace techchallenge_microservico_pagamento.Services
{
    public class PedidoService : IPedidoService
    {

        private readonly ILogger<PedidoService> _logger;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICarrinhoRepository _carrinhoRepository;


        public PedidoService(ILogger<PedidoService> logger, IPedidoRepository pedidoRepository, ICarrinhoRepository carrinhoRepository)
        {
            _logger = logger;
            _pedidoRepository = pedidoRepository;
            _carrinhoRepository = carrinhoRepository;
        }


        public async Task<Pedido> CreatePedido(Pedido pedido)
        {
            try
            {
                var numeroPedido = await _pedidoRepository.GetAllPedidos();

                var novoPedido = new Pedido
                {
                    Produtos = pedido.Produtos,
                    Total = pedido.Produtos.Sum(x => x.Preco),
                    Status = 0,
                    DataCriacao = DateTime.Now,
                    Numero = numeroPedido.Count + 1,
                    Usuario = pedido.Usuario,
                    IdCarrinho = pedido.IdCarrinho
                };

                await _pedidoRepository.CreatePedido(novoPedido);

                return novoPedido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pedido> FinalizarPedido(string id)
        {
            try
            {
                var pedido = await GetPedidoById(id);
                if (pedido is null)
                {
                    _logger.LogError("Pedido não encontrado");
                    throw new Exception("Pedido não existe");
                }

                if (pedido.Status != EPedidoStatus.Novo) throw new Exception($"Status do pedido não é válido para confirmação. Status: {pedido.Status}, NumeroPedido: {pedido.Numero}");

                pedido.Status = EPedidoStatus.PendentePagamento;
                pedido.Pagamento = new Pagamento()
                {
                    Tipo = ETipoPagamento.QRCode,
                    QRCodeUrl = "www.usdfhosdfsdhfosdfhsdofhdsfds.com.br",
                    OrdemDePagamento = Guid.NewGuid().ToString()
                };

                await _pedidoRepository.UpdatePedido(id, pedido);
                _logger.LogInformation($"Pedido atualizado id: {id}, status: {pedido.Status.ToString()}");

                //desativa o carrinho (pensar se futuramente n é melhor excluir)
                var carrinho = await _carrinhoRepository.GetCarrinhoById(id);
                if (carrinho != null)
                {
                    carrinho.Ativo = false;
                    await _carrinhoRepository.UpdateCarrinho(carrinho.Id, carrinho);
                }

                return pedido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pedido> GetPedidoById(string id)
        {
            try
            {
                return await _pedidoRepository.GetPedidoById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

    }
}
