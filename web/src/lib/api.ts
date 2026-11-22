export const API_BASE_URL =
  (import.meta.env["VITE_API_BASE_URL"] as string | undefined)?.replace(/\/$/, "") ||
  "https://positive-peace-production-1c64.up.railway.app";

const TOKEN_KEY = "mesada.master.token";

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string) {
  window.localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  window.localStorage.removeItem(TOKEN_KEY);
}

export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

function redirectToLogin() {
  clearToken();
  if (typeof window !== "undefined" && window.location.pathname !== "/login") {
    window.location.assign("/login");
  }
}

export async function extractMessage(res: Response): Promise<string> {
  let msg = `Erro ${res.status} ao comunicar com a API.`;
  try {
    const text = await res.text();
    if (!text) return msg;
    try {
      const body = JSON.parse(text);
      if (typeof body === "string") return body;
      if (body?.errors && typeof body.errors === "object") {
        const list = Object.values(body.errors as Record<string, unknown>)
          .flat()
          .filter((x): x is string => typeof x === "string");
        if (list.length) return list.join(" ");
      }
      if (typeof body?.message === "string") msg = body.message;
      else if (typeof body?.detail === "string") msg = body.detail;
      else if (typeof body?.erro === "string") msg = body.erro;
      else if (typeof body?.title === "string") msg = body.title;
    } catch {
      if (text.length < 300) msg = text;
    }
  } catch {
    /* ignore */
  }
  return msg;
}

async function request<T>(path: string, init: RequestInit = {}, auth = true): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set("Content-Type", "application/json");
  if (auth) {
    const token = getToken();
    if (!token) {
      redirectToLogin();
      throw new ApiError(401, "Sessão expirada. Faça login novamente.");
    }
    headers.set("Authorization", `Bearer ${token}`);
  }

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}${path}`, { ...init, headers });
  } catch {
    throw new ApiError(0, "Não foi possível conectar à API. Verifique se o servidor está no ar.");
  }

  if (res.status === 401) {
    if (auth) redirectToLogin();
    throw new ApiError(401, auth ? "Sessão expirada. Faça login novamente." : "Credenciais inválidas.");
  }
  if (!res.ok) throw new ApiError(res.status, await extractMessage(res));
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

const get = <T>(p: string) => request<T>(p);
const post = <T>(p: string, body?: unknown) =>
  request<T>(p, { method: "POST", body: body === undefined ? null : JSON.stringify(body) });
const put = <T>(p: string, body: unknown) => request<T>(p, { method: "PUT", body: JSON.stringify(body) });
const del = (p: string) => request<void>(p, { method: "DELETE" });

// ---- Auth ----
export async function loginMaster(email: string, senha: string): Promise<{ token: string }> {
  return request<{ token: string }>(
    "/api/auth/master/login",
    { method: "POST", body: JSON.stringify({ email, senha }) },
    false,
  );
}

export interface CadastroFamiliaInput {
  nomeFamilia: string;
  nomeMaster: string;
  emailMaster: string;
  senhaMaster: string;
}
export async function cadastrarFamilia(d: CadastroFamiliaInput): Promise<{ token: string }> {
  return request<{ token: string }>(
    "/api/familias",
    { method: "POST", body: JSON.stringify(d) },
    false,
  );
}

const anon = <T>(p: string, body: unknown) =>
  request<T>(p, { method: "POST", body: JSON.stringify(body) }, false);
export const esqueciSenha = (email: string) => anon<void>("/api/auth/master/esqueci-senha", { email });
export const redefinirSenha = (token: string, novaSenha: string) =>
  anon<void>("/api/auth/master/redefinir-senha", { token, novaSenha });
export const resgatarConviteMaster = (codigo: string, email: string, senha: string) =>
  anon<{ token: string }>(`/api/auth/convites/${encodeURIComponent(codigo)}/resgatar-master`, { email, senha });

/** Trocar senha: 401 aqui significa "senha atual errada", não sessão expirada. */
export async function trocarSenhaMaster(senhaAtual: string, novaSenha: string): Promise<void> {
  const token = getToken();
  if (!token) {
    redirectToLogin();
    throw new ApiError(401, "Sessão expirada. Faça login novamente.");
  }
  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}/api/auth/master/trocar-senha`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify({ senhaAtual, novaSenha }),
    });
  } catch {
    throw new ApiError(0, "Não foi possível conectar à API. Verifique se o servidor está no ar.");
  }
  if (res.status === 401) throw new ApiError(401, "Senha atual incorreta.");
  if (!res.ok) throw new ApiError(res.status, await extractMessage(res));
}

// ---- Família ----
export interface Familia {
  id: string;
  nome: string;
  cicloFechamentoPadrao: Ciclo;
}
export const obterFamilia = () => get<Familia>("/api/familias");
export const salvarFamilia = (d: { nome: string; cicloFechamentoPadrao: Ciclo }) =>
  put<Familia>("/api/familias", d);

// ---- Filhos ----
export type Ciclo = "Semanal" | "Quinzenal" | "Mensal" | "Personalizado";
export const CICLOS: Ciclo[] = ["Semanal", "Quinzenal", "Mensal", "Personalizado"];

export interface Filho {
  id: string;
  nome: string;
  apelido: string | null;
  saldoDevedorAcumulado: number;
  cicloFechamento: Ciclo;
  mesadaBase: number;
  valorPonto: number | null;
}
export interface FilhoInput {
  nome: string;
  apelido: string | null;
  cicloFechamento: Ciclo;
  mesadaBase: number;
  valorPonto: number | null;
}

export const listarFilhos = () => get<Filho[]>("/api/filhos");
export const criarFilho = (d: FilhoInput) => post<Filho>("/api/filhos", d);
export const editarFilho = (id: string, d: FilhoInput) => put<Filho>(`/api/filhos/${id}`, d);
export const removerFilho = (id: string) => del(`/api/filhos/${id}`);

// ---- Execuções ----
export interface ExecucaoPendente {
  id: string;
  tarefaUsuarioId: string;
  dataExecucao: string;
  status: "Feito" | "Parcial" | "NaoFeito";
  percentualConclusao: number | null;
  statusAprovacao: "Pendente";
  valorCalculado: number | null;
  pontosCalculado: number | null;
}
export const listarPendentes = () => get<ExecucaoPendente[]>("/api/execucoes/pendentes");
export const aprovarExecucao = (id: string) => post<unknown>(`/api/execucoes/${id}/aprovar`);
export const rejeitarExecucao = (id: string) => post<unknown>(`/api/execucoes/${id}/rejeitar`);

// ---- Convites ----
export type ConviteStatus = "Pendente" | "Utilizado" | "Expirado" | "Revogado";
export interface Convite {
  id: string;
  codigo: string;
  papelAlvo: "Master" | "Comum";
  usuarioComumId: string | null;
  status: ConviteStatus;
  expiraEm: string;
}
export type ConviteInput =
  | { papelAlvo: "Comum"; usuarioComumId: string; nomeConvidado: null }
  | { papelAlvo: "Master"; usuarioComumId: null; nomeConvidado: string };
export const listarConvites = () => get<Convite[]>("/api/convites");
export const criarConvite = (d: ConviteInput) => post<Convite>("/api/convites", d);
export const revogarConvite = (id: string) => del(`/api/convites/${id}`);

// ---- Masters ----
export interface Master {
  id: string;
  nome: string;
  email: string;
  isFinanceiro: boolean;
  status: "ativo" | "inativo";
}
export const listarMasters = () => get<Master[]>("/api/masters");
export const desativarMaster = (id: string) => del(`/api/masters/${id}`);

// ---- Ciclos ----
export interface CicloFechado {
  id: string;
  usuarioComumId: string;
  dataInicio: string;
  dataFim: string;
  mesadaBase: number;
  somaBonus: number;
  somaMultas: number;
  saldoDevedorAnterior: number;
  valorFinal: number;
  saldoDevedorResultante: number;
}
export interface CicloAtual {
  dataInicio: string;
  dataFim: string;
  mesadaBase: number;
  somaBonus: number;
  somaMultas: number;
  saldoDevedorAnterior: number;
  valorFinalPrevisto: number;
  saldoDevedorPrevisto: number;
}
export const historicoCiclos = (uid: string) =>
  get<CicloFechado[]>(`/api/ciclos/historico?usuarioComumId=${encodeURIComponent(uid)}`);
export const cicloAtual = (uid: string) =>
  get<CicloAtual>(`/api/ciclos/atual?usuarioComumId=${encodeURIComponent(uid)}`);
export const fecharCiclo = (d: { usuarioComumId: string; dataInicio: string; dataFim: string }) =>
  post<CicloFechado>("/api/ciclos/fechar", d);

// ---- Categorias ----
export interface Categoria {
  id: string;
  parentId: string | null;
  nome: string;
  sistema: boolean;
}
export const listarCategorias = () => get<Categoria[]>("/api/categorias");
export const criarCategoria = (d: { nome: string; parentId: string | null }) =>
  post<Categoria>("/api/categorias", d);
export const editarCategoria = (id: string, nome: string) => put<Categoria>(`/api/categorias/${id}`, { nome });
export const removerCategoria = (id: string) => del(`/api/categorias/${id}`);

// ---- Tarefas ----
export type ModoCalculo = "ValorDireto" | "Pontos";
export type TipoTarefa = "Recorrente" | "Avulsa";
export type Natureza = "Bonus" | "Obrigatoria";
export type TipoEvidencia = "Nenhuma" | "Foto" | "IntegracaoAutomatica";
export type Provedor = "Strava" | "GoogleFit" | "AppleHealth";

export interface TarefaInput {
  categoriaId: string;
  nome: string;
  descricao: string | null;
  modoCalculo: ModoCalculo;
  valor: number | null;
  pontos: number | null;
  permiteParcial: boolean;
  tipo: TipoTarefa;
  validadeAvulsa: string | null;
  natureza: Natureza;
  valorMulta: number | null;
  requerAprovacao: boolean;
  tipoEvidencia: TipoEvidencia;
  provedorIntegracao: Provedor | null;
}
export interface Tarefa extends TarefaInput {
  id: string;
}
export const listarTarefas = () => get<Tarefa[]>("/api/tarefas");
export const criarTarefa = (d: TarefaInput) => post<Tarefa>("/api/tarefas", d);
export const editarTarefa = (id: string, d: TarefaInput) => put<Tarefa>(`/api/tarefas/${id}`, d);
export const removerTarefa = (id: string) => del(`/api/tarefas/${id}`);
export const aderirTarefa = (
  id: string,
  d: { usuarioComumId: string; valorOverride: number | null; pontosOverride: number | null },
) => post<unknown>(`/api/tarefas/${id}/aderencias`, d);
export const desaderirTarefa = (id: string, uid: string) => del(`/api/tarefas/${id}/aderencias/${uid}`);
export const sugerirValor = (uid: string, modo: ModoCalculo) =>
  get<{ valor: number; modoCalculo: ModoCalculo }>(
    `/api/tarefas/sugestao?usuarioComumId=${encodeURIComponent(uid)}&modoCalculo=${modo}`,
  );

// ---- Destinatários de Notificação ----
export type TipoDestinatario = "Email" | "Telefone";
export interface Destinatario {
  id: string;
  tipo: TipoDestinatario;
  valor: string;
  ativo: boolean;
}
export const listarDestinatarios = () => get<Destinatario[]>("/api/notificacoes/destinatarios");
export const criarDestinatario = (d: { tipo: TipoDestinatario; valor: string }) =>
  post<Destinatario>("/api/notificacoes/destinatarios", d);
export const atualizarDestinatario = (id: string, ativo: boolean) =>
  put<unknown>(`/api/notificacoes/destinatarios/${id}`, { ativo });
export const removerDestinatario = (id: string) => del(`/api/notificacoes/destinatarios/${id}`);

// ---- Utils ----
export const formatBRL = (value: number) =>
  new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);

export const formatDate = (iso: string) => {
  const d = new Date(iso.length === 10 ? `${iso}T00:00:00` : iso);
  return Number.isNaN(d.getTime()) ? iso : d.toLocaleDateString("pt-BR");
};

export const errMsg = (e: unknown) => (e instanceof Error ? e.message : "Erro inesperado.");

export const parseNum = (s: string): number | null => {
  const t = s.trim().replace(",", ".");
  if (!t) return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
};
