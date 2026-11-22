import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Bell, Loader2, Mail, Phone, Plus, Trash2 } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import {
  criarDestinatario,
  atualizarDestinatario,
  errMsg,
  listarDestinatarios,
  removerDestinatario,
  type Destinatario,
  type TipoDestinatario,
} from "@/lib/api";

export const Route = createFileRoute("/_app/notificacoes")({
  head: () => ({
    meta: [
      { title: "Notificações — MesadApp" },
      { name: "description", content: "Gerencie os e-mails e telefones que recebem os alertas da família." },
      { property: "og:title", content: "Notificações — MesadApp" },
      { property: "og:description", content: "Gerencie os e-mails e telefones que recebem os alertas da família." },
    ],
  }),
  component: NotificacoesPage,
});

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const E164_RE = /^\+[1-9]\d{6,14}$/;

function validar(tipo: TipoDestinatario, valor: string): string | null {
  if (tipo === "Email" && !EMAIL_RE.test(valor)) {
    return "Informe um e-mail válido, por exemplo: voce@exemplo.com";
  }
  if (tipo === "Telefone" && !E164_RE.test(valor)) {
    return "Informe o telefone no formato internacional (E.164), por exemplo: +5511999999999";
  }
  return null;
}

function NotificacoesPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["destinatarios"], queryFn: listarDestinatarios });
  const [novo, setNovo] = useState(false);
  const [rm, setRm] = useState<Destinatario | null>(null);

  const toggle = useMutation({
    mutationFn: ({ id, ativo }: { id: string; ativo: boolean }) => atualizarDestinatario(id, ativo),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["destinatarios"] }),
    onError: (e) => toast.error(errMsg(e)),
  });

  const del = useMutation({
    mutationFn: (id: string) => removerDestinatario(id),
    onSuccess: () => {
      toast.success("Destinatário removido.");
      setRm(null);
      qc.invalidateQueries({ queryKey: ["destinatarios"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  const lista = q.data ?? [];

  return (
    <div>
      <PageHeader
        title="Notificações"
        description="E-mails e telefones extras que recebem os alertas da família, além do cadastro principal do responsável."
        actions={
          <Button onClick={() => setNovo(true)}>
            <Plus className="size-4" /> Adicionar destinatário
          </Button>
        }
      />

      {q.isError && <ErrorAlert title="Não foi possível carregar os destinatários" error={q.error} />}

      {q.isLoading ? (
        <div className="space-y-3">
          {[0, 1, 2].map((i) => (
            <Skeleton key={i} className="h-16 w-full rounded-xl" />
          ))}
        </div>
      ) : lista.length === 0 ? (
        <EmptyState
          icon={Bell}
          title="Nenhum destinatário cadastrado"
          text="Adicione e-mails ou telefones que também devem receber os alertas da família."
          action={
            <Button onClick={() => setNovo(true)}>
              <Plus className="size-4" /> Adicionar destinatário
            </Button>
          }
        />
      ) : (
        <div className="overflow-hidden rounded-2xl border bg-card">
          {lista.map((d) => (
            <div key={d.id} className="flex items-center gap-3 border-b px-5 py-3.5 last:border-b-0">
              <span className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary-soft text-primary">
                {d.tipo === "Email" ? <Mail className="size-4" /> : <Phone className="size-4" />}
              </span>
              <div className="min-w-0 flex-1">
                <div className="truncate font-semibold">{d.valor}</div>
                <div className="text-xs text-muted-foreground">
                  {d.tipo === "Email" ? "E-mail" : "Telefone"}
                </div>
              </div>
              <Badge variant={d.ativo ? "default" : "outline"}>{d.ativo ? "Ativo" : "Inativo"}</Badge>
              <Switch
                checked={d.ativo}
                disabled={toggle.isPending}
                onCheckedChange={(ativo) => toggle.mutate({ id: d.id, ativo })}
                aria-label={d.ativo ? "Desativar destinatário" : "Ativar destinatário"}
              />
              <Button size="icon" variant="ghost" aria-label="Remover" onClick={() => setRm(d)}>
                <Trash2 className="size-4 text-destructive" />
              </Button>
            </div>
          ))}
        </div>
      )}

      <NovoDestinatarioDialog open={novo} onOpenChange={setNovo} />

      <ConfirmDialog
        open={!!rm}
        onOpenChange={(o) => !o && setRm(null)}
        title="Remover destinatário"
        description={
          rm
            ? `Remover ${rm.tipo === "Email" ? "o e-mail" : "o telefone"} ${rm.valor}? Ele deixará de receber os alertas da família.`
            : ""
        }
        confirmLabel="Remover"
        pending={del.isPending}
        onConfirm={() => rm && del.mutate(rm.id)}
      />
    </div>
  );
}

function NovoDestinatarioDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (o: boolean) => void }) {
  const qc = useQueryClient();
  const [tipo, setTipo] = useState<TipoDestinatario>("Email");
  const [valor, setValor] = useState("");
  const [error, setError] = useState<string | null>(null);

  const criar = useMutation({
    mutationFn: (d: { tipo: TipoDestinatario; valor: string }) => criarDestinatario(d),
    onSuccess: () => {
      toast.success("Destinatário adicionado.");
      qc.invalidateQueries({ queryKey: ["destinatarios"] });
      onOpenChange(false);
      setValor("");
      setTipo("Email");
      setError(null);
    },
    onError: (e) => setError(errMsg(e)),
  });

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    const v = valor.trim();
    const invalido = validar(tipo, v);
    if (invalido) {
      setError(invalido);
      return;
    }
    criar.mutate({ tipo, valor: v });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Adicionar destinatário</DialogTitle>
          <DialogDescription>
            Escolha o tipo e informe o e-mail ou telefone que deve receber os alertas da família.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={onSubmit} className="space-y-4">
          <Field label="Tipo" htmlFor="tipo">
            <Select value={tipo} onValueChange={(v) => setTipo(v as TipoDestinatario)}>
              <SelectTrigger id="tipo">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Email">E-mail</SelectItem>
                <SelectItem value="Telefone">Telefone</SelectItem>
              </SelectContent>
            </Select>
          </Field>
          <Field
            label={tipo === "Email" ? "E-mail" : "Telefone"}
            htmlFor="valor"
            hint={
              tipo === "Email"
                ? "Exemplo: voce@exemplo.com"
                : "Formato internacional (E.164), exemplo: +5511999999999"
            }
          >
            <Input
              id="valor"
              required
              maxLength={255}
              value={valor}
              onChange={(e) => setValor(e.target.value)}
              placeholder={tipo === "Email" ? "voce@exemplo.com" : "+5511999999999"}
            />
          </Field>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <DialogFooter>
            <Button type="submit" disabled={criar.isPending}>
              {criar.isPending && <Loader2 className="size-4 animate-spin" />}
              Adicionar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
