import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Check, ClipboardCheck, RefreshCw, X } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { EmptyState, ErrorAlert } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { aprovarExecucao, errMsg, formatBRL, formatDate, listarPendentes, rejeitarExecucao } from "@/lib/api";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_app/aprovacoes")({
  head: () => ({
    meta: [
      { title: "Aprovações Pendentes — MesadApp" },
      { name: "description", content: "Aprove ou rejeite as tarefas concluídas pelos seus filhos." },
      { property: "og:title", content: "Aprovações Pendentes — MesadApp" },
      { property: "og:description", content: "Aprove ou rejeite as tarefas concluídas pelos seus filhos." },
    ],
  }),
  component: AprovacoesPage,
});

const STATUS_LABEL = { Feito: "Feito", Parcial: "Parcial", NaoFeito: "Não feito" } as const;

function AprovacoesPage() {
  const qc = useQueryClient();
  const { data, isPending, isError, error, refetch, isFetching } = useQuery({
    queryKey: ["pendentes"],
    queryFn: listarPendentes,
  });
  const act = useMutation({
    mutationFn: ({ id, ok }: { id: string; ok: boolean }) => (ok ? aprovarExecucao(id) : rejeitarExecucao(id)),
    onSuccess: (_, v) => {
      toast.success(v.ok ? "Execução aprovada." : "Execução rejeitada.");
      qc.invalidateQueries({ queryKey: ["pendentes"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  return (
    <>
      <PageHeader
        title="Aprovações Pendentes"
        description="Tarefas que seus filhos registraram e aguardam sua confirmação."
        actions={
          <Button variant="outline" onClick={() => refetch()} disabled={isFetching}>
            <RefreshCw className={cn("size-4", isFetching && "animate-spin")} /> Atualizar
          </Button>
        }
      />
      {isError && <ErrorAlert title="Não foi possível carregar as aprovações" error={error} />}
      {isPending ? (
        <div className="grid gap-3">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-2xl" />)}
        </div>
      ) : data && data.length === 0 ? (
        <EmptyState icon={ClipboardCheck} title="Tudo em dia" text="Não há execuções aguardando aprovação." />
      ) : (
        <div className="grid gap-3">
          {data?.map((e) => {
            const busy = act.isPending && act.variables?.id === e.id;
            return (
              <article key={e.id} className="flex flex-col gap-4 rounded-2xl border bg-card p-5 shadow-soft sm:flex-row sm:items-center">
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge variant="secondary">{STATUS_LABEL[e.status] ?? e.status}</Badge>
                    {e.percentualConclusao != null && e.status === "Parcial" && (
                      <Badge variant="outline">{e.percentualConclusao}% concluído</Badge>
                    )}
                    <span className="text-sm text-muted-foreground">Executada em {formatDate(e.dataExecucao)}</span>
                  </div>
                  <p className="mt-2 truncate text-xs text-muted-foreground">Vínculo tarefa/filho: {e.tarefaUsuarioId}</p>
                </div>
                <div className="text-right">
                  {e.valorCalculado != null && (
                    <div className="font-display text-xl font-black tabular-nums">{formatBRL(e.valorCalculado)}</div>
                  )}
                  {e.pontosCalculado != null && (
                    <div className="text-sm font-semibold text-muted-foreground">{e.pontosCalculado} pontos</div>
                  )}
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" disabled={busy} onClick={() => act.mutate({ id: e.id, ok: false })}>
                    <X className="size-4" /> Rejeitar
                  </Button>
                  <Button disabled={busy} onClick={() => act.mutate({ id: e.id, ok: true })}>
                    <Check className="size-4" /> Aprovar
                  </Button>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </>
  );
}
