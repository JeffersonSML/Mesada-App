# Deploy — Mesada App

Artefatos de deploy do backend (`backend/Dockerfile`) e desta pasta
(`docker-compose.yml`). **Nada aqui foi publicado** — por decisão explícita,
esta etapa só prepara o caminho; publicar de verdade fica para quando você
escolher a plataforma/conta.

## Opção A — VPS própria via Docker Compose

```bash
cd infra/deploy
cp .env.example .env   # preencha as senhas/chaves
set -a && source .env && set +a

docker compose up -d db
# rode as migrações uma vez, do host, contra o Postgres do compose:
PGHOST=127.0.0.1 PGSUPERUSER_PASSWORD="$PGSUPERUSER_PASSWORD" \
  MESADA_APP_DB_PASSWORD="$MESADA_APP_DB_PASSWORD" MESADA_ADMIN_DB_PASSWORD="$MESADA_ADMIN_DB_PASSWORD" \
  ../db/scripts/provisionar.sh

# crie o bucket no MinIO (uma vez, via console web em :9001 ou via `mc mb`)
# coloque a service account do Firebase em infra/deploy/fcm-service-account.json

docker compose up -d
```

A API sobe em `:8080`. Ponha um reverse proxy com TLS na frente (Caddy,
Traefik, Nginx) — a stack acima não expõe HTTPS por si só.

## Opção B — plataforma gerenciada (Railway, Fly.io, Azure App Service, etc.)

Nessas plataformas, o `backend/Dockerfile` já é suficiente para o build —
aponte a plataforma para ele. Você não precisa dos serviços `db`/`minio`
deste `docker-compose.yml`: use o Postgres gerenciado da própria plataforma
(ou Neon/Supabase) e um bucket S3 real (AWS) ou um storage compatível
oferecido por ela, configurando as mesmas variáveis de ambiente da tabela
abaixo.

Passos que dependem da sua conta na plataforma escolhida (não automatizáveis
por mim sem acesso a ela):
1. Criar o Postgres gerenciado e rodar `infra/db/scripts/provisionar.sh` contra ele.
2. Criar o bucket S3 (ou usar o storage da plataforma).
3. Configurar as variáveis de ambiente da tabela abaixo no painel da plataforma.
4. Apontar o build para `backend/Dockerfile`.

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `ConnectionStrings__MesadaApp` | sim | Connection string da role `mesada_app` (RLS ativo) — sempre com `Pooling=false`, ver docs/adr/0003 |
| `ConnectionStrings__MesadaAdmin` | sim | Connection string da role `mesada_admin` (BYPASSRLS) |
| `Jwt__Chave` | sim | 32+ bytes aleatórios — `openssl rand -base64 48` |
| `PagarMe__SecretKey` | não* | Chave secreta Stone/Pagar.me (`sk_test_...` em sandbox). Vazio = módulo de pagamento inoperante, resto do sistema funciona |
| `Email__ResendApiKey` | não* | Chave da API do Resend. Vazio = e-mails não são enviados |
| `Email__RemetenteEmail` / `Email__RemetenteNome` | não | Remetente exibido nos e-mails |
| `S3__AccessKey` / `S3__SecretKey` / `S3__BucketName` | sim, se usar evidências | Credenciais do storage de evidências |
| `S3__ServiceUrl` | não | Vazio = AWS S3 real (usa `S3__Region`); preenchido = qualquer S3-compatível (MinIO, R2, Spaces) |
| `Fcm__CaminhoServiceAccount` | não* | Caminho do JSON da service account do Firebase. Vazio = push não é enviado |

\* Os três marcados com `*` fazem o respectivo recurso ficar inoperante sem
travar o resto da aplicação — mas o objeto `IOptions` correspondente só é
resolvido no primeiro uso real (login com Master Financeiro tentando criar
assinatura, primeira notificação, etc.), então um valor ausente só aparece
como erro quando aquele fluxo específico for exercitado, não no boot da API.

## O que NÃO foi validado nesta etapa

- **Nenhuma imagem Docker foi de fato construída ou executada** — o `docker
  build`/`docker run` não pôde ser testado neste ambiente de
  desenvolvimento (daemon Docker indisponível no sandbox, ver
  docs/adr/0006-*.md). O que FOI validado: `dotnet publish` do projeto Api
  roda limpo e o binário publicado sobe corretamente contra o Postgres real
  (mesmo teste de fumaça feito nas Etapas 3 e 6).
- **Nenhum deploy foi feito** em nenhuma plataforma — por decisão do
  responsável pelo produto nesta etapa.
