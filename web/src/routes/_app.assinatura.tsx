import { createFileRoute } from "@tanstack/react-router";
import { CreditCard } from "lucide-react";
import { ComingSoon } from "@/components/app/ComingSoon";

export const Route = createFileRoute("/_app/assinatura")({
  head: () => ({
    meta: [
      { title: "Assinatura — MesadApp" },
      { name: "description", content: "Veja seu plano atual, forma de pagamento e histórico de cobranças." },
      { property: "og:title", content: "Assinatura — MesadApp" },
      { property: "og:description", content: "Veja seu plano atual, forma de pagamento e histórico de cobranças." },
    ],
  }),
  component: () => (
    <ComingSoon
      title="Assinatura"
      description="Gerencie o plano da sua família: status, forma de pagamento e faturas anteriores."
      icon={CreditCard}
      bullets={[
        "Plano atual e data da próxima cobrança",
        "Atualizar cartão ou método de pagamento",
        "Histórico de faturas para download",
      ]}
    />
  ),
});
