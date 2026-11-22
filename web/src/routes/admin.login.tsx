import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AdminBrand } from "@/components/admin/AdminShell";
import { adminLogin, getAdminToken, mustChangePassword, setAdminSession } from "@/lib/admin-api";
import { errMsg } from "@/lib/api";

export const Route = createFileRoute("/admin/login")({
  head: () => ({
    meta: [
      { title: "Acesso administrativo — MesadApp" },
      { name: "description", content: "Entrada restrita à equipe interna do MesadApp." },
      { property: "og:title", content: "Acesso administrativo — MesadApp" },
      { property: "og:description", content: "Entrada restrita à equipe interna do MesadApp." },
      { name: "robots", content: "noindex" },
    ],
  }),
  component: AdminLoginPage,
});

function AdminLoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (getAdminToken()) navigate({ to: mustChangePassword() ? "/admin/trocar-senha" : "/admin/administradores", replace: true });
  }, [navigate]);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const r = await adminLogin(email.trim(), senha);
      setAdminSession(r.token, !!r.deveTrocarSenha);
      navigate({ to: r.deveTrocarSenha ? "/admin/trocar-senha" : "/admin/administradores", replace: true });
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="grid min-h-screen place-items-center bg-sidebar px-6">
      <div className="w-full max-w-sm rounded-2xl border bg-card p-8 shadow-lift">
        <div className="mb-6 inline-flex rounded-xl bg-sidebar p-3"><AdminBrand /></div>
        <h1 className="text-2xl font-extrabold">Acesso administrativo</h1>
        <p className="mt-1 text-sm text-muted-foreground">Área restrita à equipe interna.</p>
        <form onSubmit={onSubmit} className="mt-6 space-y-4">
          <div className="space-y-2">
            <Label htmlFor="a-email">E-mail</Label>
            <Input id="a-email" type="email" required autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>
          <div className="space-y-2">
            <Label htmlFor="a-senha">Senha</Label>
            <Input id="a-senha" type="password" required autoComplete="current-password" value={senha} onChange={(e) => setSenha(e.target.value)} />
          </div>
          {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
          <Button type="submit" className="h-11 w-full" disabled={loading}>
            {loading && <Loader2 className="size-4 animate-spin" />} {loading ? "Entrando…" : "Entrar"}
          </Button>
        </form>
      </div>
    </div>
  );
}
