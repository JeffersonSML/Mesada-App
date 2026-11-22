import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AuthSplit } from "@/components/app/AuthSplit";
import { errMsg, redefinirSenha } from "@/lib/api";

export const Route = createFileRoute("/redefinir-senha")({
  validateSearch: (s: Record<string, unknown>): { token?: string } =>
    typeof s["token"] === "string" && s["token"] ? { token: s["token"] } : {},
  head: () => ({
    meta: [
      { title: "Redefinir senha — MesadApp" },
      { name: "description", content: "Crie uma nova senha para acessar o painel do responsável." },
      { property: "og:title", content: "Redefinir senha — MesadApp" },
      { property: "og:description", content: "Crie uma nova senha para acessar o painel do responsável." },
    ],
  }),
  component: Page,
});

function Page() {
  const { token } = Route.useSearch();
  const navigate = useNavigate();
  const [senha, setSenha] = useState("");
  const [confirma, setConfirma] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!token) return;
    if (senha.length < 8) return setError("A senha precisa ter pelo menos 8 caracteres.");
    if (senha !== confirma) return setError("As senhas não coincidem.");
    setLoading(true);
    try {
      await redefinirSenha(token, senha);
      toast.success("Senha redefinida! Entre com a nova senha.");
      navigate({ to: "/login", replace: true });
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthSplit title="Redefinir senha" subtitle="Escolha uma nova senha para a sua conta.">
      {!token ? (
        <Alert variant="destructive" className="mt-8">
          <AlertDescription>Link inválido. Peça um novo link de redefinição.</AlertDescription>
        </Alert>
      ) : (
        <form onSubmit={onSubmit} className="mt-8 space-y-5">
          <div className="space-y-2">
            <Label htmlFor="senha">Nova senha</Label>
            <Input id="senha" type="password" autoComplete="new-password" required minLength={8}
              value={senha} onChange={(e) => setSenha(e.target.value)} placeholder="Mínimo de 8 caracteres" className="h-11" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="confirma">Confirmar nova senha</Label>
            <Input id="confirma" type="password" autoComplete="new-password" required minLength={8}
              value={confirma} onChange={(e) => setConfirma(e.target.value)} placeholder="Repita a senha" className="h-11" />
          </div>
          {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
          <Button type="submit" className="h-11 w-full text-base font-semibold" disabled={loading}>
            {loading && <Loader2 className="size-4 animate-spin" />}
            {loading ? "Salvando…" : "Redefinir senha"}
          </Button>
        </form>
      )}
      <p className="mt-8 text-center text-sm text-muted-foreground">
        <Link to="/esqueci-senha" className="font-semibold text-primary hover:underline">Pedir novo link</Link>
        {" · "}
        <Link to="/login" className="font-semibold text-primary hover:underline">Voltar para o login</Link>
      </p>
    </AuthSplit>
  );
}
