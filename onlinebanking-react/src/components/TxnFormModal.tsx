import { useEffect, useMemo, useRef, useState } from "react";
import { X } from "lucide-react";
import React from "react";

export type TxnFormValues = {
    accountId: string;
    type: "deposit" | "withdrawal";
    amount: number;        // positiv
    category?: string;
    description?: string;
};

export default function TxnFormModal({
    open,
    onClose,
    onSubmit,
    accounts,
    categories,
    defaultAccountId,
    defaultType = "deposit",
}: {
    open: boolean;
    onClose: () => void;
    onSubmit: (v: TxnFormValues) => Promise<void> | void;
    accounts: { id: string; name: string }[];
    /** optional: Kategorienvorschläge für Dropdown */
    categories?: string[];
    /** optional: vorausgewähltes Konto */
    defaultAccountId?: string;
    /** optional: vorausgewählter Typ */
    defaultType?: "deposit" | "withdrawal";
}) {
    const [form, setForm] = useState<TxnFormValues>({
        accountId: defaultAccountId ?? "",
        type: defaultType,
        amount: 0,
        category: "",
        description: "",
    });
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const firstFieldRef = useRef<HTMLSelectElement | HTMLInputElement | null>(null);

    // Beim Öffnen Formular zurücksetzen + Fokus setzen
    useEffect(() => {
        if (!open) return;
        setForm({
            accountId: defaultAccountId ?? "",
            type: defaultType,
            amount: 0,
            category: "",
            description: "",
        });
        setError(null);
        // kleinen Timeout, damit das Element sicher gemountet ist
        const t = setTimeout(() => firstFieldRef.current?.focus(), 0);
        return () => clearTimeout(t);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [open]);

    // Deduplizierte, alphabetisch sortierte Kategorien
    const options = useMemo(() => {
        if (!categories || categories.length === 0) return [];
        return Array.from(new Set(categories.filter(Boolean))).sort((a, b) =>
            a.localeCompare(b)
        );
    }, [categories]);

    const change = <K extends keyof TxnFormValues>(k: K, v: TxnFormValues[K]) =>
        setForm((f) => ({ ...f, [k]: v }));

    // ESC schließt Modal
    useEffect(() => {
        if (!open) return;
        const onKey = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose();
        };
        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
    }, [open, onClose]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!form.accountId) return setError("Bitte ein Konto wählen.");
        if (!form.amount || form.amount <= 0)
            return setError("Betrag muss größer als 0 sein.");

        try {
            setSubmitting(true);
            await onSubmit({
                accountId: form.accountId,
                type: form.type,
                amount: Number(form.amount),
                category: form.category?.trim() || undefined,
                description: form.description?.trim() || undefined,
            });
            onClose();
        } finally {
            setSubmitting(false);
        }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/50 p-4">
            <div className="w-full max-w-lg rounded-2xl bg-white p-4 shadow-xl dark:bg-zinc-900">
                <div className="mb-3 flex items-center justify-between">
                    <h3 className="text-lg font-semibold">Ein-/Auszahlung</h3>
                    <button
                        type="button"
                        onClick={onClose}
                        className="rounded-lg p-1 hover:bg-black/5 dark:hover:bg-white/10"
                        aria-label="Modal schließen"
                    >
                        <X size={18} />
                    </button>
                </div>

                {error && (
                    <p className="mb-2 rounded-md border border-red-500/40 bg-red-50 px-3 py-2 text-sm text-red-700 dark:bg-red-950/20 dark:text-red-300">
                        {error}
                    </p>
                )}

                <form onSubmit={handleSubmit} className="space-y-3">
                    {/* Konto */}
                    <div>
                        <label className="block text-sm opacity-80">Konto</label>
                        <select
                            ref={firstFieldRef as any}
                            className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                            value={form.accountId}
                            onChange={(e) => change("accountId", e.target.value)}
                            required
                        >
                            <option value="">— wählen —</option>
                            {accounts.map((a) => (
                                <option key={a.id} value={a.id}>
                                    {a.name}
                                </option>
                            ))}
                        </select>
                        {accounts.length === 0 && (
                            <div className="mt-1 text-xs opacity-70">
                                Keine Konten gefunden. Bitte zuerst ein Konto anlegen.
                            </div>
                        )}
                    </div>

                    {/* Typ + Betrag */}
                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        <div>
                            <label className="block text-sm opacity-80">Typ</label>
                            <select
                                className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                                value={form.type}
                                onChange={(e) =>
                                    change("type", e.target.value as "deposit" | "withdrawal")
                                }
                            >
                                <option value="deposit">Einzahlung</option>
                                <option value="withdrawal">Auszahlung</option>
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm opacity-80">Betrag</label>
                            <input
                                type="number"
                                step="0.01"
                                min={0}
                                inputMode="decimal"
                                className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                                value={Number.isFinite(form.amount) ? form.amount : 0}
                                onChange={(e) => change("amount", Number(e.target.value))}
                                placeholder="z. B. 125,50"
                                required
                            />
                        </div>
                    </div>

                    {/* Kategorie + Beschreibung */}
                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        <div>
                            <label className="block text-sm opacity-80">Kategorie</label>
                            {options.length > 0 ? (
                                <select
                                    className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                                    value={form.category ?? ""}
                                    onChange={(e) => change("category", e.target.value)}
                                >
                                    <option value="">— wählen —</option>
                                    {options.map((c) => (
                                        <option key={c} value={c}>
                                            {c}
                                        </option>
                                    ))}
                                </select>
                            ) : (
                                <input
                                    className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                                    value={form.category ?? ""}
                                    onChange={(e) => change("category", e.target.value)}
                                    placeholder="z. B. Miete, Lebensmittel …"
                                />
                            )}
                        </div>

                        <div>
                            <label className="block text-sm opacity-80">Beschreibung</label>
                            <input
                                className="mt-1 w-full rounded-md border px-3 py-2 bg-transparent"
                                value={form.description ?? ""}
                                onChange={(e) => change("description", e.target.value)}
                                placeholder="optional"
                            />
                        </div>
                    </div>

                    {/* Aktionen */}
                    <div className="flex justify-end gap-2 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="rounded-md border px-3 py-2 text-sm"
                        >
                            Abbrechen
                        </button>
                        <button
                            disabled={submitting}
                            className="rounded-md bg-blue-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                        >
                            {submitting ? "Speichern…" : "Speichern"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
