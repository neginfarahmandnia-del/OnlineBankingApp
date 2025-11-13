import { useCallback, useEffect, useState } from "react";
import api from "../lib/api";
import React from "react";

type User = { id: string; email?: string | null; roles?: string[] };

export default function Admin() {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [msg, setMsg] = useState<string | null>(null);
    const [saving, setSaving] = useState<string | null>(null); // userId, die gerade gespeichert wird
    const [query, setQuery] = useState("");

    const load = useCallback(async () => {
        setMsg(null);
        setLoading(true);
        try {
            const res = await api.get<User[]>("/api/Users");
            setUsers(res.data ?? []);
        } catch (e: any) {
            setMsg(e?.response?.data?.message || "Fehler beim Laden.");
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        load();
    }, [load]);

    async function setRole(id: string, roleName: "Customer" | "Manager" | "Admin") {
        setMsg(null);
        setSaving(id);
        try {
            await api.put(`/api/Users/${id}/role`, { roleName });
            setMsg("Rolle aktualisiert.");
            await load(); // UI aktualisieren
        } catch (e: any) {
            setMsg(e?.response?.data?.message || "Fehler beim Aktualisieren.");
        } finally {
            setSaving(null);
        }
    }

    const filtered = users.filter((u) => {
        if (!query.trim()) return true;
        const q = query.toLowerCase();
        return (
            (u.email ?? "").toLowerCase().includes(q) ||
            (u.id ?? "").toLowerCase().includes(q) ||
            (u.roles ?? []).join(",").toLowerCase().includes(q)
        );
    });

    if (loading) return <div className="p-6">Lade Benutzer…</div>;

    return (
        <div className="mx-auto max-w-3xl space-y-3 p-6">
            <div className="flex items-center justify-between gap-2">
                <h2 className="text-xl font-semibold">Admin – Benutzer</h2>
                <button
                    onClick={load}
                    className="rounded-md border px-3 py-1.5 text-sm hover:bg-black/5 dark:hover:bg-white/10"
                >
                    Neu laden
                </button>
            </div>

            {msg && <div className="rounded-md border px-3 py-2 text-sm">{msg}</div>}

            <input
                className="w-full rounded-md border px-3 py-2 text-sm bg-transparent"
                placeholder="Suchen (E-Mail, ID, Rolle)…"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
            />

            {filtered.length === 0 && (
                <div className="opacity-70">Keine Benutzer gefunden.</div>
            )}

            {filtered.map((u) => (
                <div
                    key={u.id}
                    className="flex items-center justify-between rounded border p-3"
                >
                    <div className="min-w-0">
                        <div className="truncate font-medium">
                            {u.email || "(ohne E-Mail)"}
                        </div>
                        <div className="truncate text-xs opacity-70">{u.id}</div>
                        {!!u.roles?.length && (
                            <div className="mt-1 text-xs opacity-70">
                                Rollen: {u.roles.join(", ")}
                            </div>
                        )}
                    </div>

                    <div className="flex gap-2">
                        <button
                            className="rounded bg-zinc-200 px-3 py-1 text-sm dark:bg-zinc-700 disabled:opacity-50"
                            disabled={saving === u.id}
                            onClick={() => setRole(u.id, "Customer")}
                        >
                            Customer
                        </button>
                        <button
                            className="rounded bg-zinc-200 px-3 py-1 text-sm dark:bg-zinc-700 disabled:opacity-50"
                            disabled={saving === u.id}
                            onClick={() => setRole(u.id, "Manager")}
                        >
                            Manager
                        </button>
                        <button
                            className="rounded bg-blue-600 px-3 py-1 text-sm text-white disabled:opacity-50"
                            disabled={saving === u.id}
                            onClick={() => setRole(u.id, "Admin")}
                        >
                            Admin
                        </button>
                    </div>
                </div>
            ))}
        </div>
    );
}
