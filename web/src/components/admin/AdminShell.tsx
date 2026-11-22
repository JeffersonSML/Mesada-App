import { Link, useNavigate } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { Layers, LogOut, Menu, Moon, ShieldHalf, Sun, UserCog, UsersRound } from "lucide-react";
import { useState, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { clearAdminSession } from "@/lib/admin-api";
import { useTheme } from "@/lib/theme";
import { useAdminMe } from "./useAdminMe";

const ADMIN_NAV = [
  { to: "/admin/administradores", label: "Administradores", icon: UsersRound },
  { to: "/admin/grupos", label: "Grupos", icon: Layers },
  { to: "/admin/perfil", label: "Meu perfil", icon: UserCog },
] as const;

export function AdminBrand() {
  return (
    <div className="flex items-center gap-2.5">
      <span className="grid size-9 place-items-center rounded-xl bg-sidebar-primary text-sidebar-primary-foreground">
        <ShieldHalf className="size-5" />
      </span>
      <div className="leading-tight">
        <div className="font-display text-base font-extrabold text-sidebar-foreground">MesadApp</div>
        <div className="text-[11px] font-medium uppercase tracking-wider text-sidebar-foreground/60">Administração interna</div>
      </div>
    </div>
  );
}

function Body({ onNavigate, onLogout }: { onNavigate?: () => void; onLogout: () => void }) {
  const { me, isOwner } = useAdminMe();
  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      <div className="px-5 pb-4 pt-6"><AdminBrand /></div>
      <nav className="flex flex-1 flex-col gap-1 px-3">
        {ADMIN_NAV.map(({ to, label, icon: Icon }) => (
          <Link
            key={to}
            to={to}
            {...(onNavigate ? { onClick: onNavigate } : {})}
            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-sidebar-foreground/75 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
            activeProps={{ className: "bg-sidebar-accent text-sidebar-accent-foreground" }}
          >
            <Icon className="size-4.5 text-sidebar-primary" />
            {label}
          </Link>
        ))}
      </nav>
      <div className="border-t border-sidebar-border p-3">
        {me && (
          <div className="mb-2 px-3 text-xs">
            <div className="truncate font-semibold">{me.nome}</div>
            <div className="mt-1 flex items-center gap-1.5 text-sidebar-foreground/60">
              {me.grupoNome}
              {isOwner && <Badge className="h-4 bg-sidebar-primary px-1.5 text-[10px] text-sidebar-primary-foreground">Owner</Badge>}
            </div>
          </div>
        )}
        <button
          onClick={onLogout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-sidebar-foreground/75 hover:bg-sidebar-accent"
        >
          <LogOut className="size-4.5 text-sidebar-foreground/50" /> Sair
        </button>
      </div>
    </div>
  );
}

export function AdminShell({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const { theme, toggle } = useTheme();
  const [open, setOpen] = useState(false);
  const logout = () => {
    qc.removeQueries({ queryKey: ["admin"] });
    clearAdminSession();
    navigate({ to: "/admin/login", replace: true });
  };
  return (
    <div className="flex min-h-screen bg-background">
      <aside className="sticky top-0 hidden h-screen w-64 shrink-0 lg:block"><Body onLogout={logout} /></aside>
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between gap-3 border-b bg-background/85 px-4 backdrop-blur-md sm:px-6 lg:px-8">
          <div className="flex items-center gap-2 lg:hidden">
            <Sheet open={open} onOpenChange={setOpen}>
              <SheetTrigger asChild>
                <Button variant="ghost" size="icon" aria-label="Abrir menu"><Menu className="size-5" /></Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-72 p-0">
                <SheetTitle className="sr-only">Navegação</SheetTitle>
                <Body onNavigate={() => setOpen(false)} onLogout={logout} />
              </SheetContent>
            </Sheet>
            <span className="font-display font-extrabold">Admin</span>
          </div>
          <div className="hidden text-sm text-muted-foreground lg:block">Painel administrativo — equipe interna</div>
          <Button variant="outline" size="icon" className="rounded-full" onClick={toggle} aria-label="Alternar tema">
            {theme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
          </Button>
        </header>
        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
          <div className="mx-auto w-full max-w-6xl">{children}</div>
        </main>
      </div>
    </div>
  );
}
