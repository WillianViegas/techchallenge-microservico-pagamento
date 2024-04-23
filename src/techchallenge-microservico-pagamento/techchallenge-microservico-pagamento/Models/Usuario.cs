using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace techchallenge_microservico_pagamento.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string CPF { get; set; }
        public string? Tipo { get; set; } = null;
        public string? Senha { get; set; } = null;
    }
}
