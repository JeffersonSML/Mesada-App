import { Link, useNavigate } from "@tanstack/react-router";
import {
  LayoutDashboard,
  Users,
  ClipboardCheck,
  Receipt,
  BarChart3,
  KeyRound,
  CreditCard,
  LogOut,
  Menu,
  Moon,
  Sun,
  Tags,
  ListTodo,
  Bell,
  Home,
  UserCircle,
} from "lucide-react";
import { useId, useState, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { clearToken } from "@/lib/api";
import { useTheme } from "@/lib/theme";
import { cn } from "@/lib/utils";

export const NAV = [
  { to: "/dashboard", label: "Dashboard da Família", icon: LayoutDashboard },
  { to: "/familia", label: "Minha Família", icon: Home },
  { to: "/filhos", label: "Gestão de Filhos", icon: Users },
  { to: "/categorias", label: "Categorias", icon: Tags },
  { to: "/tarefas", label: "Tarefas", icon: ListTodo },
  { to: "/aprovacoes", label: "Aprovações Pendentes", icon: ClipboardCheck },
  { to: "/extrato", label: "Extrato e Histórico", icon: Receipt },
  { to: "/graficos", label: "Gráficos de Desempenho", icon: BarChart3 },
  { to: "/acessos", label: "Controle de Acessos", icon: KeyRound },
  { to: "/notificacoes", label: "Notificações", icon: Bell },
  { to: "/assinatura", label: "Assinatura", icon: CreditCard },
  { to: "/perfil", label: "Meu Perfil", icon: UserCircle },
] as const;

function CoinMark({ size = 36 }: { size?: number }) {
  const id = useId();
  return (
    <svg width={size} height={size} viewBox="0 0 200 200">
      <defs>
        <radialGradient id={`coinGrad-${id}`} cx="35%" cy="28%" r="80%">
          <stop offset="0%" stopColor="#FF9A5C" />
          <stop offset="60%" stopColor="#F4601F" />
          <stop offset="100%" stopColor="#C8460C" />
        </radialGradient>
        <linearGradient id={`mGrad-${id}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#14110F" />
          <stop offset="100%" stopColor="#0A0807" />
        </linearGradient>
      </defs>
      <circle cx="100" cy="100" r="97" fill="none" stroke="#A83B09" strokeWidth="7" strokeDasharray="2.5 3" />
      <circle cx="100" cy="100" r="92" fill="#D6540F" />
      <circle cx="100" cy="100" r="92" fill="none" stroke="#FFE4C7" strokeWidth="5" opacity="0.4" strokeDasharray="144 434" transform="rotate(-135 100 100)" />
      <circle cx="100" cy="100" r="76" fill={`url(#coinGrad-${id})`} />
      <circle cx="100" cy="100" r="76" fill="none" stroke="#A83B09" strokeWidth="2.5" opacity="0.5" />
      <text x="103" y="144" textAnchor="middle" fontFamily="Inter, sans-serif" fontWeight="900" fontSize="106" fill="#0A0807" opacity="0.45">M</text>
      <text x="100" y="141" textAnchor="middle" fontFamily="Inter, sans-serif" fontWeight="900" fontSize="106" fill={`url(#mGrad-${id})`}>M</text>
    </svg>
  );
}

export function Brand({ compact = false }: { compact?: boolean }) {
  return (
    <div className="flex items-center gap-2.5">
      <CoinMark size={36} />
      {!compact && (
        <div className="leading-tight">
          <div className="font-display text-base font-extrabold tracking-tight text-sidebar-foreground">
            MesadApp
          </div>
          <div className="text-[11px] font-medium uppercase tracking-wider text-sidebar-foreground/60">
            Painel do responsável
          </div>
        </div>
      )}
    </div>
  );
}

function NavLinks({ onNavigate }: { onNavigate?: (() => void) | undefined }) {
  return (
    <nav className="flex flex-col gap-1">
      {NAV.map(({ to, label, icon: Icon }) => (
        <Link
          key={to}
          to={to}
          onClick={onNavigate}
          className="group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-sidebar-foreground/75 transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
          activeProps={{
            className: "bg-sidebar-accent text-sidebar-accent-foreground shadow-soft",
          }}
        >
          {({ isActive }) => (
            <>
              <Icon
                className={cn(
                  "size-4.5 shrink-0 transition-colors",
                  isActive ? "text-sidebar-primary" : "text-sidebar-foreground/50 group-hover:text-sidebar-foreground",
                )}
              />
              <span className="truncate">{label}</span>
            </>
          )}
        </Link>
      ))}
    </nav>
  );
}

function SidebarBody({ onNavigate, onLogout }: { onNavigate?: () => void; onLogout: () => void }) {
  return (
    <div className="flex h-full flex-col bg-sidebar text-sidebar-foreground">
      <div className="px-5 pb-4 pt-6">
        <Brand />
      </div>
      <div className="flex-1 overflow-y-auto px-3">
        <NavLinks onNavigate={onNavigate} />
      </div>
      <div className="border-t border-sidebar-border p-3">
        <button
          onClick={onLogout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-sidebar-foreground/75 transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
        >
          <LogOut className="size-4.5 text-sidebar-foreground/50" />
          Sair da conta
        </button>
      </div>
    </div>
  );
}

export function AppShell({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const { theme, toggle } = useTheme();
  const [open, setOpen] = useState(false);

  const logout = () => {
    clearToken();
    navigate({ to: "/login" });
  };

  return (
    <div className="flex min-h-screen bg-background">
      <aside className="sticky top-0 hidden h-screen w-68 shrink-0 lg:block">
        <SidebarBody onLogout={logout} />
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between gap-3 border-b bg-background/85 px-4 backdrop-blur-md sm:px-6 lg:px-8">
          <div className="flex items-center gap-2 lg:hidden">
            <Sheet open={open} onOpenChange={setOpen}>
              <SheetTrigger asChild>
                <Button variant="ghost" size="icon" aria-label="Abrir menu">
                  <Menu className="size-5" />
                </Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-72 border-sidebar-border p-0">
                <SheetTitle className="sr-only">Navegação</SheetTitle>
                <SidebarBody onNavigate={() => setOpen(false)} onLogout={logout} />
              </SheetContent>
            </Sheet>
            <span className="font-display text-base font-extrabold">MesadApp</span>
          </div>
          <div className="hidden text-sm text-muted-foreground lg:block">
            Bem-vindo(a) de volta. Aqui está o resumo da sua família.
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="icon"
              onClick={toggle}
              aria-label="Alternar tema"
              className="rounded-full"
            >
              {theme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
            </Button>
          </div>
        </header>

        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
          <div className="mx-auto w-full max-w-6xl">{children}</div>
        </main>
      </div>
    </div>
  );
}
