import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { adminTrocarSenha, setMustChangePassword } from "@/lib/admin-api";
import { errMsg } from "@/lib/api";

export function TrocarSenhaForm({ onDone }: { onDone: () => void }) {
  const [atual, setAtual] = useState("");
  const [nova, setNova] = useState("");
  const [conf, setConf] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    if (nova.length < 8) return setError("A nova senha precisa ter pelo menos 8 caracteres.");
    if (nova !== conf) return setError("A confirmação não confere com a nova senha.");
    setLoading(true);
    try {
      await adminTrocarSenha(atual, nova);
      setMustChangePassword(false);
      setAtual(""); setNova(""); setConf("");
      onDone();
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="s-atual">Senha atual</Label>
        <Input id="s-atual" type="password" required autoComplete="current-password" value={atual} onChange={(e) => setAtual(e.target.value)} />
      </div>
      <div className="space-y-2">
        <Label htmlFor="s-nova">Nova senha (mín. 8 caracteres)</Label>
        <Input id="s-nova" type="password" required autoComplete="new-password" value={nova} onChange={(e) => setNova(e.target.value)} />
      </div>
      <div className="space-y-2">
        <Label htmlFor="s-conf">Confirmar nova senha</Label>
        <Input id="s-conf" type="password" required autoComplete="new-password" value={conf} onChange={(e) => setConf(e.target.value)} />
      </div>
      {error && <Alert variant="destructive"><AlertDescription>{error}</AlertDescription></Alert>}
      <Button type="submit" disabled={loading}>
        {loading && <Loader2 className="size-4 animate-spin" />} {loading ? "Salvando…" : "Trocar senha"}
      </Button>
    </form>
  );
}
