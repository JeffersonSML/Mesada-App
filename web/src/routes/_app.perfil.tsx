import { createFileRoute } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { errMsg, trocarSenhaMaster } from "@/lib/api";

export const Route = createFileRoute("/_app/perfil")({
  head: () => ({
    meta: [
      { title: "Meu Perfil — MesadApp" },
      { name: "description", content: "Troque a senha da sua conta de responsável." },
      { property: "og:title", content: "Meu Perfil — MesadApp" },
      { property: "og:description", content: "Troque a senha da sua conta de responsável." },
    ],
  }),
  component: PerfilPage,
});

function PerfilPage() {
  const [atual, setAtual] = useState("");
  const [nova, setNova] = useState("");
  const [confirma, setConfirma] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    if (nova.length < 8) return setError("A nova senha precisa ter pelo menos 8 caracteres.");
    if (nova !== confirma) return setError("As senhas não coincidem.");
    setLoading(true);
    try {
      await trocarSenhaMaster(atual, nova);
      toast.success("Senha alterada com sucesso.");
      setAtual(""); setNova(""); setConfirma("");
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader title="Meu Perfil" description="Gerencie o acesso à sua conta." />
      <form onSubmit={onSubmit} className="max-w-xl space-y-5 rounded-2xl border bg-card p-6 shadow-soft">
        <h2 className="text-lg font-extrabold">Trocar senha</h2>
        <Field label="Senha atual" htmlFor="atual">
          <Input id="atual" type="password" autoComplete="current-password" required value={atual} onChange={(e) => setAtual(e.target.value)} />
        </Field>
        <Field label="Nova senha" htmlFor="nova" hint="Mínimo de 8 caracteres.">
          <Input id="nova" type="password" autoComplete="new-password" required minLength={8} value={nova} onChange={(e) => setNova(e.target.value)} />
        </Field>
        <Field label="Confirmar nova senha" htmlFor="confirma">
          <Input id="confirma" type="password" autoComplete="new-password" required minLength={8} value={confirma} onChange={(e) => setConfirma(e.target.value)} />
        </Field>
        {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
        <Button type="submit" disabled={loading}>
          {loading && <Loader2 className="size-4 animate-spin" />} Trocar senha
        </Button>
      </form>
    </>
  );
}
