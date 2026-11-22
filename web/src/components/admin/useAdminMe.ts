import { useQuery } from "@tanstack/react-query";
import { findMe, listarAdmins, listarGrupos } from "@/lib/admin-api";

/** Descobre o administrador logado e se ele pertence ao grupo Owner. */
export function useAdminMe() {
  const admins = useQuery({ queryKey: ["admin", "administradores"], queryFn: listarAdmins });
  const grupos = useQuery({ queryKey: ["admin", "grupos"], queryFn: listarGrupos });
  const me = admins.data ? findMe(admins.data) : undefined;
  const grupo = grupos.data?.find((g) => g.id === me?.grupoId);
  const isOwner = !!me && (me.grupoNome === "Owner" || (grupo?.sistema ?? false));
  return { me, isOwner, loading: admins.isPending || grupos.isPending };
}
