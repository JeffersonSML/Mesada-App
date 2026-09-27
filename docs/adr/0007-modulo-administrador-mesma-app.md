# ADR 0007: Módulo Administrador vive na mesma aplicação Web, não em projeto separado

- Status: aceito
- Data: 2026-09-27

## Contexto

`docs/especificacao.md#módulo-administrador` já previa uma tela "Controle
de Acessos" com "gestão de administradores do sistema, permissões por
administrador, log de auditoria" — junto com Famílias, Assinaturas,
Suporte, Métricas e Categorias Padrão. O [ADR 0004](0004-web-via-lovable.md)
e `web/README.md` registravam a intenção original de tratar o Módulo
Administrador como "um sistema/projeto Lovable separado" do painel do
Master.

Ao pedir para o sistema ficar "pronto para comercialização em massa", o
responsável pelo produto pediu explicitamente um "painel administrativo"
onde só ele (dono) tem acesso, podendo dar acesso a outras pessoas da
equipe dentro de grupos específicos (ex.: "desenvolvedores/tecnologia").
Perguntado se isso deveria ser um projeto Lovable separado ou uma nova
seção protegida por papel dentro do mesmo app, a decisão foi **manter na
mesma aplicação**.

## Decisão

O Módulo Administrador (começando pela gestão de Administradores e Grupos
de acesso — as demais telas previstas na spec, como Famílias/Assinaturas/
Suporte/Métricas, ainda não têm backend e ficam para depois) é implementado
como uma nova área (`/admin/...`) dentro do mesmo projeto Lovable
("Mesada Master Panel"), não como um projeto separado.

**Autenticação e dados continuam completamente segregados**, apesar de
compartilharem o mesmo bundle de frontend:
- Login do Administrador é `POST /api/admin/auth/login` — endpoint, JWT e
  claims (`administrador_id`, `grupo_administrador_id`,
  `grupo_administrador_sistema`) totalmente distintos do login do Master.
- Um Administrador nunca tem `familia_id`; a tabela `administradores` e
  `grupos_administrador` nunca são acessíveis pela role `mesada_app`
  (RLS/tenant), só por `mesada_admin` (ver
  [ADR 0002](0002-schema-e-rls.md)).
- O frontend guarda os tokens em chaves de storage diferentes
  (`mesada.master.token` vs `mesada.admin.token`) e nunca deve linkar uma
  área a partir da outra.

## Justificativa

- Reduz custo operacional: um único projeto/deploy Lovable para manter, em
  vez de dois.
- O isolamento real (o que importa para segurança) já é garantido no
  backend — login, claims, tabelas e RLS são inteiramente segregados. Ter
  dois builds de frontend não adicionaria isolamento de dados algum, só
  mais superfície para manter em sincronia.
- Decisão explícita do responsável pelo produto, entre as duas opções
  apresentadas.

## Consequências

- `web/README.md` e o `README.md` raiz precisam refletir esta decisão — a
  menção anterior a "sistema/projeto Lovable separado" fica desatualizada e
  deve ser corrigida.
- A Project Knowledge do projeto Lovable documenta as duas áreas e a regra
  de nunca misturá-las (ver seção "Duas áreas com auth totalmente
  separadas").
- Grupos de acesso (`GrupoAdministrador`) são dados, não código: o grupo
  "Owner" (`sistema = true`) tem acesso total e é o único que não pode ser
  editado/removido; qualquer outro grupo (ex.: "Tecnologia") é criado
  livremente pelo Owner, sem precisar de deploy novo.
- **Limitação atual, importante**: o dicionário `permissoes` de um grupo
  (chave→booleano, editável livre na tela de Grupos) ainda não é lido por
  nenhum endpoint para autorizar uma ação específica — a única checagem de
  autorização que existe hoje é binária (`ExigirOwner`/`grupo.sistema`),
  usada nos endpoints de Administradores e Grupos. Ou seja, todo
  administrador não-Owner tem hoje exatamente o mesmo acesso (leitura),
  não importa quais chaves de permissão o grupo dele tenha marcadas — as
  chaves são só um registro para uso futuro, quando existirem mais telas
  do Módulo Administrador para gatear por permissão granular.
- As demais telas do Módulo Administrador previstas na spec (Famílias,
  Assinaturas, Suporte, Métricas, Categorias Padrão) continuam sem backend
  e sem tela — ficam para uma etapa futura, quando os endpoints
  correspondentes existirem.
