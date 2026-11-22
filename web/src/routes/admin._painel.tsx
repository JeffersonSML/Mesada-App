import { Outlet, createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { AdminShell } from "@/components/admin/AdminShell";
import { getAdminToken, mustChangePassword } from "@/lib/admin-api";

export const Route = createFileRoute("/admin/_painel")({
  ssr: false,
  component: AdminLayout,
});

function AdminLayout() {
  const navigate = useNavigate();
  const [ready, setReady] = useState(false);
  useEffect(() => {
    if (!getAdminToken()) navigate({ to: "/admin/login", replace: true });
    else if (mustChangePassword()) navigate({ to: "/admin/trocar-senha", replace: true });
    else setReady(true);
  }, [navigate]);
  if (!ready) return <div className="min-h-screen bg-background" />;
  return (
    <AdminShell>
      <Outlet />
    </AdminShell>
  );
}
