import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { TrocarSenhaForm } from "@/components/admin/TrocarSenhaForm";
import { clearAdminSession, getAdminToken, mustChangePassword } from "@/lib/admin-api";

export const Route = createFileRoute("/admin/trocar-senha")({
  ssr: false,
  head: () => ({
    meta: [
      { title: "Trocar senha — Administração MesadApp" },
      { name: "description", content: "Defina uma nova senha para continuar no painel administrativo." },
      { property: "og:title", content: "Trocar senha — Administração MesadApp" },
      { property: "og:description", content: "Defina uma nova senha para continuar no painel administrativo." },
      { name: "robots", content: "noindex" },
    ],
  }),
  component: TrocarSenhaPage,
});

function TrocarSenhaPage() {
  const navigate = useNavigate();
  const [ready, setReady] = useState(false);
  const [forced, setForced] = useState(false);
  useEffect(() => {
    if (!getAdminToken()) navigate({ to: "/admin/login", replace: true });
    else {
      setForced(mustChangePassword());
      setReady(true);
    }
  }, [navigate]);
  if (!ready) return <div className="min-h-screen bg-background" />;

  return (
    <div className="grid min-h-screen place-items-center bg-sidebar px-6">
      <div className="w-full max-w-sm rounded-2xl border bg-card p-8 shadow-lift">
        <span className="grid size-12 place-items-center rounded-xl bg-primary-soft text-primary"><KeyRound className="size-6" /></span>
        <h1 className="mt-4 text-2xl font-extrabold">Defina sua nova senha</h1>
        <p className="mb-6 mt-1 text-sm text-muted-foreground">
          {forced
            ? "Você entrou com uma senha temporária. Troque-a para liberar o acesso ao painel."
            : "Escolha uma nova senha para sua conta."}
        </p>
        <TrocarSenhaForm
          onDone={() => {
            toast.success("Senha alterada.");
            navigate({ to: "/admin/administradores", replace: true });
          }}
        />
        <Button
          variant="link"
          className="mt-4 px-0 text-muted-foreground"
          onClick={() => {
            clearAdminSession();
            navigate({ to: "/admin/login", replace: true });
          }}
        >
          Sair
        </Button>
      </div>
    </div>
  );
}
