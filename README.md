# techchallenge-microservico-pagamento

Repositório relacionado a Fase 4 e 5 do techChallenge FIAP. Refatoração do projeto de totem em três microsserviços (Pedido, Pagamento e Produção) + Utilização do Padrão SAGA;

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



Fluxograma (Contendo arquitetura e Justificativa da utilização do padrão SAGA, está na Fase 5):
https://www.figma.com/board/foY2Q9t6aj6Gzv9WK8actk/Documenta%C3%A7%C3%A3o-Sistema-DDD?node-id=0%3A1&t=oY6vBdqPodcM5LMR-1

Video explicando a estruturas
- Fase 4 (Fase Passada): https://youtu.be/-OZgHsUoLkM
- Fase 5 (Fase Atual): https://youtu.be/_8Dvd5Me59w

Link para os relatórios OWASP ZAP:
- Vulnerabilidades: https://fiap-docs.s3.amazonaws.com/OWASP+ZAP+Relatorios/Vulnerabilidades/MS-Pagamento.html
- Correções: https://fiap-docs.s3.amazonaws.com/OWASP+ZAP+Relatorios/Correcoes/MS-Pagamento.html

Links para repositórios relacionados: 

- techchallenge-microservico-pedido (Microsserviço de Pedido):
  - https://github.com/WillianViegas/techchallenge-microservico-pedido
- techchallenge-microservico-producao (Microsserviço de Produção)
  - https://github.com/WillianViegas/techchallenge-microservico-producao
- TechChallenge-LanchoneteTotem (Repositório com o projeto que originou os microsserviços e histórico das fases):
  - https://github.com/WillianViegas/TechChallenge-LanchoneteTotem
- MS-Cancelamento-Dados (Microsserviço de solicitação de exclusão de dados):
  - https://github.com/WillianViegas/techchallenge-microservico-cancelamento-dados
 

## Rodando ambiente com Docker

### Pré-Requisitos
* Possuir o docker instalado:
    https://www.docker.com/products/docker-desktop/

Acesse o diretório em que o repositório foi clonado através do terminal e
execute os comandos:
 - `docker-compose build` para compilar imagens, criar containers etc.
 - `docker-compose up` para criar os containers do banco de dados e do projeto

### Iniciando e finalizando containers
Para inicializar execute o comando `docker-compose start` e
para finalizar `docker-compose stop`

Lembrando que se você for rodar pelo visual studio fica bem mais simplificado, basta estar com o docker desktop aberto na máquina e escolher a opção abaixo:


![image](https://github.com/user-attachments/assets/80049acd-d909-428a-801b-2ed10e33b04d)

## Rodando ambiente Kubernetes
* Possuir kubernetes instalados:
    https://kubernetes.io/pt-br/docs/setup/

Com o kubernetes instalado corretamente basta ir até a pasta k8s e executar o seguinte comando:
- `kubectl apply -f .`  (serão executados todos os arquivos da pasta iniciando suas configurações)
- `kubectl get deploy` (para ver os deploys que subiram, a API e o banco de dados no nosso caso);
- `kubectl get Pods` (para ver os respectivos pods)
- `kubectl get Svc` (para ver a configuração dos services dos pods, aqui você consegue pegar a porta ou endpoint para acessar seu container)

ex:
![image](https://github.com/user-attachments/assets/b336de8e-8774-4025-a8d3-256dc47317f8)

Caso esteja tendo dificuldades para acessar a respectiva porta você pode utilizar esse comando localmente para gerar um acesso em uma porta de sua escolha, basta abrir o cmd e executar:
`kubectl port-forward deployment/{nomeDeployment} 7003:80 7004:443`

Para finalizar os pods e os deploys você pode executar o seguinte comando:
`kubectl delete -f .`  (serão deletadas todas as configurações dos arquivos iniciados, finalizando assim os pods)
