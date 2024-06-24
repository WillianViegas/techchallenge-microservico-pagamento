# techchallenge-microservico-pagamento

Repositório relacionado a Fase 4 do techChallenge FIAP. Refatoração do projeto de totem em três microsserviços (Pedido, Pagamento e Produção);

Repositório bloqueado para push na main, é necessário abrir um PullRequest;

Este microsserviço tem como objetivo receber informações de pedidos em formato de mensagens vindas de uma fila do serviço de mensageria SQS da AWS, convertendo as mensagens em objetos do tipo pedido e realizando o cadastro na base de dados. Possibilitando assim a finalização do pedido, gerando uma ordem de pagamento e QRCode para o usuário. (Atualmente não está implementado o terceiro de pagamento, então está sendo gerado um objeto de pagamento apenas para testes do fluxo).

Após pedido finalizado é enviado para uma fila de pedidos gerados, onde será capturado pelo microsserviço de produção.

Estrutura
 - Banco de dados = MongoDB
 - Simulação de ambiente AWS = Localstack
 - Implementação de fila na AWS = SQS
 - Containers = Docker + Docker-Compose
 - Orquestração de containers = Kubernetes
 - Testes unitários e com BDD utilizando a extensão SpecFlow
 - Cobertura de código = SonarCloud
 - Pipeline = Github Actions
 - Deploy = Terraform



![image](https://github.com/WillianViegas/techchallenge-microservico-pagamento/assets/58482678/d4b8584f-7ab1-49e5-87a5-f9778300deb9)



Fluxograma:
https://www.figma.com/board/foY2Q9t6aj6Gzv9WK8actk/Documenta%C3%A7%C3%A3o-Sistema-DDD?node-id=0%3A1&t=oY6vBdqPodcM5LMR-1

Video explicando a estrutura:
https://youtu.be/-OZgHsUoLkM

Links para repositórios relacionados: 

- techchallenge-microservico-pedido (Microsserviço de Pedido):
  - https://github.com/WillianViegas/techchallenge-microservico-pedido
- techchallenge-microservico-producao (Microsserviço de Produção)
  - https://github.com/WillianViegas/techchallenge-microservico-producao
- TechChallenge-LanchoneteTotem (Repositório com o projeto que originou os microsserviços e histórico das fases):
  - https://github.com/WillianViegas/TechChallenge-LanchoneteTotem
