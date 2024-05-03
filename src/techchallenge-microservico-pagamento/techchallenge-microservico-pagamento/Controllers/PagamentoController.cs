using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using techchallenge_microservico_pagamento.Models;
using techchallenge_microservico_pagamento.Services;
using techchallenge_microservico_pagamento.Services.Interfaces;

namespace techchallenge_microservico_pagamento.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PagamentoController : ControllerBase
    {
        private readonly ILogger<PagamentoController> _logger;
        private IPedidoService _pedidoService;


        public PagamentoController(ILogger<PagamentoController> logger, IPedidoService pedidoService)
        {
            _logger = logger;
            _pedidoService = pedidoService;
        }

        [HttpGet("/teste")]
        public IResult Teste()
        {
            try
            {
                return TypedResults.Ok("Teste");
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }


        [HttpPost("/finalizarPedido")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [SwaggerOperation(
        Summary = "Finalizar pedido",
        Description = "Finaliza o pedido gerando QRcode para pagamento")]
        public async Task<IResult> FinalizarPedido(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return TypedResults.BadRequest("Id inválido");

                var pedido = await _pedidoService.FinalizarPedido(id);
                return TypedResults.Created($"/pedido/{pedido.Id}", pedido);
            }
            catch (Exception ex)
            {
                var erro = $"Erro ao confirmar criar o pedido. Id: {id}";
                _logger.LogError(erro, ex);
                return TypedResults.Problem(erro);
            }
        }
    }
}
