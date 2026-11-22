import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { Eye, EyeOff, Loader2, ShieldCheck, HeartHandshake, LineChart, Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Brand } from "@/components/app/AppShell";
import { ApiError, cadastrarFamilia, getToken, setToken } from "@/lib/api";
import { useTheme } from "@/lib/theme";

export const Route = createFileRoute("/cadastro")({
  head: () => ({
    meta: [
      { title: "Criar conta — MesadApp" },
      { name: "description", content: "Cadastre sua família no MesadApp e comece a organizar a mesada dos seus filhos." },
      { property: "og:title", content: "Criar conta — MesadApp" },
      { property: "og:description", content: "Cadastre sua família no MesadApp e comece a organizar a mesada dos seus filhos." },
    ],
  }),
  component: CadastroPage,
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

function CadastroPage() {
  const navigate = useNavigate();
  const [nomeFamilia, setNomeFamilia] = useState("");
  const [nomeMaster, setNomeMaster] = useState("");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [confirma, setConfirma] = useState("");
  const [show, setShow] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (getToken()) navigate({ to: "/dashboard", replace: true });
  }, [navigate]);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    if (senha.length < 8) {
      setError("A senha precisa ter pelo menos 8 caracteres.");
      return;
    }
    if (senha !== confirma) {
      setError("As senhas não coincidem. Digite a mesma senha nos dois campos.");
      return;
    }
    setLoading(true);
    try {
      const { token } = await cadastrarFamilia({
        nomeFamilia: nomeFamilia.trim(),
        nomeMaster: nomeMaster.trim(),
        emailMaster: email.trim(),
        senhaMaster: senha,
      });
      setToken(token);
      navigate({ to: "/dashboard", replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setError("Este e-mail já está cadastrado. Tente entrar na sua conta.");
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
          <h2 className="text-2xl font-extrabold tracking-tight">Criar conta da família</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Cadastre sua família e comece a organizar a mesada em poucos minutos.
          </p>

          <form onSubmit={onSubmit} className="mt-8 space-y-5">
            <div className="space-y-2">
              <Label htmlFor="nomeFamilia">Nome da família</Label>
              <Input
                id="nomeFamilia"
                required
                maxLength={100}
                value={nomeFamilia}
                onChange={(e) => setNomeFamilia(e.target.value)}
                placeholder="Família Silva"
                className="h-11"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="nomeMaster">Seu nome (responsável)</Label>
              <Input
                id="nomeMaster"
                required
                maxLength={100}
                autoComplete="name"
                value={nomeMaster}
                onChange={(e) => setNomeMaster(e.target.value)}
                placeholder="Maria Silva"
                className="h-11"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">E-mail do responsável</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                required
                maxLength={255}
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
                  autoComplete="new-password"
                  required
                  minLength={8}
                  value={senha}
                  onChange={(e) => setSenha(e.target.value)}
                  placeholder="Mínimo de 8 caracteres"
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
            <div className="space-y-2">
              <Label htmlFor="confirma">Confirmar senha</Label>
              <Input
                id="confirma"
                type={show ? "text" : "password"}
                autoComplete="new-password"
                required
                minLength={8}
                value={confirma}
                onChange={(e) => setConfirma(e.target.value)}
                placeholder="Repita a senha"
                className="h-11"
              />
            </div>

            {error && (
              <Alert variant="destructive">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <Button type="submit" className="h-11 w-full text-base font-semibold" disabled={loading}>
              {loading && <Loader2 className="size-4 animate-spin" />}
              {loading ? "Criando conta…" : "Criar conta"}
            </Button>
          </form>

          <p className="mt-8 text-center text-sm text-muted-foreground">
            Já tem conta?{" "}
            <Link to="/login" className="font-semibold text-primary hover:underline">
              Entrar
            </Link>
          </p>
        </div>
      </main>
    </div>
  );
}
