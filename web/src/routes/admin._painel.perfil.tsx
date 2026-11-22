import { createFileRoute } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { PageHeader } from "@/components/app/PageHeader";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { TrocarSenhaForm } from "@/components/admin/TrocarSenhaForm";
import { useAdminMe } from "@/components/admin/useAdminMe";
import { adminTrocarEmail } from "@/lib/admin-api";
import { ApiError, errMsg } from "@/lib/api";

export const Route = createFileRoute("/admin/_painel/perfil")({
  head: () => ({
    meta: [
      { title: "Meu perfil — Administração MesadApp" },
      { name: "description", content: "Altere seu e-mail e sua senha de administrador." },
      { property: "og:title", content: "Meu perfil — Administração MesadApp" },
      { property: "og:description", content: "Altere seu e-mail e sua senha de administrador." },
      { name: "robots", content: "noindex" },
    ],
  }),
  component: PerfilPage,
});

function PerfilPage() {
  const qc = useQueryClient();
  const { me } = useAdminMe();
  const [email, setEmail] = useState("");
  const [saving, setSaving] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  const saveEmail = async (e: FormEvent) => {
    e.preventDefault();
    setErr(null);
    setSaving(true);
    try {
      await adminTrocarEmail(email.trim());
      toast.success("E-mail atualizado.");
      setEmail("");
      qc.invalidateQueries({ queryKey: ["admin", "administradores"] });
    } catch (x) {
      setErr(x instanceof ApiError && x.status === 409 ? "Este e-mail já está em uso por outro administrador." : errMsg(x));
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <PageHeader title="Meu perfil" description={me ? `${me.nome} · ${me.grupoNome}` : "Seus dados de acesso."} />
      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-2xl border bg-card p-6 shadow-soft">
          <h2 className="text-lg font-extrabold">Trocar e-mail</h2>
          {me && <p className="mt-1 text-sm text-muted-foreground">Atual: {me.email}</p>}
          <form onSubmit={saveEmail} className="mt-4 space-y-4">
            <div className="space-y-2">
              <Label htmlFor="p-email">Novo e-mail</Label>
              <Input id="p-email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
            </div>
            {err && <p className="text-sm font-medium text-destructive">{err}</p>}
            <Button type="submit" disabled={saving}>{saving ? "Salvando…" : "Salvar e-mail"}</Button>
          </form>
        </section>
        <section className="rounded-2xl border bg-card p-6 shadow-soft">
          <h2 className="mb-4 text-lg font-extrabold">Trocar senha</h2>
          <TrocarSenhaForm onDone={() => toast.success("Senha alterada.")} />
        </section>
      </div>
    </>
  );
}
