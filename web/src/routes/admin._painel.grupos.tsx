import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Layers, Lock, Pencil, Plus, Trash2, X } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, EmptyState, ErrorAlert, Field } from "@/components/app/common";
import { useAdminMe } from "@/components/admin/useAdminMe";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Skeleton } from "@/components/ui/skeleton";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { criarGrupo, editarGrupo, listarGrupos, removerGrupo, type Grupo } from "@/lib/admin-api";
import { ApiError, errMsg } from "@/lib/api";

export const Route = createFileRoute("/admin/_painel/grupos")({
  head: () => ({
    meta: [
      { title: "Grupos — Administração MesadApp" },
      { name: "description", content: "Grupos de administradores e suas permissões." },
      { property: "og:title", content: "Grupos — Administração MesadApp" },
      { property: "og:description", content: "Grupos de administradores e suas permissões." },
      { name: "robots", content: "noindex" },
    ],
  }),
  component: GruposPage,
});

function GruposPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["admin", "grupos"], queryFn: listarGrupos });
  const { isOwner, loading } = useAdminMe();
  const [form, setForm] = useState<Grupo | "new" | null>(null);
  const [rm, setRm] = useState<Grupo | null>(null);
  const del = useMutation({
    mutationFn: (id: string) => removerGrupo(id),
    onSuccess: () => {
      toast.success("Grupo removido.");
      setRm(null);
      qc.invalidateQueries({ queryKey: ["admin", "grupos"] });
    },
    onError: (e) => {
      setRm(null);
      toast.error(
        e instanceof ApiError && e.status === 400
          ? "Não é possível remover: ainda há administradores neste grupo. Mova-os para outro grupo antes."
          : errMsg(e),
      );
    },
  });

  return (
    <>
      <PageHeader
        title="Grupos"
        description="Cada administrador pertence a um grupo, que define suas permissões."
        actions={
          isOwner ? (
            <Button onClick={() => setForm("new")}><Plus className="size-4" /> Novo grupo</Button>
          ) : !loading ? (
            <Badge variant="outline" className="gap-1 py-1.5"><Lock className="size-3" /> Somente leitura — alterações exclusivas do Owner</Badge>
          ) : null
        }
      />
      {q.isError && <ErrorAlert title="Não foi possível carregar os grupos" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-48 rounded-2xl" />
      ) : q.data && q.data.length === 0 ? (
        <EmptyState icon={Layers} title="Nenhum grupo" text="Nenhum grupo cadastrado." />
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {q.data?.map((g) => {
            const perms = Object.entries(g.permissoes ?? {});
            const on = perms.filter(([, v]) => v);
            return (
              <article key={g.id} className="rounded-2xl border bg-card p-5 shadow-soft">
                <div className="flex items-start gap-3">
                  <div className="min-w-0 flex-1">
                    <h3 className="flex items-center gap-2 text-lg font-extrabold">
                      {g.nome}
                      {g.sistema && <Badge variant="outline" className="gap-1"><Lock className="size-3" /> Fixo</Badge>}
                    </h3>
                    <p className="text-sm text-muted-foreground">{g.descricao || "Sem descrição"}</p>
                  </div>
                  {isOwner && !g.sistema && (
                    <div className="flex">
                      <Button size="icon" variant="ghost" aria-label="Editar" onClick={() => setForm(g)}><Pencil className="size-4" /></Button>
                      <Button size="icon" variant="ghost" aria-label="Remover" onClick={() => setRm(g)}><Trash2 className="size-4 text-destructive" /></Button>
                    </div>
                  )}
                </div>
                <div className="mt-4 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                  Permissões · {on.length} de {perms.length} ativas
                </div>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {g.sistema && perms.length === 0 && <Badge className="bg-primary-soft text-primary hover:bg-primary-soft">Acesso total</Badge>}
                  {perms.map(([k, v]) => (
                    <Badge key={k} variant={v ? "secondary" : "outline"} className={v ? "" : "text-muted-foreground line-through"}>{k}</Badge>
                  ))}
                </div>
              </article>
            );
          })}
        </div>
      )}
      {form && isOwner && <GrupoDialog grupo={form === "new" ? null : form} onClose={() => setForm(null)} />}
      <ConfirmDialog
        open={!!rm}
        onOpenChange={(o) => !o && setRm(null)}
        title="Remover grupo?"
        description={`O grupo "${rm?.nome ?? ""}" será removido.`}
        confirmLabel="Remover"
        pending={del.isPending}
        onConfirm={() => rm && del.mutate(rm.id)}
      />
    </>
  );
}

function GrupoDialog({ grupo, onClose }: { grupo: Grupo | null; onClose: () => void }) {
  const qc = useQueryClient();
  const [nome, setNome] = useState(grupo?.nome ?? "");
  const [descricao, setDescricao] = useState(grupo?.descricao ?? "");
  const [perms, setPerms] = useState<[string, boolean][]>(Object.entries(grupo?.permissoes ?? {}));
  const [nova, setNova] = useState("");
  const [err, setErr] = useState<string | null>(null);

  const add = () => {
    const k = nova.trim();
    if (!k || perms.some(([x]) => x === k)) return;
    setPerms((p) => [...p, [k, true]]);
    setNova("");
  };

  const m = useMutation({
    mutationFn: () => {
      if (!nome.trim()) throw new Error("Informe o nome.");
      const d = { nome: nome.trim(), descricao: descricao.trim() || null, permissoes: Object.fromEntries(perms) };
      return grupo ? editarGrupo(grupo.id, d) : criarGrupo(d);
    },
    onSuccess: () => {
      toast.success(grupo ? "Grupo atualizado." : "Grupo criado.");
      qc.invalidateQueries({ queryKey: ["admin", "grupos"] });
      qc.invalidateQueries({ queryKey: ["admin", "administradores"] });
      onClose();
    },
    onError: (e) => setErr(errMsg(e)),
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{grupo ? "Editar grupo" : "Novo grupo"}</DialogTitle>
          <DialogDescription>Defina o nome e quais permissões o grupo possui.</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <Field label="Nome" htmlFor="g-nome"><Input id="g-nome" value={nome} onChange={(e) => setNome(e.target.value)} /></Field>
          <Field label="Descrição" htmlFor="g-desc"><Textarea id="g-desc" value={descricao} onChange={(e) => setDescricao(e.target.value)} /></Field>
          <Field label="Permissões">
            <div className="grid gap-2">
              {perms.map(([k, v], i) => (
                <div key={k} className="flex items-center gap-2 rounded-lg border px-3 py-2">
                  <code className="flex-1 truncate text-sm">{k}</code>
                  <Switch checked={v} onCheckedChange={(b) => setPerms((p) => p.map((x, j) => (j === i ? [x[0], b] : x)))} aria-label={k} />
                  <Button size="icon" variant="ghost" aria-label={`Remover ${k}`} onClick={() => setPerms((p) => p.filter((_, j) => j !== i))}>
                    <X className="size-4" />
                  </Button>
                </div>
              ))}
              <div className="flex gap-2">
                <Input
                  placeholder="nova chave, ex. familias.ler"
                  value={nova}
                  onChange={(e) => setNova(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      add();
                    }
                  }}
                />
                <Button type="button" variant="outline" onClick={add}><Plus className="size-4" /> Adicionar</Button>
              </div>
            </div>
          </Field>
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
