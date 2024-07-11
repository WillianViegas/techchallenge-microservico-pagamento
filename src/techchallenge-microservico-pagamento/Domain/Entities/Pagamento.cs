using techchallenge_microservico_pagamento.Enums;

namespace techchallenge_microservico_pagamento.Models
{
    public class Pagamento
    {
        public string Id { get; set; }
        public ETipoPagamento? Tipo { get; set; }
        public string? QRCodeUrl { get; set; }
        public string? Bandeira { get; set; }
        public string? OrdemDePagamento { get; set; }
    }
}
