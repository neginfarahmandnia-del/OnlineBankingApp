// src/pages/Profile.tsx
import React from "react";
import { useMe } from "../hooks/useMe";

export default function Profile() {
    const { me, loading, error } = useMe();  // ⬅️ korrekt destrukturieren

    if (loading) return <div className="p-6">Lade Profil…</div>;
    if (error) return <div className="p-6 text-red-600">Fehler: {String(error)}</div>;
    if (!me) return <div className="p-6">Kein Profil gefunden.</div>;

    return (
        <div className="max-w-xl mx-auto p-6">
            <h2 className="text-xl font-semibold mb-2">Profil</h2>
            <div className="rounded border p-4 space-y-1">
                <div><span className="font-medium">UserID:</span> {me.userId ?? "-"}</div>
                <div><span className="font-medium">E-Mail:</span> {me.email ?? "-"}</div>
                <div><span className="font-medium">Rollen:</span> {me.roles?.join(", ") || "-"}</div>
            </div>
        </div>
    );
}
