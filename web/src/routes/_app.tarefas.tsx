import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { ListTodo, Link2, Pencil, Plus, Sparkles, Trash2, Unlink } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import {
  aderirTarefa,
  criarTarefa,
  desaderirTarefa,
  editarTarefa,
  errMsg,
  formatBRL,
  formatDate,
  listarCategorias,
  listarFilhos,
  listarTarefas,
  parseNum,
  removerTarefa,
  sugerirValor,
  type ModoCalculo,
  type Natureza,
  type Provedor,
  type Tarefa,
  type TarefaInput,
  type TipoEvidencia,
  type TipoTarefa,
} from "@/lib/api";

export const Route = createFileRoute("/_app/tarefas")({
  head: () => ({
    meta: [
      { title: "Tarefas — MesadApp" },
      { name: "description", content: "Crie tarefas de bônus e obrigatórias e vincule-as a cada filho." },
      { property: "og:title", content: "Tarefas — MesadApp" },
      { property: "og:description", content: "Crie tarefas de bônus e obrigatórias e vincule-as a cada filho." },
    ],
  }),
  component: TarefasPage,
});

function TarefasPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["tarefas"], queryFn: listarTarefas });
  const cats = useQuery({ queryKey: ["categorias"], queryFn: listarCategorias });
  const [edit, setEdit] = useState<Tarefa | "new" | null>(null);
  const [rm, setRm] = useState<Tarefa | null>(null);
  const [ader, setAder] = useState<Tarefa | null>(null);
  const del = useMutation({
    mutationFn: (id: string) => removerTarefa(id),
    onSuccess: () => {
      toast.success("Tarefa removida.");
      setRm(null);
      qc.invalidateQueries({ queryKey: ["tarefas"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });
  const catNome = (id: string) => cats.data?.find((c) => c.id === id)?.nome ?? "—";

  return (
    <>
      <PageHeader
        title="Tarefas"
        description="Defina as tarefas, como são recompensadas e quais filhos participam de cada uma."
        actions={<Button onClick={() => setEdit("new")}><Plus className="size-4" /> Nova tarefa</Button>}
      />
      {q.isError && <ErrorAlert title="Não foi possível carregar as tarefas" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-48 rounded-2xl" />
      ) : q.data && q.data.length === 0 ? (
        <EmptyState icon={ListTodo} title="Nenhuma tarefa" text="Crie a primeira tarefa da família." />
      ) : (
        <div className="grid gap-3 md:grid-cols-2">
          {q.data?.map((t) => (
            <article key={t.id} className="flex flex-col rounded-2xl border bg-card p-5 shadow-soft">
              <div className="flex items-start gap-3">
                <div className="min-w-0 flex-1">
                  <h3 className="truncate text-lg font-extrabold">{t.nome}</h3>
                  <p className="text-xs text-muted-foreground">{catNome(t.categoriaId)}</p>
                </div>
                <div className="text-right font-display text-lg font-black tabular-nums">
                  {t.modoCalculo === "ValorDireto" ? formatBRL(t.valor ?? 0) : `${t.pontos ?? 0} pts`}
                </div>
              </div>
              {t.descricao && <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">{t.descricao}</p>}
              <div className="mt-3 flex flex-wrap gap-1.5">
                <Badge variant={t.natureza === "Obrigatoria" ? "destructive" : "secondary"}>
                  {t.natureza === "Obrigatoria" ? `Obrigatória${t.valorMulta ? ` · multa ${formatBRL(t.valorMulta)}` : ""}` : "Bônus"}
                </Badge>
                <Badge variant="outline">{t.tipo === "Avulsa" ? `Avulsa até ${t.validadeAvulsa ? formatDate(t.validadeAvulsa) : "—"}` : "Recorrente"}</Badge>
                {t.requerAprovacao && <Badge variant="outline">Requer aprovação</Badge>}
                {t.permiteParcial && <Badge variant="outline">Aceita parcial</Badge>}
                {t.tipoEvidencia !== "Nenhuma" && (
                  <Badge variant="outline">{t.tipoEvidencia === "Foto" ? "Foto" : t.provedorIntegracao}</Badge>
                )}
              </div>
              <div className="mt-4 flex gap-2 border-t pt-3">
                <Button size="sm" variant="outline" onClick={() => setAder(t)}><Link2 className="size-4" /> Filhos</Button>
                <div className="flex-1" />
                <Button size="icon" variant="ghost" aria-label="Editar" onClick={() => setEdit(t)}><Pencil className="size-4" /></Button>
                <Button size="icon" variant="ghost" aria-label="Remover" onClick={() => setRm(t)}><Trash2 className="size-4 text-destructive" /></Button>
              </div>
            </article>
          ))}
        </div>
      )}
      {edit && <TarefaSheet tarefa={edit === "new" ? null : edit} onClose={() => setEdit(null)} />}
      {ader && <AderenciasDialog tarefa={ader} onClose={() => setAder(null)} />}
      <ConfirmDialog
        open={!!rm}
        onOpenChange={(o) => !o && setRm(null)}
        title="Remover tarefa?"
        description={`"${rm?.nome ?? ""}" deixará de estar disponível.`}
        confirmLabel="Remover"
        pending={del.isPending}
        onConfirm={() => rm && del.mutate(rm.id)}
      />
    </>
  );
}

function TarefaSheet({ tarefa, onClose }: { tarefa: Tarefa | null; onClose: () => void }) {
  const qc = useQueryClient();
  const cats = useQuery({ queryKey: ["categorias"], queryFn: listarCategorias });
  const filhos = useQuery({ queryKey: ["filhos"], queryFn: listarFilhos });
  const [f, setF] = useState({
    categoriaId: tarefa?.categoriaId ?? "",
    nome: tarefa?.nome ?? "",
    descricao: tarefa?.descricao ?? "",
    modoCalculo: (tarefa?.modoCalculo ?? "ValorDireto") as ModoCalculo,
    valor: tarefa?.valor != null ? String(tarefa.valor) : "",
    pontos: tarefa?.pontos != null ? String(tarefa.pontos) : "",
    permiteParcial: tarefa?.permiteParcial ?? false,
    tipo: (tarefa?.tipo ?? "Recorrente") as TipoTarefa,
    validadeAvulsa: tarefa?.validadeAvulsa?.slice(0, 10) ?? "",
    natureza: (tarefa?.natureza ?? "Bonus") as Natureza,
    valorMulta: tarefa?.valorMulta != null ? String(tarefa.valorMulta) : "",
    requerAprovacao: tarefa?.requerAprovacao ?? true,
    tipoEvidencia: (tarefa?.tipoEvidencia ?? "Nenhuma") as TipoEvidencia,
    provedorIntegracao: (tarefa?.provedorIntegracao ?? "") as Provedor | "",
  });
  const set = <K extends keyof typeof f>(k: K, v: (typeof f)[K]) => setF((s) => ({ ...s, [k]: v }));
  const [err, setErr] = useState<string | null>(null);
  const [sugFilho, setSugFilho] = useState("");

  const sug = useMutation({
    mutationFn: () => {
      if (!sugFilho) throw new Error("Escolha um filho para calcular a sugestão.");
      return sugerirValor(sugFilho, f.modoCalculo);
    },
    onSuccess: (r) => {
      if (r.modoCalculo === "Pontos") set("pontos", String(r.valor));
      else set("valor", String(r.valor));
      toast.success("Sugestão aplicada.");
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  const save = useMutation({
    mutationFn: () => {
      if (!f.categoriaId) throw new Error("Escolha a categoria.");
      if (!f.nome.trim()) throw new Error("Informe o nome.");
      const valor = f.modoCalculo === "ValorDireto" ? parseNum(f.valor) : null;
      const pontos = f.modoCalculo === "Pontos" ? parseNum(f.pontos) : null;
      if (f.modoCalculo === "ValorDireto" && valor === null) throw new Error("Informe o valor da tarefa.");
      if (f.modoCalculo === "Pontos" && pontos === null) throw new Error("Informe os pontos da tarefa.");
      if (f.tipo === "Avulsa" && !f.validadeAvulsa) throw new Error("Informe a validade da tarefa avulsa.");
      if (f.tipoEvidencia === "IntegracaoAutomatica" && !f.provedorIntegracao) throw new Error("Escolha o provedor de integração.");
      const body: TarefaInput = {
        categoriaId: f.categoriaId,
        nome: f.nome.trim(),
        descricao: f.descricao.trim() || null,
        modoCalculo: f.modoCalculo,
        valor,
        pontos,
        permiteParcial: f.permiteParcial,
        tipo: f.tipo,
        validadeAvulsa: f.tipo === "Avulsa" ? f.validadeAvulsa : null,
        natureza: f.natureza,
        valorMulta: f.natureza === "Obrigatoria" ? parseNum(f.valorMulta) : null,
        requerAprovacao: f.requerAprovacao,
        tipoEvidencia: f.tipoEvidencia,
        provedorIntegracao: f.tipoEvidencia === "IntegracaoAutomatica" && f.provedorIntegracao ? f.provedorIntegracao : null,
      };
      return tarefa ? editarTarefa(tarefa.id, body) : criarTarefa(body);
    },
    onSuccess: () => {
      toast.success(tarefa ? "Tarefa atualizada." : "Tarefa criada.");
      qc.invalidateQueries({ queryKey: ["tarefas"] });
      onClose();
    },
    onError: (e) => setErr(errMsg(e)),
  });

  const Sel = <T extends string>({ value, onChange, opts }: { value: T | ""; onChange: (v: T) => void; opts: [T, string][] }) => (
    <Select value={value} onValueChange={(v) => onChange(v as T)}>
      <SelectTrigger><SelectValue placeholder="Selecione" /></SelectTrigger>
      <SelectContent>{opts.map(([v, l]) => <SelectItem key={v} value={v}>{l}</SelectItem>)}</SelectContent>
    </Select>
  );
  const Toggle = ({ id, label, checked, onChange }: { id: string; label: string; checked: boolean; onChange: (b: boolean) => void }) => (
    <div className="flex items-center justify-between rounded-lg border px-3 py-2">
      <Label htmlFor={id}>{label}</Label>
      <Switch id={id} checked={checked} onCheckedChange={onChange} />
    </div>
  );

  return (
    <Sheet open onOpenChange={(o) => !o && onClose()}>
      <SheetContent className="w-full overflow-y-auto sm:max-w-lg">
        <SheetHeader>
          <SheetTitle>{tarefa ? "Editar tarefa" : "Nova tarefa"}</SheetTitle>
          <SheetDescription>Os campos se ajustam às regras de cada tipo de tarefa.</SheetDescription>
        </SheetHeader>
        <form
          className="grid gap-4 px-4 pb-6"
          onSubmit={(e) => { e.preventDefault(); setErr(null); save.mutate(); }}
        >
          <Field label="Categoria">
            <Sel value={f.categoriaId} onChange={(v) => set("categoriaId", v)} opts={(cats.data ?? []).map((c) => [c.id, c.nome])} />
          </Field>
          <Field label="Nome" htmlFor="t-nome"><Input id="t-nome" value={f.nome} onChange={(e) => set("nome", e.target.value)} /></Field>
          <Field label="Descrição" htmlFor="t-desc"><Textarea id="t-desc" value={f.descricao} onChange={(e) => set("descricao", e.target.value)} /></Field>

          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Modo de cálculo">
              <Sel value={f.modoCalculo} onChange={(v) => set("modoCalculo", v)} opts={[["ValorDireto", "Valor em R$"], ["Pontos", "Pontos"]]} />
            </Field>
            {f.modoCalculo === "ValorDireto" ? (
              <Field label="Valor (R$)" htmlFor="t-valor"><Input id="t-valor" inputMode="decimal" value={f.valor} onChange={(e) => set("valor", e.target.value)} /></Field>
            ) : (
              <Field label="Pontos" htmlFor="t-pontos"><Input id="t-pontos" inputMode="decimal" value={f.pontos} onChange={(e) => set("pontos", e.target.value)} /></Field>
            )}
          </div>
          <div className="flex flex-wrap items-end gap-2 rounded-lg border border-dashed p-3">
            <div className="min-w-40 flex-1">
              <Field label="Sugerir com base na mesada de">
                <Sel value={sugFilho} onChange={setSugFilho} opts={(filhos.data ?? []).map((x) => [x.id, x.nome])} />
              </Field>
            </div>
            <Button type="button" variant="outline" disabled={sug.isPending} onClick={() => sug.mutate()}>
              <Sparkles className="size-4" /> Sugerir valor
            </Button>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Tipo">
              <Sel value={f.tipo} onChange={(v) => set("tipo", v)} opts={[["Recorrente", "Recorrente"], ["Avulsa", "Avulsa"]]} />
            </Field>
            {f.tipo === "Avulsa" && (
              <Field label="Válida até" htmlFor="t-val"><Input id="t-val" type="date" value={f.validadeAvulsa} onChange={(e) => set("validadeAvulsa", e.target.value)} /></Field>
            )}
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Natureza">
              <Sel value={f.natureza} onChange={(v) => set("natureza", v)} opts={[["Bonus", "Bônus"], ["Obrigatoria", "Obrigatória"]]} />
            </Field>
            {f.natureza === "Obrigatoria" && (
              <Field label="Multa se não fizer (R$)" htmlFor="t-multa"><Input id="t-multa" inputMode="decimal" value={f.valorMulta} onChange={(e) => set("valorMulta", e.target.value)} /></Field>
            )}
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Evidência">
              <Sel
                value={f.tipoEvidencia}
                onChange={(v) => set("tipoEvidencia", v)}
                opts={[["Nenhuma", "Nenhuma"], ["Foto", "Foto"], ["IntegracaoAutomatica", "Integração automática"]]}
              />
            </Field>
            {f.tipoEvidencia === "IntegracaoAutomatica" && (
              <Field label="Provedor">
                <Sel
                  value={f.provedorIntegracao}
                  onChange={(v) => set("provedorIntegracao", v)}
                  opts={[["Strava", "Strava"], ["GoogleFit", "Google Fit"], ["AppleHealth", "Apple Health"]]}
                />
              </Field>
            )}
          </div>
          <Toggle id="t-parcial" label="Aceita conclusão parcial" checked={f.permiteParcial} onChange={(b) => set("permiteParcial", b)} />
          <Toggle id="t-aprov" label="Requer aprovação do responsável" checked={f.requerAprovacao} onChange={(b) => set("requerAprovacao", b)} />

          {err && <p className="text-sm font-medium text-destructive">{err}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={save.isPending}>{save.isPending ? "Salvando…" : "Salvar"}</Button>
          </div>
        </form>
      </SheetContent>
    </Sheet>
  );
}

function AderenciasDialog({ tarefa, onClose }: { tarefa: Tarefa; onClose: () => void }) {
  const filhos = useQuery({ queryKey: ["filhos"], queryFn: listarFilhos });
  const [override, setOverride] = useState<Record<string, string>>({});
  const m = useMutation({
    mutationFn: ({ uid, on }: { uid: string; on: boolean }) => {
      if (!on) return desaderirTarefa(tarefa.id, uid);
      const v = parseNum(override[uid] ?? "");
      return aderirTarefa(tarefa.id, {
        usuarioComumId: uid,
        valorOverride: tarefa.modoCalculo === "ValorDireto" ? v : null,
        pontosOverride: tarefa.modoCalculo === "Pontos" ? v : null,
      });
    },
    onSuccess: (_, v) => toast.success(v.on ? "Filho vinculado à tarefa." : "Filho desvinculado."),
    onError: (e: unknown, v) => {
      const s = (e as { status?: number }).status;
      toast.error(v.on && s === 400 ? "Este filho já está vinculado a esta tarefa." : errMsg(e));
    },
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Filhos em "{tarefa.nome}"</DialogTitle>
          <DialogDescription>
            Vincule ou desvincule filhos. Opcionalmente defina um {tarefa.modoCalculo === "Pontos" ? "número de pontos" : "valor"} diferente para cada um.
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-2">
          {filhos.data?.map((x) => {
            const busy = m.isPending && m.variables?.uid === x.id;
            return (
              <div key={x.id} className="flex items-center gap-2 rounded-lg border px-3 py-2">
                <span className="flex-1 truncate font-semibold">{x.nome}</span>
                <Input
                  className="w-24"
                  placeholder={tarefa.modoCalculo === "Pontos" ? "pts" : "R$"}
                  inputMode="decimal"
                  value={override[x.id] ?? ""}
                  onChange={(e) => setOverride((s) => ({ ...s, [x.id]: e.target.value }))}
                />
                <Button size="sm" disabled={busy} onClick={() => m.mutate({ uid: x.id, on: true })}><Link2 className="size-4" /> Vincular</Button>
                <Button size="sm" variant="outline" disabled={busy} onClick={() => m.mutate({ uid: x.id, on: false })}><Unlink className="size-4" /></Button>
              </div>
            );
          })}
          {filhos.data?.length === 0 && <p className="text-sm text-muted-foreground">Nenhum filho cadastrado.</p>}
        </div>
      </DialogContent>
    </Dialog>
  );
}
