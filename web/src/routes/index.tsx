import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";
import { getToken } from "@/lib/api";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "MesadApp — Painel do Responsável" },
      { name: "description", content: "Acompanhe a mesada, tarefas e saldo dos seus filhos em um só lugar." },
      { property: "og:title", content: "MesadApp — Painel do Responsável" },
      { property: "og:description", content: "Acompanhe a mesada, tarefas e saldo dos seus filhos em um só lugar." },
    ],
  }),
  component: Index,
});

function Index() {
  const navigate = useNavigate();
  useEffect(() => {
    navigate({ to: getToken() ? "/dashboard" : "/login", replace: true });
  }, [navigate]);
  return <div className="min-h-screen bg-background" />;
}
