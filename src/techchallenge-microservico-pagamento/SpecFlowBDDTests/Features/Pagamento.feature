Feature: Pagamento

@mytag
Scenario: FinalizarPedido
	Given Que devo finalizar e gerar o pagamento de um pedido
	When Recebo o id de referencia do pedido
	Then Crio o objeto de pagamento e associo ao pedido

@mytag
Scenario: CreatePedido
	Given Que devo criar um pedido
	When Recebo as informacoes do meu pedido 
	Then Valido o objeto
	And Crio o pedido

@mytag
Scenario: GetPedidoById
	Given Que devo buscar um pedido pelo id
	When Recebo o id de referencia para buscar o pedido
	Then Busco e retorno o respectivo pedido