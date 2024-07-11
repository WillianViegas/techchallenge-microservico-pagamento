using techchallenge_microservico_pagamento.Models;

namespace techchallenge_microservico_pedido.Models
{
    public class MessageBody
    {
        public string Id { get; set; }
        public int Numero { get; set; }
        public List<Produto> Produtos { get; set; }
        public Usuario Usuario { get; set; }
        public double Total { get; set; }
        public string IdCarrinho { get; set; }
        public string Status { get; set; }
        public DateTime DataTransacao { get; set; }
        public string IdPedidoOrigem { get; set; }
    }
}
