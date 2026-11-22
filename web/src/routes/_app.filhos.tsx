import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { AlertTriangle, CheckCircle2, Pencil, Plus, RefreshCw, Trash2, Users } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
  CICLOS,
  criarFilho,
  editarFilho,
  errMsg,
  formatBRL,
  listarFilhos,
  parseNum,
  removerFilho,
  type Ciclo,
  type Filho,
} from "@/lib/api";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_app/filhos")({
  head: () => ({
    meta: [
      { title: "Gestão de Filhos — MesadApp" },
      { name: "description", content: "Cadastre, edite e acompanhe o saldo devedor de cada filho." },
      { property: "og:title", content: "Gestão de Filhos — MesadApp" },
      { property: "og:description", content: "Cadastre, edite e acompanhe o saldo devedor de cada filho." },
    ],
  }),
  component: FilhosPage,
});

const initials = (nome: string) =>
  nome
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join("");

function FilhosPage() {
  const qc = useQueryClient();
  const { data, isPending, isError, error, refetch, isFetching } = useQuery({
    queryKey: ["filhos"],
    queryFn: listarFilhos,
  });
  const [editing, setEditing] = useState<Filho | "new" | null>(null);
  const [removing, setRemoving] = useState<Filho | null>(null);

  const rm = useMutation({
    mutationFn: (id: string) => removerFilho(id),
    onSuccess: () => {
      toast.success("Filho removido.");
      setRemoving(null);
      qc.invalidateQueries({ queryKey: ["filhos"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  const comDivida = data?.filter((f) => f.saldoDevedorAcumulado > 0).length ?? 0;

  return (
    <>
      <PageHeader
        title="Gestão de Filhos"
        description="Cadastre os filhos da família, defina a mesada base e acompanhe o saldo devedor de cada um."
        actions={
          <>
            <Button variant="outline" onClick={() => refetch()} disabled={isFetching}>
              <RefreshCw className={cn("size-4", isFetching && "animate-spin")} />
              Atualizar
            </Button>
            <Button onClick={() => setEditing("new")}>
              <Plus className="size-4" /> Novo filho
            </Button>
          </>
        }
      />

      {isError && <ErrorAlert title="Não foi possível carregar os filhos" error={error} />}

      {isPending ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-40 rounded-2xl" />
          ))}
        </div>
      ) : data && data.length === 0 ? (
        <EmptyState
          icon={Users}
          title="Nenhum filho cadastrado ainda"
          text="Cadastre o primeiro filho para começar a montar a mesada e as tarefas."
          action={
            <Button onClick={() => setEditing("new")}>
              <Plus className="size-4" /> Cadastrar filho
            </Button>
          }
        />
      ) : data ? (
        <>
          <div className="mb-5 flex flex-wrap gap-2 text-sm text-muted-foreground">
            <span className="rounded-full border bg-card px-3 py-1">
              <strong className="text-foreground">{data.length}</strong> {data.length === 1 ? "filho" : "filhos"}
            </span>
            <span
              className={cn(
                "rounded-full border px-3 py-1",
                comDivida > 0 ? "border-destructive/30 bg-destructive-soft text-destructive" : "bg-card",
              )}
            >
              <strong>{comDivida}</strong> com saldo devedor
            </span>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {data.map((f) => {
              const devedor = f.saldoDevedorAcumulado > 0;
              return (
                <article
                  key={f.id}
                  className={cn(
                    "relative overflow-hidden rounded-2xl border bg-card p-5 shadow-soft transition-shadow hover:shadow-lift",
                    devedor && "border-destructive/40",
                  )}
                >
                  {devedor && <span className="absolute inset-x-0 top-0 h-1 bg-destructive" />}
                  <div className="flex items-start gap-4">
                    <Avatar className="size-12">
                      <AvatarFallback
                        className={cn(
                          "font-display text-base font-extrabold",
                          devedor ? "bg-destructive-soft text-destructive" : "bg-primary-soft text-primary",
                        )}
                      >
                        {initials(f.nome)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="min-w-0 flex-1">
                      <h3 className="truncate text-lg font-extrabold">{f.nome}</h3>
                      <p className="truncate text-sm text-muted-foreground">
                        {f.apelido ? `“${f.apelido}”` : "Sem apelido"}
                      </p>
                    </div>
                    <div className="flex gap-1">
                      <Button size="icon" variant="ghost" aria-label="Editar" onClick={() => setEditing(f)}>
                        <Pencil className="size-4" />
                      </Button>
                      <Button size="icon" variant="ghost" aria-label="Remover" onClick={() => setRemoving(f)}>
                        <Trash2 className="size-4 text-destructive" />
                      </Button>
                    </div>
                  </div>

                  <div className="mt-5 flex items-end justify-between gap-3">
                    <div>
                      <div className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
                        Saldo devedor acumulado
                      </div>
                      <div
                        className={cn(
                          "mt-1 font-display text-2xl font-black tabular-nums",
                          devedor ? "text-destructive" : "text-foreground",
                        )}
                      >
                        {formatBRL(f.saldoDevedorAcumulado)}
                      </div>
                    </div>
                    {devedor ? (
                      <Badge className="gap-1 bg-destructive-soft text-destructive hover:bg-destructive-soft">
                        <AlertTriangle className="size-3" /> Em dívida
                      </Badge>
                    ) : (
                      <Badge className="gap-1 bg-success-soft text-success hover:bg-success-soft">
                        <CheckCircle2 className="size-3" /> Em dia
                      </Badge>
                    )}
                  </div>
                </article>
              );
            })}
          </div>
        </>
      ) : null}

      {editing && (
        <FilhoDialog filho={editing === "new" ? null : editing} onClose={() => setEditing(null)} />
      )}
      <ConfirmDialog
        open={!!removing}
        onOpenChange={(o) => !o && setRemoving(null)}
        title="Remover filho?"
        description={`${removing?.nome ?? ""} deixará de aparecer no painel. O histórico é preservado.`}
        confirmLabel="Remover"
        pending={rm.isPending}
        onConfirm={() => removing && rm.mutate(removing.id)}
      />
    </>
  );
}

function FilhoDialog({ filho, onClose }: { filho: Filho | null; onClose: () => void }) {
  const qc = useQueryClient();
  const [nome, setNome] = useState(filho?.nome ?? "");
  const [apelido, setApelido] = useState(filho?.apelido ?? "");
  const [ciclo, setCiclo] = useState<Ciclo>(filho?.cicloFechamento ?? "Mensal");
  const [mesada, setMesada] = useState(filho?.mesadaBase != null ? String(filho.mesadaBase) : "");
  const [ponto, setPonto] = useState(filho?.valorPonto != null ? String(filho.valorPonto) : "");
  const [err, setErr] = useState<string | null>(null);

  const save = useMutation({
    mutationFn: () => {
      const mesadaBase = parseNum(mesada);
      const valorPonto = parseNum(ponto);
      if (!nome.trim()) throw new Error("Informe o nome.");
      if (mesadaBase === null || mesadaBase < 0) throw new Error("Mesada base deve ser um valor ≥ 0.");
      if (valorPonto !== null && valorPonto <= 0) throw new Error("Valor do ponto deve ser maior que zero.");
      const body = { nome: nome.trim(), apelido: apelido.trim() || null, cicloFechamento: ciclo, mesadaBase, valorPonto };
      return filho ? editarFilho(filho.id, body) : criarFilho(body);
    },
    onSuccess: () => {
      toast.success(filho ? "Filho atualizado." : "Filho cadastrado.");
      qc.invalidateQueries({ queryKey: ["filhos"] });
      onClose();
    },
    onError: (e) => setErr(errMsg(e)),
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{filho ? "Editar filho" : "Novo filho"}</DialogTitle>
          <DialogDescription>
            Dados usados para calcular a mesada.
          </DialogDescription>
        </DialogHeader>
        <form
          className="grid gap-4"
          onSubmit={(e) => {
            e.preventDefault();
            setErr(null);
            save.mutate();
          }}
        >
          <Field label="Nome" htmlFor="f-nome">
            <Input id="f-nome" value={nome} onChange={(e) => setNome(e.target.value)} />
          </Field>
          <Field label="Apelido (opcional)" htmlFor="f-apelido">
            <Input id="f-apelido" value={apelido} onChange={(e) => setApelido(e.target.value)} />
          </Field>
          <div className="grid gap-4 sm:grid-cols-3">
            <Field label="Ciclo de fechamento">
              <Select value={ciclo} onValueChange={(v) => setCiclo(v as Ciclo)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {CICLOS.map((c) => <SelectItem key={c} value={c}>{c}</SelectItem>)}
                </SelectContent>
              </Select>
            </Field>
            <Field label="Mesada base (R$)" htmlFor="f-mesada">
              <Input id="f-mesada" inputMode="decimal" value={mesada} onChange={(e) => setMesada(e.target.value)} />
            </Field>
            <Field label="Valor do ponto (R$)" htmlFor="f-ponto" hint="Só para tarefas em pontos.">
              <Input id="f-ponto" inputMode="decimal" value={ponto} onChange={(e) => setPonto(e.target.value)} />
            </Field>
          </div>
          {err && <p className="text-sm font-medium text-destructive">{err}</p>}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={save.isPending}>{save.isPending ? "Salvando…" : "Salvar"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
