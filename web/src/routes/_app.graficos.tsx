import { createFileRoute } from "@tanstack/react-router";
import { BarChart3 } from "lucide-react";
import { ComingSoon } from "@/components/app/ComingSoon";

export const Route = createFileRoute("/_app/graficos")({
  head: () => ({
    meta: [
      { title: "Gráficos de Desempenho — MesadApp" },
      { name: "description", content: "Acompanhe a evolução das tarefas e da mesada dos seus filhos ao longo do tempo." },
      { property: "og:title", content: "Gráficos de Desempenho — MesadApp" },
      { property: "og:description", content: "Acompanhe a evolução das tarefas e da mesada dos seus filhos ao longo do tempo." },
    ],
  }),
  component: () => (
    <ComingSoon
      title="Gráficos de Desempenho"
      description="Visualize a evolução de cada filho: tarefas cumpridas, valores recebidos e consistência mês a mês."
      icon={BarChart3}
      bullets={[
        "Tarefas concluídas por semana e por categoria",
        "Comparativo entre filhos e metas combinadas",
        "Tendência do saldo devedor ao longo dos meses",
      ]}
    />
  ),
});
