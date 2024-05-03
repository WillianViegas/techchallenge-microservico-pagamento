using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace techchallenge_microservico_pagamento.Models
{
    public class Produto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Nome { get; set; } = null;
        public string? Descricao { get; set; } = null;
        public decimal Preco { get; set; }
        public string? CategoriaId { get; set; } = null;
    }
}
