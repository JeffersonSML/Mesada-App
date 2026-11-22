import type { ReactNode } from "react";
import { ShieldCheck, HeartHandshake, LineChart, Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Brand } from "@/components/app/AppShell";
import { useTheme } from "@/lib/theme";

function ThemeToggle() {
  const { theme, toggle } = useTheme();
  return (
    <Button
      variant="outline"
      size="icon"
      onClick={toggle}
      aria-label="Alternar tema"
      className="absolute right-4 top-4 z-10 rounded-full"
    >
      {theme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
    </Button>
  );
}

export function AuthSplit({ title, subtitle, children }: { title: string; subtitle: string; children: ReactNode }) {
  return (
    <div className="relative grid min-h-screen lg:grid-cols-[1.1fr_1fr]">
      <ThemeToggle />
      <aside className="relative hidden overflow-hidden bg-hero-gradient p-12 text-sidebar-foreground lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 bg-dots opacity-60" />
        <div className="relative">
          <Brand />
        </div>
        <div className="relative max-w-md">
          <h1 className="text-4xl font-black leading-tight">
            Mesada com propósito, <span className="text-sidebar-primary">sem dor de cabeça</span>.
          </h1>
          <p className="mt-4 text-base text-sidebar-foreground/75">
            Acompanhe tarefas, aprove conquistas e ensine seus filhos a cuidar do próprio dinheiro —
            tudo em um painel feito para o responsável.
          </p>
          <ul className="mt-8 space-y-3 text-sm">
            {[
              { icon: ShieldCheck, text: "Dados da família isolados e protegidos" },
              { icon: HeartHandshake, text: "Aprovações rápidas, combinados claros" },
              { icon: LineChart, text: "Evolução de cada filho ao longo do tempo" },
            ].map(({ icon: Icon, text }) => (
              <li key={text} className="flex items-center gap-3">
                <span className="grid size-8 place-items-center rounded-lg bg-sidebar-accent">
                  <Icon className="size-4 text-sidebar-primary" />
                </span>
                {text}
              </li>
            ))}
          </ul>
        </div>
        <p className="relative text-xs text-sidebar-foreground/50">© 2026 MesadApp</p>
      </aside>
      <main className="flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-sm">
          <div className="mb-8 lg:hidden">
            <div className="inline-flex rounded-2xl bg-sidebar p-3">
              <Brand />
            </div>
          </div>
          <h2 className="text-2xl font-extrabold tracking-tight">{title}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>
          {children}
        </div>
      </main>
    </div>
  );
}
