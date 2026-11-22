import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Lock, Pencil, Plus, Tags, Trash2 } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { criarCategoria, editarCategoria, errMsg, listarCategorias, removerCategoria, type Categoria } from "@/lib/api";

export const Route = createFileRoute("/_app/categorias")({
  head: () => ({
    meta: [
      { title: "Categorias — MesadApp" },
      { name: "description", content: "Organize as tarefas da família em categorias e subcategorias." },
      { property: "og:title", content: "Categorias — MesadApp" },
      { property: "og:description", content: "Organize as tarefas da família em categorias e subcategorias." },
    ],
  }),
  component: CategoriasPage,
});

function CategoriasPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["categorias"], queryFn: listarCategorias });
  const [edit, setEdit] = useState<Categoria | "new" | null>(null);
  const [rm, setRm] = useState<Categoria | null>(null);
  const del = useMutation({
    mutationFn: (id: string) => removerCategoria(id),
    onSuccess: () => {
      toast.success("Categoria removida.");
      setRm(null);
      qc.invalidateQueries({ queryKey: ["categorias"] });
    },
    onError: (e) => toast.error(errMsg(e)),
  });

  const all = q.data ?? [];
  const roots = all.filter((c) => !c.parentId || !all.some((p) => p.id === c.parentId));
  const children = (id: string) => all.filter((c) => c.parentId === id);

  const Row = ({ c, sub }: { c: Categoria; sub?: boolean }) => (
    <div className={`flex items-center gap-3 border-b px-5 py-3 last:border-b-0 ${sub ? "pl-12" : ""}`}>
      <span className={`flex-1 ${sub ? "text-sm" : "font-semibold"}`}>{c.nome}</span>
      {c.sistema ? (
        <Badge variant="outline" className="gap-1"><Lock className="size-3" /> Padrão</Badge>
      ) : (
        <>
          <Button size="icon" variant="ghost" aria-label="Editar" onClick={() => setEdit(c)}><Pencil className="size-4" /></Button>
          <Button size="icon" variant="ghost" aria-label="Remover" onClick={() => setRm(c)}><Trash2 className="size-4 text-destructive" /></Button>
        </>
      )}
    </div>
  );

  return (
    <>
      <PageHeader
        title="Categorias"
        description="Agrupe as tarefas. Categorias padrão da plataforma não podem ser alteradas."
        actions={<Button onClick={() => setEdit("new")}><Plus className="size-4" /> Nova categoria</Button>}
      />
      {q.isError && <ErrorAlert title="Não foi possível carregar as categorias" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-48 rounded-2xl" />
      ) : all.length === 0 ? (
        <EmptyState icon={Tags} title="Nenhuma categoria" text="Crie a primeira categoria para organizar as tarefas." />
      ) : (
        <div className="overflow-hidden rounded-2xl border bg-card shadow-soft">
          {roots.map((c) => (
            <div key={c.id}>
              <Row c={c} />
              {children(c.id).map((s) => <Row key={s.id} c={s} sub />)}
            </div>
          ))}
        </div>
      )}
      {edit && <CategoriaDialog cat={edit === "new" ? null : edit} roots={roots} onClose={() => setEdit(null)} />}
      <ConfirmDialog
        open={!!rm}
        onOpenChange={(o) => !o && setRm(null)}
        title="Remover categoria?"
        description={`"${rm?.nome ?? ""}" será removida.`}
        confirmLabel="Remover"
        pending={del.isPending}
        onConfirm={() => rm && del.mutate(rm.id)}
      />
    </>
  );
}

function CategoriaDialog({ cat, roots, onClose }: { cat: Categoria | null; roots: Categoria[]; onClose: () => void }) {
  const qc = useQueryClient();
  const [nome, setNome] = useState(cat?.nome ?? "");
  const [parent, setParent] = useState("none");
  const [err, setErr] = useState<string | null>(null);
  const m = useMutation({
    mutationFn: () => {
      if (!nome.trim()) throw new Error("Informe o nome.");
      return cat ? editarCategoria(cat.id, nome.trim()) : criarCategoria({ nome: nome.trim(), parentId: parent === "none" ? null : parent });
    },
    onSuccess: () => {
      toast.success(cat ? "Categoria atualizada." : "Categoria criada.");
      qc.invalidateQueries({ queryKey: ["categorias"] });
      onClose();
    },
    onError: (e) => setErr(errMsg(e)),
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{cat ? "Editar categoria" : "Nova categoria"}</DialogTitle>
          <DialogDescription>{cat ? "Altere o nome." : "Pode ser uma categoria principal ou subcategoria."}</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <Field label="Nome" htmlFor="cat-nome"><Input id="cat-nome" value={nome} onChange={(e) => setNome(e.target.value)} /></Field>
          {!cat && (
            <Field label="Categoria pai">
              <Select value={parent} onValueChange={setParent}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">Nenhuma (principal)</SelectItem>
                  {roots.map((r) => <SelectItem key={r.id} value={r.id}>{r.nome}</SelectItem>)}
                </SelectContent>
              </Select>
            </Field>
          )}
          {err && <p className="text-sm font-medium text-destructive">{err}</p>}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button disabled={m.isPending} onClick={() => { setErr(null); m.mutate(); }}>{m.isPending ? "Salvando…" : "Salvar"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
