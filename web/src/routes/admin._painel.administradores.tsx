import { createFileRoute } from "@tanstack/react-router";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { AlertTriangle, Copy, Lock, Pencil, UserPlus, UserX } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { ConfirmDialog, ErrorAlert, Field } from "@/components/app/common";
import { useAdminMe } from "@/components/admin/useAdminMe";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { convidarAdmin, desativarAdmin, editarAdmin, listarAdmins, listarGrupos, type Administrador } from "@/lib/admin-api";
import { ApiError, errMsg } from "@/lib/api";

export const Route = createFileRoute("/admin/_painel/administradores")({
  head: () => ({
    meta: [
      { title: "Administradores — Administração MesadApp" },
      { name: "description", content: "Equipe interna com acesso ao painel administrativo." },
      { property: "og:title", content: "Administradores — Administração MesadApp" },
      { property: "og:description", content: "Equipe interna com acesso ao painel administrativo." },
      { name: "robots", content: "noindex" },
    ],
  }),
  component: AdminsPage,
});

const ativo = (s: string) => s.toLowerCase() === "ativo";

function AdminsPage() {
  const qc = useQueryClient();
  const q = useQuery({ queryKey: ["admin", "administradores"], queryFn: listarAdmins });
  const { me, isOwner, loading } = useAdminMe();
  const [form, setForm] = useState<Administrador | "new" | null>(null);
  const [senha, setSenha] = useState<{ nome: string; senha: string } | null>(null);
  const [alvo, setAlvo] = useState<Administrador | null>(null);

  const del = useMutation({
    mutationFn: (id: string) => desativarAdmin(id),
    onSuccess: () => {
      toast.success("Administrador desativado.");
      setAlvo(null);
      qc.invalidateQueries({ queryKey: ["admin", "administradores"] });
    },
    onError: (e) => {
      setAlvo(null);
      toast.error(
        e instanceof ApiError && e.status === 400
          ? "Não é possível desativar: você não pode desativar a si mesmo nem o último Owner."
          : errMsg(e),
      );
    },
  });

  return (
    <>
      <PageHeader
        title="Administradores"
        description="Pessoas da equipe interna com acesso a este painel."
        actions={
          isOwner ? (
            <Button onClick={() => setForm("new")}><UserPlus className="size-4" /> Convidar</Button>
          ) : !loading ? (
            <Badge variant="outline" className="gap-1 py-1.5"><Lock className="size-3" /> Somente leitura — alterações exclusivas do Owner</Badge>
          ) : null
        }
      />
      {q.isError && <ErrorAlert title="Não foi possível carregar os administradores" error={q.error} />}
      {q.isPending ? (
        <Skeleton className="h-48 rounded-2xl" />
      ) : (
        <div className="overflow-x-auto rounded-2xl border bg-card shadow-soft">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>E-mail</TableHead>
                <TableHead>Grupo</TableHead>
                <TableHead>Status</TableHead>
                {isOwner && <TableHead className="text-right">Ações</TableHead>}
              </TableRow>
            </TableHeader>
            <TableBody>
              {q.data?.map((a) => (
                <TableRow key={a.id}>
                  <TableCell className="font-semibold">
                    {a.nome} {me?.id === a.id && <span className="text-xs font-normal text-muted-foreground">(você)</span>}
                  </TableCell>
                  <TableCell className="text-muted-foreground">{a.email}</TableCell>
                  <TableCell><Badge variant="secondary">{a.grupoNome}</Badge></TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      <Badge variant={ativo(a.status) ? "secondary" : "outline"}>{ativo(a.status) ? "Ativo" : a.status}</Badge>
                      {a.deveTrocarSenha && <Badge variant="outline">Senha temporária</Badge>}
                    </div>
                  </TableCell>
                  {isOwner && (
                    <TableCell className="text-right">
                      <Button size="icon" variant="ghost" aria-label="Editar" onClick={() => setForm(a)}><Pencil className="size-4" /></Button>
                      {ativo(a.status) && me?.id !== a.id && (
                        <Button size="icon" variant="ghost" aria-label="Desativar" onClick={() => setAlvo(a)}>
                          <UserX className="size-4 text-destructive" />
                        </Button>
                      )}
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {form && isOwner && (
        <AdminDialog
          admin={form === "new" ? null : form}
          onClose={() => setForm(null)}
          onInvited={(nome, s) => {
            setForm(null);
            setSenha({ nome, senha: s });
          }}
        />
      )}

      <Dialog open={!!senha} onOpenChange={(o) => !o && setSenha(null)}>
        <DialogContent onInteractOutside={(e) => e.preventDefault()}>
          <DialogHeader>
            <DialogTitle>Senha temporária de {senha?.nome}</DialogTitle>
            <DialogDescription>Envie esta senha ao novo administrador. No primeiro acesso ele será obrigado a trocá-la.</DialogDescription>
          </DialogHeader>
          <div className="rounded-2xl border-2 border-dashed border-primary/40 bg-primary-soft p-6 text-center">
            <div className="select-all break-all font-mono text-2xl font-black tracking-wider text-primary">{senha?.senha}</div>
          </div>
          <Alert variant="destructive">
            <AlertTriangle className="size-4" />
            <AlertTitle>Esta senha só aparece uma vez</AlertTitle>
            <AlertDescription>Ao fechar esta janela não será possível vê-la de novo. Copie antes de sair.</AlertDescription>
          </Alert>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() =>
                senha &&
                navigator.clipboard.writeText(senha.senha).then(
                  () => toast.success("Senha copiada."),
                  () => toast.error("Não foi possível copiar."),
                )
              }
            >
              <Copy className="size-4" /> Copiar senha
            </Button>
            <Button onClick={() => setSenha(null)}>Já copiei, fechar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={!!alvo}
        onOpenChange={(o) => !o && setAlvo(null)}
        title="Desativar administrador?"
        description={`${alvo?.nome ?? ""} perderá o acesso ao painel administrativo.`}
        confirmLabel="Desativar"
        pending={del.isPending}
        onConfirm={() => alvo && del.mutate(alvo.id)}
      />
    </>
  );
}

function AdminDialog({
  admin,
  onClose,
  onInvited,
}: {
  admin: Administrador | null;
  onClose: () => void;
  onInvited: (nome: string, senha: string) => void;
}) {
  const qc = useQueryClient();
  const grupos = useQuery({ queryKey: ["admin", "grupos"], queryFn: listarGrupos });
  const [nome, setNome] = useState(admin?.nome ?? "");
  const [email, setEmail] = useState(admin?.email ?? "");
  const [grupoId, setGrupoId] = useState(admin?.grupoId ?? "");
  const [err, setErr] = useState<string | null>(null);

  const m = useMutation({
    mutationFn: async () => {
      if (!nome.trim()) throw new Error("Informe o nome.");
      if (!grupoId) throw new Error("Escolha o grupo.");
      if (admin) {
        await editarAdmin(admin.id, { nome: nome.trim(), grupoId });
        return null;
      }
      if (!email.trim()) throw new Error("Informe o e-mail.");
      return convidarAdmin({ nome: nome.trim(), email: email.trim(), grupoId });
    },
    onSuccess: (r) => {
      qc.invalidateQueries({ queryKey: ["admin", "administradores"] });
      if (r) onInvited(r.administrador?.nome ?? nome, r.senhaTemporaria);
      else {
        toast.success("Administrador atualizado.");
        onClose();
      }
    },
    onError: (e) => setErr(e instanceof ApiError && e.status === 409 ? "Já existe um administrador com este e-mail." : errMsg(e)),
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{admin ? "Editar administrador" : "Convidar administrador"}</DialogTitle>
          <DialogDescription>{admin ? "Altere nome ou grupo." : "Uma senha temporária será gerada para o primeiro acesso."}</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <Field label="Nome" htmlFor="ad-nome"><Input id="ad-nome" value={nome} onChange={(e) => setNome(e.target.value)} /></Field>
          <Field label="E-mail" htmlFor="ad-email">
            <Input id="ad-email" type="email" value={email} disabled={!!admin} onChange={(e) => setEmail(e.target.value)} />
          </Field>
          <Field label="Grupo">
            <Select value={grupoId} onValueChange={setGrupoId}>
              <SelectTrigger><SelectValue placeholder={grupos.isPending ? "Carregando…" : "Selecione"} /></SelectTrigger>
              <SelectContent>
                {grupos.data?.map((g) => <SelectItem key={g.id} value={g.id}>{g.nome}</SelectItem>)}
              </SelectContent>
            </Select>
          </Field>
          {err && <p className="text-sm font-medium text-destructive">{err}</p>}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>Cancelar</Button>
          <Button disabled={m.isPending} onClick={() => { setErr(null); m.mutate(); }}>
            {m.isPending ? "Salvando…" : admin ? "Salvar" : "Convidar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
