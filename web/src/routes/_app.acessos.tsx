import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Copy, KeyRound, Plus, ShieldCheck, UserX } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
  criarConvite,
  desativarMaster,
  errMsg,
  formatDate,
  listarConvites,
  listarFilhos,
  listarMasters,
  revogarConvite,
  type Convite,
  type ConviteStatus,
  type Master,
} from "@/lib/api";

export const Route = createFileRoute("/_app/acessos")({
  head: () => ({
    meta: [
      { title: "Controle de Acessos — MesadApp" },
      { name: "description", content: "Gere convites para filhos e responsáveis e gerencie quem acessa a família." },
      { property: "og:title", content: "Controle de Acessos — MesadApp" },
      { property: "og:description", content: "Gere convites para filhos e responsáveis e gerencie quem acessa a família." },
    ],
  }),
  component: AcessosPage,
});

const STATUS_CLS: Record<ConviteStatus, string> = {
  Pendente: "bg-honey-soft text-honey-foreground hover:bg-honey-soft dark:text-honey",
  Utilizado: "bg-success-soft text-success hover:bg-success-soft",
  Expirado: "bg-muted text-muted-foreground hover:bg-muted",
  Revogado: "bg-destructive-soft text-destructive hover:bg-destructive-soft",
};

function copy(text: string) {
  navigator.clipboard.writeText(text).then(
    () => toast.success("Código copiado."),
    () => toast.error("Não foi possível copiar."),
  );
}

function AcessosPage() {
  return (
    <>
      <PageHeader title="Controle de Acessos" description="Convide filhos e outro responsável e gerencie quem acessa a conta da família." />
      <Tabs defaultValue="convites">
        <TabsList className="mb-4">
          <TabsTrigger value="convites">Convites</TabsTrigger>
          <TabsTrigger value="masters">Responsáveis</TabsTrigger>
        </TabsList>
        <TabsContent value="convites"><Convites /></TabsContent>
        <TabsContent value="masters"><Masters /></TabsContent>
      </Tabs>
    </>
  );
}

function Convites() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["convites"], queryFn: listarConvites });
  const filhos = useQuery({ queryKey: ["filhos"], queryFn: listarFilhos });
  const [novo, setNovo] = useState(false);
  const [gerado, setGerado] = useState<Convite | null>(null);
  const [revogar, setRevogar] = useState<Convite | null>(null);
  const nomeFilho = (id: string | null) => filhos.data?.find((f) => f.id === id)?.nome;

  const rv = useMutation({
    mutationFn: (id: string) => revogarConvite(id),
    onSuccess: () => {
      toast.success("Convite revogado.");
      setRevogar(null);
      qc.invalidateQueries({ queryKey: ["convites"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  return (
    <>
      <div className="mb-4 flex justify-end">
        <Button onClick={() => setNovo(true)}><Plus className="size-4" /> Gerar convite</Button>
      </div>
      {q.isError && <ErrorAlert title="Não foi possível carregar os convites" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-40 rounded-2xl" />
      ) : q.data && q.data.length === 0 ? (
        <EmptyState icon={KeyRound} title="Nenhum convite ainda" text="Gere um convite para um filho entrar no app ou para um segundo responsável." />
      ) : (
        <div className="overflow-hidden rounded-2xl border bg-card shadow-soft">
          {q.data?.map((c) => (
            <div key={c.id} className="flex flex-wrap items-center gap-3 border-b px-5 py-4 last:border-b-0">
              <code className="rounded-md bg-muted px-2.5 py-1 font-mono text-sm font-bold tracking-wider">{c.codigo}</code>
              <Button size="icon" variant="ghost" aria-label="Copiar código" onClick={() => copy(c.codigo)}>
                <Copy className="size-4" />
              </Button>
              <div className="min-w-0 flex-1 text-sm">
                <div className="font-semibold">
                  {c.papelAlvo === "Master" ? "Responsável" : `Filho${nomeFilho(c.usuarioComumId) ? ` · ${nomeFilho(c.usuarioComumId)}` : ""}`}
                </div>
                <div className="text-xs text-muted-foreground">Expira em {formatDate(c.expiraEm)}</div>
              </div>
              <Badge className={STATUS_CLS[c.status]}>{c.status}</Badge>
              {c.status === "Pendente" && (
                <Button size="sm" variant="outline" onClick={() => setRevogar(c)}>Revogar</Button>
              )}
            </div>
          ))}
        </div>
      )}

      {novo && (
        <NovoConviteDialog
          onClose={() => setNovo(false)}
          onCreated={(c) => {
            setNovo(false);
            setGerado(c);
            qc.invalidateQueries({ queryKey: ["convites"] });
          }}
        />
      )}

      <Dialog open={!!gerado} onOpenChange={(o) => !o && setGerado(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Convite gerado</DialogTitle>
            <DialogDescription>
              Compartilhe este código com {gerado?.papelAlvo === "Master" ? "o responsável convidado" : "seu filho"}.
            </DialogDescription>
          </DialogHeader>
            <div className="min-w-0 rounded-2xl border-2 border-dashed border-primary/40 bg-primary-soft p-6 text-center">
              <div className="break-all font-mono text-3xl font-black tracking-[0.2em] text-primary">{gerado?.codigo}</div>
            {gerado && <p className="mt-2 text-xs text-muted-foreground">Válido até {formatDate(gerado.expiraEm)}</p>}
          </div>
          {gerado?.papelAlvo === "Master" && (
            <div className="grid min-w-0 gap-1.5">
              <p className="text-sm font-medium">Link para compartilhar</p>
              <div className="flex min-w-0 items-center gap-2">
                <code className="min-w-0 flex-1 truncate rounded-md bg-muted px-2.5 py-2 text-xs">
                  {`${window.location.origin}/convite-responsavel?codigo=${encodeURIComponent(gerado.codigo)}`}
                </code>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => copy(`${window.location.origin}/convite-responsavel?codigo=${encodeURIComponent(gerado.codigo)}`)}
                >
                  <Copy className="size-4" /> Copiar link
                </Button>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button onClick={() => gerado && copy(gerado.codigo)}><Copy className="size-4" /> Copiar código</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={!!revogar}
        onOpenChange={(o) => !o && setRevogar(null)}
        title="Revogar convite?"
        description={`O código ${revogar?.codigo ?? ""} deixará de funcionar.`}
        confirmLabel="Revogar"
        pending={rv.isPending}
        onConfirm={() => revogar && rv.mutate(revogar.id)}
      />
    </>
  );
}

function NovoConviteDialog({ onClose, onCreated }: { onClose: () => void; onCreated: (c: Convite) => void }) {
  const filhos = useQuery({ queryKey: ["filhos"], queryFn: listarFilhos });
  const [papel, setPapel] = useState<"Comum" | "Master">("Comum");
  const [filhoId, setFilhoId] = useState("");
  const [nome, setNome] = useState("");
  const [err, setErr] = useState<string | null>(null);
  const m = useMutation({
    mutationFn: () => {
      if (papel === "Comum") {
        if (!filhoId) throw new Error("Escolha o filho.");
        return criarConvite({ papelAlvo: "Comum", usuarioComumId: filhoId, nomeConvidado: null });
      }
      if (!nome.trim()) throw new Error("Informe o nome do convidado.");
      return criarConvite({ papelAlvo: "Master", usuarioComumId: null, nomeConvidado: nome.trim() });
    },
    onSuccess: onCreated,
    onError: (e) => setErr(errMsg(e)),
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Gerar convite</DialogTitle>
          <DialogDescription>Escolha para quem é o convite.</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <Field label="Tipo de convite">
            <Select value={papel} onValueChange={(v) => setPapel(v as "Comum" | "Master")}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="Comum">Filho (app mobile)</SelectItem>
                <SelectItem value="Master">Segundo responsável</SelectItem>
              </SelectContent>
            </Select>
          </Field>
          {papel === "Comum" ? (
            <Field label="Filho">
              <Select value={filhoId} onValueChange={setFilhoId}>
                <SelectTrigger><SelectValue placeholder={filhos.isPending ? "Carregando…" : "Selecione"} /></SelectTrigger>
                <SelectContent>
                  {filhos.data?.map((f) => <SelectItem key={f.id} value={f.id}>{f.nome}</SelectItem>)}
                </SelectContent>
              </Select>
            </Field>
          ) : (
            <Field label="Nome do convidado" htmlFor="c-nome">
              <Input id="c-nome" value={nome} onChange={(e) => setNome(e.target.value)} />
            </Field>
          )}
          {err && <p className="text-sm font-medium text-destructive">{err}</p>}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button disabled={m.isPending} onClick={() => { setErr(null); m.mutate(); }}>
            {m.isPending ? "Gerando…" : "Gerar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function Masters() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["masters"], queryFn: listarMasters });
  const [alvo, setAlvo] = useState<Master | null>(null);
  const m = useMutation({
    mutationFn: (id: string) => desativarMaster(id),
    onSuccess: () => {
      toast.success("Responsável desativado.");
      setAlvo(null);
      qc.invalidateQueries({ queryKey: ["masters"] });
    },
    onError: (e: unknown) => {
      setAlvo(null);
      const status = (e as { status?: number }).status;
      toast.error(
        status === 400
          ? "Não é possível desativar: você não pode desativar a própria conta nem o único responsável financeiro ativo."
          : errMsg(e),
      );
    },
  });

  return (
    <>
      <p className="mb-4 text-sm text-muted-foreground">Novos responsáveis entram somente por convite.</p>
      {q.isError && <ErrorAlert title="Não foi possível carregar os responsáveis" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-32 rounded-2xl" />
      ) : (
        <div className="overflow-hidden rounded-2xl border bg-card shadow-soft">
          {q.data?.map((ms) => (
            <div key={ms.id} className="flex flex-wrap items-center gap-3 border-b px-5 py-4 last:border-b-0">
              <div className="min-w-0 flex-1">
                <div className="font-semibold">{ms.nome}</div>
                <div className="truncate text-sm text-muted-foreground">{ms.email}</div>
              </div>
              {ms.isFinanceiro && (
                <Badge className="gap-1 bg-primary-soft text-primary hover:bg-primary-soft"><ShieldCheck className="size-3" /> Financeiro</Badge>
              )}
              <Badge variant={ms.status === "ativo" ? "secondary" : "outline"}>{ms.status === "ativo" ? "Ativo" : "Inativo"}</Badge>
              {ms.status === "ativo" && (
                <Button size="sm" variant="outline" onClick={() => setAlvo(ms)}><UserX className="size-4" /> Desativar</Button>
              )}
            </div>
          ))}
        </div>
      )}
      <ConfirmDialog
        open={!!alvo}
        onOpenChange={(o) => !o && setAlvo(null)}
        title="Desativar responsável?"
        description={`${alvo?.nome ?? ""} perderá o acesso ao painel da família.`}
        confirmLabel="Desativar"
        pending={m.isPending}
        onConfirm={() => alvo && m.mutate(alvo.id)}
      />
    </>
  );
}
