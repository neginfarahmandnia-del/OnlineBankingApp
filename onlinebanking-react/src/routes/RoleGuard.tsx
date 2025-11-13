import { Navigate, Outlet } from "react-router-dom";
import { useMe } from "../hooks/useMe";
import React from "react";

export default function RoleGuard({ allow }: { allow: string[] }) {
    const { me, loading } = useMe(true);
    if (loading) return null;
    const roles = me?.roles ?? [];
    const ok = roles.some((r) => allow.includes(r));
    return ok ? <Outlet /> : <Navigate to="/dashboard" replace />;
}
