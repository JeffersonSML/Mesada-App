import type { LucideIcon } from "lucide-react";
import { Sparkles } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { PageHeader } from "./PageHeader";

export function ComingSoon({
  title,
  description,
  icon: Icon,
  bullets,
}: {
  title: string;
  description: string;
  icon: LucideIcon;
  bullets: string[];
}) {
  return (
    <>
      <PageHeader title={title} description={description} />
      <div className="relative overflow-hidden rounded-2xl border bg-card p-8 shadow-soft sm:p-12">
        <div className="pointer-events-none absolute -right-16 -top-16 size-64 rounded-full bg-primary-soft blur-3xl" />
        <div className="pointer-events-none absolute -bottom-20 -left-10 size-56 rounded-full bg-honey-soft blur-3xl" />
        <div className="relative mx-auto flex max-w-xl flex-col items-center text-center">
          <span className="grid size-16 place-items-center rounded-2xl bg-primary-soft text-primary">
            <Icon className="size-8" />
          </span>
          <Badge className="mt-5 gap-1 bg-honey-soft text-honey-foreground hover:bg-honey-soft dark:text-honey">
            <Sparkles className="size-3" /> Em breve
          </Badge>
          <h2 className="mt-3 text-xl font-extrabold">Estamos preparando esta tela</h2>
          <p className="mt-2 text-sm text-muted-foreground">{description}</p>
          <ul className="mt-6 grid w-full gap-2 text-left sm:grid-cols-1">
            {bullets.map((b) => (
              <li
                key={b}
                className="flex items-start gap-3 rounded-lg border bg-background/60 px-4 py-3 text-sm"
              >
                <span className="mt-1.5 size-1.5 shrink-0 rounded-full bg-primary" />
                <span>{b}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </>
  );
}
