// Cliente do Painel Administrativo — sessão totalmente separada da sessão Master.
import { API_BASE_URL, ApiError, extractMessage } from "@/lib/api";

const TOKEN_KEY = "mesada.admin.token";
const TROCA_KEY = "mesada.admin.deveTrocarSenha";

export const getAdminToken = () =>
  typeof window === "undefined" ? null : window.localStorage.getItem(TOKEN_KEY);
export const mustChangePassword = () =>
  typeof window !== "undefined" && window.localStorage.getItem(TROCA_KEY) === "1";
export const setMustChangePassword = (v: boolean) =>
  v ? window.localStorage.setItem(TROCA_KEY, "1") : window.localStorage.removeItem(TROCA_KEY);

export function setAdminSession(token: string, deveTrocarSenha: boolean) {
  window.localStorage.setItem(TOKEN_KEY, token);
  setMustChangePassword(deveTrocarSenha);
}
export function clearAdminSession() {
  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(TROCA_KEY);
}

function toLogin() {
  clearAdminSession();
  if (typeof window !== "undefined" && window.location.pathname !== "/admin/login") {
    window.location.assign("/admin/login");
  }
}

async function request<T>(path: string, init: RequestInit = {}, opts: { auth?: boolean; redirect401?: boolean } = {}): Promise<T> {
  const auth = opts.auth ?? true;
  const headers = new Headers(init.headers);
  headers.set("Content-Type", "application/json");
  if (auth) {
    const t = getAdminToken();
    if (!t) {
      toLogin();
      throw new ApiError(401, "Sessão expirada. Faça login novamente.");
    }
    headers.set("Authorization", `Bearer ${t}`);
  }
  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}${path}`, { ...init, headers });
  } catch {
    throw new ApiError(0, "Não foi possível conectar à API. Verifique se o servidor está no ar.");
  }
  if (res.status === 401) {
    if (auth && (opts.redirect401 ?? true)) {
      toLogin();
      throw new ApiError(401, "Sessão expirada. Faça login novamente.");
    }
    throw new ApiError(401, auth ? "Senha atual incorreta." : "E-mail ou senha incorretos.");
  }
  if (res.status === 403) throw new ApiError(403, "Ação exclusiva do grupo Owner.");
  if (!res.ok) throw new ApiError(res.status, await extractMessage(res));
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

const body = (method: string, b?: unknown): RequestInit => ({ method, body: b === undefined ? null : JSON.stringify(b) });

// ---- Auth ----
export const adminLogin = (email: string, senha: string) =>
  request<{ token: string; deveTrocarSenha: boolean }>("/api/admin/auth/login", body("POST", { email, senha }), { auth: false });
export const adminTrocarSenha = (senhaAtual: string, novaSenha: string) =>
  request<void>("/api/admin/auth/trocar-senha", body("POST", { senhaAtual, novaSenha }), { redirect401: false });
export const adminTrocarEmail = (novoEmail: string) => request<void>("/api/admin/auth/email", body("PUT", { novoEmail }));

// ---- Administradores ----
export interface Administrador {
  id: string;
  nome: string;
  email: string;
  status: string;
  deveTrocarSenha: boolean;
  grupoId: string;
  grupoNome: string;
}
export const listarAdmins = () => request<Administrador[]>("/api/admin/administradores");
export const convidarAdmin = (d: { nome: string; email: string; grupoId: string }) =>
  request<{ administrador: Administrador; senhaTemporaria: string }>("/api/admin/administradores", body("POST", d));
export const editarAdmin = (id: string, d: { nome: string; grupoId: string }) =>
  request<Administrador>(`/api/admin/administradores/${id}`, body("PUT", d));
export const desativarAdmin = (id: string) => request<void>(`/api/admin/administradores/${id}`, body("DELETE"));

// ---- Grupos ----
export interface Grupo {
  id: string;
  nome: string;
  descricao: string | null;
  permissoes: Record<string, boolean>;
  sistema: boolean;
}
export type GrupoInput = { nome: string; descricao: string | null; permissoes: Record<string, boolean> };
export const listarGrupos = () => request<Grupo[]>("/api/admin/grupos");
export const criarGrupo = (d: GrupoInput) => request<Grupo>("/api/admin/grupos", body("POST", d));
export const editarGrupo = (id: string, d: GrupoInput) => request<Grupo>(`/api/admin/grupos/${id}`, body("PUT", d));
export const removerGrupo = (id: string) => request<void>(`/api/admin/grupos/${id}`, body("DELETE"));

// ---- Identidade do admin logado (a partir do JWT) ----
export function adminClaims(): Record<string, unknown> {
  const t = getAdminToken();
  const part = t?.split(".")[1];
  if (!part) return {};
  try {
    const json = atob(part.replace(/-/g, "+").replace(/_/g, "/").padEnd(Math.ceil(part.length / 4) * 4, "="));
    return JSON.parse(decodeURIComponent(escape(json))) as Record<string, unknown>;
  } catch {
    return {};
  }
}

const CLAIM_ID = ["sub", "nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "id", "adminId"];
const CLAIM_EMAIL = ["email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "unique_name"];

export function findMe(admins: Administrador[]): Administrador | undefined {
  const c = adminClaims();
  const ids = CLAIM_ID.map((k) => c[k]).filter((v): v is string => typeof v === "string");
  const emails = CLAIM_EMAIL.map((k) => c[k]).filter((v): v is string => typeof v === "string").map((e) => e.toLowerCase());
  return admins.find((a) => ids.includes(a.id)) ?? admins.find((a) => emails.includes(a.email.toLowerCase()));
}
