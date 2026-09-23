# ADR 0006: Pagamento (Pagar.me), e-mail, push, storage e artefatos de deploy

- Status: aceito
- Data: 2026-09-23

## Contexto

Etapa 6 do roadmap: plugar os quatro serviços de infraestrutura que a
especificação original previa atrás de interfaces (`IPaymentProvider` já
existia desde a Etapa 3) — pagamento, push (FCM), e-mail (SendGrid/Resend) e
storage de evidências (S3) — mais preparar o deploy do backend.

## Decisões do responsável pelo produto (mudaram o escopo original)

1. **Gateway de pagamento: Stone/Pagar.me, não Asaas/Stripe.** A
   especificação original citava Asaas ou Stripe; o responsável pelo
   produto já tem conta na Stone/Pagar.me e pediu para usar essa. A
   interface `IPaymentProvider` (Application) já foi desenhada para ser
   agnóstica de gateway — só a implementação concreta muda
   (`Mesada.Infrastructure.Payments.PagarMePaymentProvider`).
2. **Sem chave de sandbox da Stone disponível** nesta sessão — a validação
   contra a API real não foi possível (ver seção de validação abaixo).
3. **Push/e-mail/storage: sem contas ainda** — implementados com os SDKs
   oficiais de verdade (não simulados), mas configuráveis para apontar a
   alternativas locais/self-hosted (MinIO no lugar de S3) até o responsável
   decidir os provedores definitivos.
4. **Deploy: só artefatos, sem publicar nada** — `backend/Dockerfile` e
   `infra/deploy/docker-compose.yml` ficam prontos; nenhuma conta de
   hospedagem foi criada nem usada em nome do produto.

## `IPaymentProvider` ganhou parâmetros que não existiam

A assinatura original de `CriarAssinaturaAsync` (Etapa 3) só tinha
`providerCustomerId` e `planoCodigo`. Ao implementar de verdade contra a API
do Pagar.me (inspecionada por reflexão sobre o `.dll` real do pacote NuGet
"PagarMe" — não adivinhada), ficou claro que criar uma assinatura
exige um **preço** e um **token de cartão** (`cardToken`, gerado no
frontend via SDK JS do gateway — o número do cartão nunca deve trafegar
pelo nosso backend). A assinatura foi ampliada para
`CriarAssinaturaAsync(providerCustomerId, planoCodigo, precoMensalCentavos, cardToken, ct)`.
Sem código de produção ainda chamando esse método, a mudança foi segura
(conferido com grep antes de alterar).

## Validação: o que foi possível provar de verdade nesta sessão

Sem contas reais em nenhum dos quatro provedores, a estratégia foi a mesma
das Etapas 3 e 5 — nunca aceitar "parece certo" quando dá para provar:

1. **Toda assinatura de método/tipo usada nos quatro adapters foi conferida
   por reflexão sobre o `.dll` real do pacote NuGet correspondente**
   (`PagarMe`, `Resend`, `AWSSDK.S3`, `FirebaseAdmin`) antes de escrever o
   código — não por lembrança/suposição. Isso já pegou dois erros reais
   antes de rodar qualquer teste:
   - `Resend`: o cast de `Dictionary<string,string>` para `IReadOnlyDictionary`
     via `as` silenciosamente viraria `null` dependendo do tipo concreto
     recebido — trocado por uma cópia explícita.
   - `S3`: `SignatureVersion = "4"` no `AmazonS3Config` **não** muda o
     esquema de assinatura de `GetPreSignedURLAsync` quando `ServiceUrl` é
     customizado sem `RegionEndpoint` — continua saindo V2
     (`AWSAccessKeyId`/`Signature`), só descoberto rodando o teste local e
     inspecionando a URL de verdade. Documentado no código; revisitar
     quando houver um provedor S3-compatível real para confirmar se aceita V2.
2. **Pagar.me e Resend: validados contra um servidor HTTP local** que
   imita o formato de resposta real de cada API (`PagarMePaymentProviderTests`,
   `ResendEmailSenderTests`) — prova que o adapter monta a requisição
   certa e interpreta a resposta certa, não que a API real se comporta
   assim (só uma chamada real, com chave de sandbox, provaria isso).
3. **S3: `EnviarAsync` validado contra servidor HTTP local; `ObterUrlTemporariaAsync`
   validado de verdade sem servidor nenhum** — geração de URL pré-assinada é
   assinatura local (SigV2/SigV4), não uma chamada de rede.
4. **FCM: só a montagem da mensagem (`ConstruirMensagem`) é testável sem uma
   service account real** — extraída à parte exatamente por isso
   (`FcmPushNotificationSenderTests`). `FirebaseApp.Create`/`SendAsync` não
   foram exercitados.
5. **Nenhuma chamada real foi feita contra Pagar.me, Resend, FCM ou um S3
   de verdade.** Isso só acontece quando o responsável pelo produto
   fornecer as credenciais de cada um.
6. **Deploy: `dotnet publish` do projeto Api testado e o binário publicado
   validado rodando contra o Postgres real** (mesmo teste de fumaça das
   Etapas 3/5). **`docker build`/`docker run` não puderam ser testados** —
   o daemon Docker deste sandbox não inicia (`ulimit: error setting limit
   (Operation not permitted)`), mesma classe de restrição de ambiente do
   ADR 0005.

37 testes automatizados no total após esta etapa (27 do motor de cálculo +
10 de integração, todos rodados com sucesso antes deste commit).

## Cada serviço externo é opcional em runtime, não trava o boot

`PagarMe:SecretKey`, `Email:ResendApiKey` e `Fcm:CaminhoServiceAccount`
podem ficar vazios sem impedir a API de subir — os clientes desses SDKs são
registrados via fábricas lazy (mesmo padrão do `NpgsqlDataSource`, ADR
0003) e só falham quando o recurso correspondente é de fato usado (criar
assinatura, mandar e-mail, mandar push). `S3` é a exceção prática: qualquer
fluxo de evidência depende dele, então em produção suas credenciais são,
na prática, obrigatórias.

## Consequências

- Qualquer novo provedor de pagamento (ex.: InfinitePay, cogitado mas não
  implementado nesta etapa) implementa a mesma `IPaymentProvider` sem tocar
  em Application/Api — é exatamente o que a interface foi desenhada para
  permitir.
- `infra/deploy/README.md` documenta as variáveis de ambiente e os dois
  caminhos de deploy (VPS via Docker Compose, ou plataforma gerenciada) —
  nenhum dos dois foi executado, ambos exigem uma conta do responsável
  pelo produto.
- Antes de aceitar pagamentos de verdade, alguém com acesso à conta
  Stone/Pagar.me precisa rodar `PagarMePaymentProviderTests`-equivalente
  contra a API de sandbox real (com uma chave `sk_test_...`) para confirmar
  que o `BaseResponse`/`PagarMeErrorsResponse` reais têm exatamente o
  formato assumido aqui.
