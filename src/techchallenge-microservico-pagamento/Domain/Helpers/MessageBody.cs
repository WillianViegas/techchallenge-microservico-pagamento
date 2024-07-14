namespace techchallenge_microservico_pedido.Models
{
    public class MessageBody
    {
        public string IdTransacao { get; set; }
        public string idPedido { get; set; }
        public string Status { get; set; }
        public DateTime DataTransacao { get; set; }
    }

    public class MessageBodyTransacaoPagamento
    {
        public string Id { get; set; }
        public string OrderDePagamento { get; set; }
        public string Status { get; set; }
        public DateTime DataTransacao { get; set; }
    }
}
