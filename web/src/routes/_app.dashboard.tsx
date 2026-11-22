import { createFileRoute, Link } from "@tanstack/react-router";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ArrowRight, ClipboardCheck, Users, Wallet } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { formatBRL, listarFilhos } from "@/lib/api";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_app/dashboard")({
  head: () => ({
    meta: [
      { title: "Dashboard da Família — MesadApp" },
      { name: "description", content: "Resumo do saldo dos filhos e das tarefas aguardando aprovação." },
      { property: "og:title", content: "Dashboard da Família — MesadApp" },
      { property: "og:description", content: "Resumo do saldo dos filhos e das tarefas aguardando aprovação." },
    ],
  }),
  component: DashboardPage,
});

function DashboardPage() {
  const { data, isPending, isError, error } = useQuery({
    queryKey: ["filhos"],
    queryFn: listarFilhos,
  });

  const totalDevedor = data?.reduce((s, f) => s + f.saldoDevedorAcumulado, 0) ?? 0;
  const comDivida = data?.filter((f) => f.saldoDevedorAcumulado > 0).length ?? 0;

  return (
    <>
      <PageHeader
        title="Dashboard da Família"
        description="Uma visão rápida de como está a mesada de cada filho hoje."
      />

      {isError && (
        <Alert variant="destructive" className="mb-6">
          <AlertTriangle className="size-4" />
          <AlertTitle>Não foi possível carregar os dados</AlertTitle>
          <AlertDescription>{(error as Error).message}</AlertDescription>
        </Alert>
      )}

      <section className="grid gap-4 sm:grid-cols-3">
        <SummaryCard
          icon={Users}
          label="Filhos na família"
          value={isPending ? null : String(data?.length ?? 0)}
          tone="highlight"
        />
        <SummaryCard
          icon={Wallet}
          label="Saldo devedor total"
          value={isPending ? null : formatBRL(totalDevedor)}
          tone={totalDevedor > 0 ? "danger" : "ok"}
        />
        <SummaryCard
          icon={AlertTriangle}
          label="Filhos com dívida"
          value={isPending ? null : String(comDivida)}
          tone={comDivida > 0 ? "danger" : "ok"}
        />
      </section>

      <section className="mt-8">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-extrabold">Saldo por filho</h2>
          <Link to="/filhos" className="flex items-center gap-1 text-sm font-medium text-primary hover:underline">
            Ver todos <ArrowRight className="size-4" />
          </Link>
        </div>
        {isPending ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-24 rounded-2xl" />
            ))}
          </div>
        ) : data && data.length > 0 ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {data.map((f) => {
              const devedor = f.saldoDevedorAcumulado > 0;
              return (
                <div
                  key={f.id}
                  className={cn(
                    "flex items-center justify-between rounded-2xl border bg-card p-4 shadow-soft",
                    devedor && "border-destructive/40",
                  )}
                >
                  <div className="min-w-0">
                    <div className="truncate font-semibold">{f.apelido || f.nome}</div>
                    <div className="text-xs text-muted-foreground">Saldo devedor</div>
                  </div>
                  <div
                    className={cn(
                      "font-display text-lg font-black tabular-nums",
                      devedor ? "text-destructive" : "text-success",
                    )}
                  >
                    {formatBRL(f.saldoDevedorAcumulado)}
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed bg-card p-8 text-center text-sm text-muted-foreground">
            Nenhum filho cadastrado ainda.
          </div>
        )}
      </section>

      <section className="mt-8">
        <div className="mb-3 flex items-center gap-2">
          <h2 className="text-lg font-extrabold">Tarefas pendentes de aprovação</h2>
          <Badge className="bg-honey-soft text-honey-foreground hover:bg-honey-soft dark:text-honey">Em breve</Badge>
        </div>
        <div className="flex flex-col items-center rounded-2xl border border-dashed bg-card px-6 py-12 text-center">
          <span className="grid size-14 place-items-center rounded-2xl bg-honey-soft text-honey-foreground dark:text-honey">
            <ClipboardCheck className="size-7" />
          </span>
          <h3 className="mt-4 font-extrabold">Nada para aprovar por aqui — ainda</h3>
          <p className="mt-1 max-w-md text-sm text-muted-foreground">
            Em breve, as tarefas que seus filhos concluírem aparecerão aqui para você aprovar ou
            devolver com um comentário, direto do dashboard.
          </p>
        </div>
      </section>
    </>
  );
}

function SummaryCard({
  icon: Icon,
  label,
  value,
  tone = "neutral",
}: {
  icon: typeof Users;
  label: string;
  value: string | null;
  tone?: "neutral" | "ok" | "danger" | "highlight";
}) {
  const highlight = tone === "highlight";
  return (
    <div
      className={cn(
        "rounded-2xl border bg-card p-5 shadow-soft",
        highlight && "border-transparent bg-primary text-primary-foreground",
      )}
    >
      <div className="flex items-center justify-between">
        <span className={cn("text-sm font-medium text-muted-foreground", highlight && "text-primary-foreground")}>
          {label}
        </span>
        <span
          className={cn(
            "grid size-9 place-items-center rounded-xl",
            highlight && "bg-primary-foreground/15 text-primary-foreground",
            tone === "danger" && "bg-destructive-soft text-destructive",
            tone === "ok" && "bg-success-soft text-success",
            tone === "neutral" && "bg-primary-soft text-primary",
          )}
        >
          <Icon className="size-4.5" />
        </span>
      </div>
      {value === null ? (
        <Skeleton className="mt-3 h-8 w-28" />
      ) : (
        <div
          className={cn(
            "mt-3 font-display text-3xl font-black tabular-nums",
            tone === "danger" && "text-destructive",
          )}
        >
          {value}
        </div>
      )}
    </div>
  );
}
