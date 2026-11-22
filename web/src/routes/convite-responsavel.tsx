import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AuthSplit } from "@/components/app/AuthSplit";
import { ApiError, errMsg, resgatarConviteMaster, setToken } from "@/lib/api";

export const Route = createFileRoute("/convite-responsavel")({
  validateSearch: (s: Record<string, unknown>): { codigo?: string } =>
    typeof s["codigo"] === "string" && s["codigo"] ? { codigo: s["codigo"] } : {},
  head: () => ({
    meta: [
      { title: "Aceitar convite de responsável — MesadApp" },
      { name: "description", content: "Use o código do convite para entrar como responsável da família no MesadApp." },
      { property: "og:title", content: "Aceitar convite de responsável — MesadApp" },
      { property: "og:description", content: "Use o código do convite para entrar como responsável da família no MesadApp." },
    ],
  }),
  component: Page,
});

function Page() {
  const search = Route.useSearch();
  const navigate = useNavigate();
  const [codigo, setCodigo] = useState(search.codigo ?? "");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [confirma, setConfirma] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [duplicado, setDuplicado] = useState(false);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setDuplicado(false);
    if (senha.length < 8) return setError("A senha precisa ter pelo menos 8 caracteres.");
    if (senha !== confirma) return setError("As senhas não coincidem.");
    setLoading(true);
    try {
      const { token } = await resgatarConviteMaster(codigo.trim(), email.trim(), senha);
      setToken(token);
      navigate({ to: "/dashboard", replace: true });
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setDuplicado(true);
        setError("Este e-mail já está cadastrado.");
      } else if (err instanceof ApiError && err.status === 400) {
        setError(err.message || "Convite inválido ou expirado. Confira o código.");
      } else setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthSplit title="Aceitar convite" subtitle="Você foi convidado(a) como responsável de uma família. Crie seu acesso.">
      <form onSubmit={onSubmit} className="mt-8 space-y-5">
        <div className="space-y-2">
          <Label htmlFor="codigo">Código do convite</Label>
          <Input id="codigo" required value={codigo} onChange={(e) => setCodigo(e.target.value)}
            placeholder="Ex.: AB12CD" className="h-11 font-mono tracking-wider" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="email">E-mail</Label>
          <Input id="email" type="email" autoComplete="email" required value={email}
            onChange={(e) => setEmail(e.target.value)} placeholder="voce@exemplo.com" className="h-11" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="senha">Senha</Label>
          <Input id="senha" type="password" autoComplete="new-password" required minLength={8}
            value={senha} onChange={(e) => setSenha(e.target.value)} placeholder="Mínimo de 8 caracteres" className="h-11" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="confirma">Confirmar senha</Label>
          <Input id="confirma" type="password" autoComplete="new-password" required minLength={8}
            value={confirma} onChange={(e) => setConfirma(e.target.value)} placeholder="Repita a senha" className="h-11" />
        </div>
        {error && (
          <Alert variant="destructive">
            <AlertDescription>
              {error}{" "}
              {duplicado && <Link to="/login" className="font-semibold underline">Ir para o login</Link>}
            </AlertDescription>
          </Alert>
        )}
        <Button type="submit" className="h-11 w-full text-base font-semibold" disabled={loading}>
          {loading && <Loader2 className="size-4 animate-spin" />}
          {loading ? "Entrando…" : "Aceitar convite"}
        </Button>
      </form>
      <p className="mt-8 text-center text-sm text-muted-foreground">
        Já tem conta?{" "}
        <Link to="/login" className="font-semibold text-primary hover:underline">Entrar</Link>
      </p>
    </AuthSplit>
  );
}
