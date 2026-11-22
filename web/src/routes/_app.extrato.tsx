import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { CalendarCheck, Receipt } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cicloAtual, errMsg, fecharCiclo, formatBRL, formatDate, historicoCiclos, listarFilhos } from "@/lib/api";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_app/extrato")({
  head: () => ({
    meta: [
      { title: "Extrato e Histórico — MesadApp" },
      { name: "description", content: "Período em andamento e ciclos de mesada fechados de cada filho." },
      { property: "og:title", content: "Extrato e Histórico — MesadApp" },
      { property: "og:description", content: "Período em andamento e ciclos de mesada fechados de cada filho." },
    ],
  }),
  component: ExtratoPage,
});

function ExtratoPage() {
  const filhos = useQuery({ queryKey: ["filhos"], queryFn: listarFilhos });
  const [uid, setUid] = useState("");
  const [fechar, setFechar] = useState(false);
  useEffect(() => {
    if (!uid && filhos.data?.[0]) setUid(filhos.data[0].id);
  }, [filhos.data, uid]);

  const atual = useQuery({ queryKey: ["ciclo-atual", uid], queryFn: () => cicloAtual(uid), enabled: !!uid });
  const hist = useQuery({ queryKey: ["ciclo-hist", uid], queryFn: () => historicoCiclos(uid), enabled: !!uid });

  return (
    <>
      <PageHeader
        title="Extrato e Histórico"
        description="Prévia do período em andamento e todos os ciclos já fechados, por filho."
        actions={
          <>
            <Select value={uid} onValueChange={setUid}>
              <SelectTrigger className="w-52"><SelectValue placeholder="Selecione o filho" /></SelectTrigger>
              <SelectContent>
                {filhos.data?.map((f) => <SelectItem key={f.id} value={f.id}>{f.nome}</SelectItem>)}
              </SelectContent>
            </Select>
            <Button disabled={!uid} onClick={() => setFechar(true)}><CalendarCheck className="size-4" /> Fechar ciclo</Button>
          </>
        }
      />
      {filhos.isError && <ErrorAlert title="Não foi possível carregar os filhos" error={filhos.error} />}
      {filhos.data?.length === 0 && (
        <EmptyState icon={Receipt} title="Nenhum filho cadastrado" text="Cadastre um filho para acompanhar o extrato." />
      )}

      {uid && (
        <>
          <section className="mb-8 rounded-2xl border bg-card p-6 shadow-soft">
            <div className="mb-4 flex flex-wrap items-baseline justify-between gap-2">
              <h2 className="text-lg font-extrabold">Período em andamento</h2>
              {atual.data && (
                <span className="text-sm text-muted-foreground">
                  {formatDate(atual.data.dataInicio)} – {formatDate(atual.data.dataFim)} · valores previstos
                </span>
              )}
            </div>
            {atual.isError ? (
              <p className="text-sm text-destructive">{errMsg(atual.error)}</p>
            ) : !atual.data ? (
              <Skeleton className="h-24" />
            ) : (
              <div className="grid gap-4 sm:grid-cols-3 lg:grid-cols-6">
                <Stat label="Mesada base" v={atual.data.mesadaBase} />
                <Stat label="Bônus" v={atual.data.somaBonus} tone="ok" />
                <Stat label="Multas" v={-atual.data.somaMultas} tone="bad" />
                <Stat label="Saldo devedor anterior" v={atual.data.saldoDevedorAnterior} />
                <Stat label="Valor final previsto" v={atual.data.valorFinalPrevisto} strong />
                <Stat label="Saldo devedor previsto" v={atual.data.saldoDevedorPrevisto} tone={atual.data.saldoDevedorPrevisto > 0 ? "bad" : undefined} />
              </div>
            )}
          </section>

          <h2 className="mb-3 text-lg font-extrabold">Ciclos fechados</h2>
          {hist.isError && <ErrorAlert title="Não foi possível carregar o histórico" error={hist.error} />}
          {hist.isPending ? (
            <Skeleton className="h-40 rounded-2xl" />
          ) : hist.data && hist.data.length === 0 ? (
            <EmptyState icon={Receipt} title="Nenhum ciclo fechado" text="Quando você fechar um ciclo, ele aparecerá aqui." />
          ) : (
            <div className="overflow-x-auto rounded-2xl border bg-card shadow-soft">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Período</TableHead>
                    <TableHead className="text-right">Mesada</TableHead>
                    <TableHead className="text-right">Bônus</TableHead>
                    <TableHead className="text-right">Multas</TableHead>
                    <TableHead className="text-right">Dívida anterior</TableHead>
                    <TableHead className="text-right">Valor final</TableHead>
                    <TableHead className="text-right">Dívida resultante</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {hist.data?.map((c) => (
                    <TableRow key={c.id}>
                      <TableCell className="whitespace-nowrap">{formatDate(c.dataInicio)} – {formatDate(c.dataFim)}</TableCell>
                      <TableCell className="text-right tabular-nums">{formatBRL(c.mesadaBase)}</TableCell>
                      <TableCell className="text-right tabular-nums text-success">{formatBRL(c.somaBonus)}</TableCell>
                      <TableCell className="text-right tabular-nums text-destructive">{formatBRL(c.somaMultas)}</TableCell>
                      <TableCell className="text-right tabular-nums">{formatBRL(c.saldoDevedorAnterior)}</TableCell>
                      <TableCell className="text-right font-bold tabular-nums">{formatBRL(c.valorFinal)}</TableCell>
                      <TableCell className={cn("text-right tabular-nums", c.saldoDevedorResultante > 0 && "text-destructive")}>
                        {formatBRL(c.saldoDevedorResultante)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </>
      )}

      {fechar && uid && (
        <FecharDialog
          uid={uid}
          nome={filhos.data?.find((f) => f.id === uid)?.nome ?? ""}
          inicio={atual.data?.dataInicio?.slice(0, 10) ?? ""}
          fim={atual.data?.dataFim?.slice(0, 10) ?? ""}
          onClose={() => setFechar(false)}
        />
      )}
    </>
  );
}

function Stat({ label, v, tone, strong }: { label: string; v: number; tone?: "ok" | "bad" | undefined; strong?: boolean }) {
  return (
    <div className={cn("rounded-xl border bg-background/60 p-3", strong && "border-primary/40 bg-primary-soft")}>
      <div className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">{label}</div>
      <div
        className={cn(
          "mt-1 font-display text-lg font-black tabular-nums",
          tone === "ok" && "text-success",
          tone === "bad" && "text-destructive",
          strong && "text-primary",
        )}
      >
        {formatBRL(v)}
      </div>
    </div>
  );
}

function FecharDialog({ uid, nome, inicio, fim, onClose }: { uid: string; nome: string; inicio: string; fim: string; onClose: () => void }) {
  const qc = useQueryClient();
  const [di, setDi] = useState(inicio);
  const [df, setDf] = useState(fim);
  const [err, setErr] = useState<string | null>(null);
  const m = useMutation({
    mutationFn: () => {
      if (!di || !df) throw new Error("Informe as duas datas.");
      if (df < di) throw new Error("A data final deve ser posterior à inicial.");
      return fecharCiclo({ usuarioComumId: uid, dataInicio: di, dataFim: df });
    },
    onSuccess: (c) => {
      toast.success(`Ciclo fechado: ${formatBRL(c.valorFinal)}.`);
      qc.invalidateQueries({ queryKey: ["ciclo-hist", uid] });
      qc.invalidateQueries({ queryKey: ["ciclo-atual", uid] });
      qc.invalidateQueries({ queryKey: ["filhos"] });
      onClose();
    },
    onError: (e) => setErr(errMsg(e)),
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Fechar ciclo — {nome}</DialogTitle>
          <DialogDescription>Informe o período. O fechamento é definitivo.</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label="Data de início" htmlFor="di"><Input id="di" type="date" value={di} onChange={(e) => setDi(e.target.value)} /></Field>
          <Field label="Data de fim" htmlFor="df"><Input id="df" type="date" value={df} onChange={(e) => setDf(e.target.value)} /></Field>
        </div>
        {err && <p className="text-sm font-medium text-destructive">{err}</p>}
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button disabled={m.isPending} onClick={() => { setErr(null); m.mutate(); }}>
            {m.isPending ? "Fechando…" : "Fechar ciclo"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
