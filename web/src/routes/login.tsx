import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { Eye, EyeOff, Loader2, ShieldCheck, HeartHandshake, LineChart, Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Brand } from "@/components/app/AppShell";
import { useTheme } from "@/lib/theme";
import { ApiError, getToken, loginMaster, setToken } from "@/lib/api";

export const Route = createFileRoute("/login")({
  head: () => ({
    meta: [
      { title: "Entrar — MesadApp" },
      { name: "description", content: "Acesse o painel do responsável do MesadApp." },
      { property: "og:title", content: "Entrar — MesadApp" },
      { property: "og:description", content: "Acesse o painel do responsável do MesadApp." },
    ],
  }),
  component: LoginPage,
});

function ThemeToggle() {
  const { theme, toggle } = useTheme();
  return (
    <Button
      variant="outline"
      size="icon"
      onClick={toggle}
      aria-label="Alternar tema"
      className="absolute right-4 top-4 z-10 rounded-full"
    >
      {theme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
    </Button>
  );
}

function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [show, setShow] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (getToken()) navigate({ to: "/dashboard", replace: true });
  }, [navigate]);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const { token } = await loginMaster(email.trim(), senha);
      setToken(token);
      navigate({ to: "/dashboard", replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setError("E-mail ou senha incorretos. Confira os dados e tente novamente.");
      } else if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("Algo deu errado. Tente novamente em instantes.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative grid min-h-screen lg:grid-cols-[1.1fr_1fr]">
      <ThemeToggle />
      <aside className="relative hidden overflow-hidden bg-hero-gradient p-12 text-sidebar-foreground lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 bg-dots opacity-60" />
        <div className="relative">
          <Brand />
        </div>
        <div className="relative max-w-md">
          <h1 className="text-4xl font-black leading-tight">
            Mesada com propósito, <span className="text-sidebar-primary">sem dor de cabeça</span>.
          </h1>
          <p className="mt-4 text-base text-sidebar-foreground/75">
            Acompanhe tarefas, aprove conquistas e ensine seus filhos a cuidar do próprio dinheiro —
            tudo em um painel feito para o responsável.
          </p>
          <ul className="mt-8 space-y-3 text-sm">
            {[
              { icon: ShieldCheck, text: "Dados da família isolados e protegidos" },
              { icon: HeartHandshake, text: "Aprovações rápidas, combinados claros" },
              { icon: LineChart, text: "Evolução de cada filho ao longo do tempo" },
            ].map(({ icon: Icon, text }) => (
              <li key={text} className="flex items-center gap-3">
                <span className="grid size-8 place-items-center rounded-lg bg-sidebar-accent">
                  <Icon className="size-4 text-sidebar-primary" />
                </span>
                {text}
              </li>
            ))}
          </ul>
        </div>
        <p className="relative text-xs text-sidebar-foreground/50">© 2026 MesadApp</p>
      </aside>

      <main className="flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-sm">
          <div className="mb-8 lg:hidden">
            <div className="inline-flex rounded-2xl bg-sidebar p-3">
              <Brand />
            </div>
          </div>
          <h2 className="text-2xl font-extrabold tracking-tight">Entrar no painel</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Use o e-mail e a senha do responsável pela família.
          </p>

          <form onSubmit={onSubmit} className="mt-8 space-y-5">
            <div className="space-y-2">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="voce@exemplo.com"
                className="h-11"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="senha">Senha</Label>
              <div className="relative">
                <Input
                  id="senha"
                  type={show ? "text" : "password"}
                  autoComplete="current-password"
                  required
                  value={senha}
                  onChange={(e) => setSenha(e.target.value)}
                  placeholder="••••••••"
                  className="h-11 pr-11"
                />
                <button
                  type="button"
                  onClick={() => setShow((s) => !s)}
                  aria-label={show ? "Ocultar senha" : "Mostrar senha"}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  {show ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
                </button>
              </div>
            </div>

            {error && (
              <Alert variant="destructive">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <Button type="submit" className="h-11 w-full text-base font-semibold" disabled={loading}>
              {loading && <Loader2 className="size-4 animate-spin" />}
              {loading ? "Entrando…" : "Entrar"}
            </Button>
          </form>
          <p className="mt-4 text-center text-sm">
            <Link to="/esqueci-senha" className="font-medium text-primary hover:underline">
              Esqueci minha senha
            </Link>
          </p>

          <p className="mt-8 text-center text-sm text-muted-foreground">
            Ainda não tem conta?{" "}
            <Link to="/cadastro" className="font-semibold text-primary hover:underline">
              Criar conta
            </Link>
          </p>
          <p className="mt-3 text-center text-xs text-muted-foreground">
            Acesso exclusivo para o responsável (perfil Master).
          </p>
        </div>
      </main>
    </div>
  );
}
