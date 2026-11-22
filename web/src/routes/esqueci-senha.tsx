import { createFileRoute, Link } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { CheckCircle2, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AuthSplit } from "@/components/app/AuthSplit";
import { ApiError, esqueciSenha } from "@/lib/api";

export const Route = createFileRoute("/esqueci-senha")({
  head: () => ({
    meta: [
      { title: "Esqueci minha senha — MesadApp" },
      { name: "description", content: "Receba por e-mail um link para redefinir a senha do responsável." },
      { property: "og:title", content: "Esqueci minha senha — MesadApp" },
      { property: "og:description", content: "Receba por e-mail um link para redefinir a senha do responsável." },
    ],
  }),
  component: Page,
});

function Page() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await esqueciSenha(email.trim());
      setDone(true);
    } catch (err) {
      // Só mostramos erro de conexão; qualquer resposta da API vira a mensagem genérica.
      if (err instanceof ApiError && err.status === 0) setError(err.message);
      else setDone(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthSplit title="Esqueci minha senha" subtitle="Informe seu e-mail e enviaremos um link para criar uma nova senha.">
      {done ? (
        <Alert className="mt-8">
          <CheckCircle2 className="size-4" />
          <AlertDescription>Se esse e-mail existir na nossa base, enviamos um link de redefinição.</AlertDescription>
        </Alert>
      ) : (
        <form onSubmit={onSubmit} className="mt-8 space-y-5">
          <div className="space-y-2">
            <Label htmlFor="email">E-mail</Label>
            <Input id="email" type="email" autoComplete="email" required value={email}
              onChange={(e) => setEmail(e.target.value)} placeholder="voce@exemplo.com" className="h-11" />
          </div>
          {error && (
            <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>
          )}
          <Button type="submit" className="h-11 w-full text-base font-semibold" disabled={loading}>
            {loading && <Loader2 className="size-4 animate-spin" />}
            {loading ? "Enviando…" : "Enviar link"}
          </Button>
        </form>
      )}
      <p className="mt-8 text-center text-sm text-muted-foreground">
        <Link to="/login" className="font-semibold text-primary hover:underline">Voltar para o login</Link>
      </p>
    </AuthSplit>
  );
}
