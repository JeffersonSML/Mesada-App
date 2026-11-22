import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CICLOS, errMsg, obterFamilia, salvarFamilia, type Ciclo } from "@/lib/api";

export const Route = createFileRoute("/_app/familia")({
  head: () => ({
    meta: [
      { title: "Minha Família — MesadApp" },
      { name: "description", content: "Edite o nome da família e o ciclo de fechamento padrão da mesada." },
      { property: "og:title", content: "Minha Família — MesadApp" },
      { property: "og:description", content: "Edite o nome da família e o ciclo de fechamento padrão da mesada." },
    ],
  }),
  component: FamiliaPage,
});

function FamiliaPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["familia"], queryFn: obterFamilia });
  const [nome, setNome] = useState("");
  const [ciclo, setCiclo] = useState<Ciclo>("Mensal");

  useEffect(() => {
    if (q.data) {
      setNome(q.data.nome);
      setCiclo(q.data.cicloFechamentoPadrao);
    }
  }, [q.data]);

  const m = useMutation({
    mutationFn: () => salvarFamilia({ nome: nome.trim(), cicloFechamentoPadrao: ciclo }),
    onSuccess: () => {
      toast.success("Dados da família salvos.");
      qc.invalidateQueries({ queryKey: ["familia"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!nome.trim()) {
      toast.error("Informe o nome da família.");
      return;
    }
    m.mutate();
  };

  return (
    <>
      <PageHeader title="Minha Família" description="Nome da família e ciclo padrão de fechamento da mesada." />
      {q.error && <ErrorAlert title="Não foi possível carregar a família" error={q.error} />}
      <form onSubmit={onSubmit} className="max-w-xl space-y-5 rounded-2xl border bg-card p-6 shadow-soft">
        {q.isLoading ? (
          <div className="space-y-4"><Skeleton className="h-10" /><Skeleton className="h-10" /></div>
        ) : (
          <>
            <Field label="Nome da família" htmlFor="nome">
              <Input id="nome" value={nome} maxLength={100} onChange={(e) => setNome(e.target.value)} />
            </Field>
            <Field label="Ciclo de fechamento padrão">
              <Select value={ciclo} onValueChange={(v) => setCiclo(v as Ciclo)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {CICLOS.map((c) => <SelectItem key={c} value={c}>{c}</SelectItem>)}
                </SelectContent>
              </Select>
            </Field>
            <Button type="submit" disabled={m.isPending || !q.data}>
              {m.isPending && <Loader2 className="size-4 animate-spin" />} Salvar
            </Button>
          </>
        )}
      </form>
    </>
  );
}
