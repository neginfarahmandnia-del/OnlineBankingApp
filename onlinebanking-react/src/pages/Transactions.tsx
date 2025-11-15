import { useEffect, useState } from "react";
import { getTransactions, type Transaction, type Paged } from "../lib/api.js"
import React from "react";

type Sort = "date_desc" | "date_asc" | "amount_desc" | "amount_asc";

export default function TransactionsPage() {
    const [items, setItems] = useState<Transaction[]>([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);

    // Filter / Query
    const [q, setQ] = useState("");
    const [dateFrom, setDateFrom] = useState("");
    const [dateTo, setDateTo] = useState("");
    const [minAmount, setMinAmount] = useState("");
    const [maxAmount, setMaxAmount] = useState("");
    const [sort, setSort] = useState<Sort>("date_desc");
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    async function load() {
        setLoading(true);
        const res = await getTransactions({
            search: q || undefined,
            dateFrom: dateFrom || undefined,
            dateTo: dateTo || undefined,
            minAmount: minAmount ? Number(minAmount) : undefined,
            maxAmount: maxAmount ? Number(maxAmount) : undefined,
            page,
            pageSize,
            sort,
        });
        if (Array.isArray(res)) {
            setItems(res);
            setTotal(res.length);
        } else {
            const p = res as Paged<Transaction>;
            setItems(p.items);
            setTotal(p.total);
        }
        setLoading(false);
    }

    useEffect(() => { load(); /* eslint-disable-next-line */ }, [page, pageSize, sort]);

    function onSubmit(e: React.FormEvent) {
        e.preventDefault();
        setPage(1);
        load();
    }

    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    return (
        <div className="space-y-4">
            <h1 className="text-2xl font-semibold">Transaktionen</h1>

            <form onSubmit={onSubmit} className="grid md:grid-cols-6 gap-2 items-end">
                <div className="md:col-span-2">
                    <label className="block text-sm">Suche</label>
                    <input className="w-full border rounded px-2 py-2" value={q} onChange={(e) => setQ(e.target.value)} placeholder="Beschreibung/Kategorie" />
                </div>
                <div>
                    <label className="block text-sm">Von</label>
                    <input className="w-full border rounded px-2 py-2" type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
                </div>
                <div>
                    <label className="block text-sm">Bis</label>
                    <input className="w-full border rounded px-2 py-2" type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
                </div>
                <div>
                    <label className="block text-sm">Min €</label>
                    <input className="w-full border rounded px-2 py-2" type="number" step="0.01" value={minAmount} onChange={(e) => setMinAmount(e.target.value)} />
                </div>
                <div>
                    <label className="block text-sm">Max €</label>
                    <input className="w-full border rounded px-2 py-2" type="number" step="0.01" value={maxAmount} onChange={(e) => setMaxAmount(e.target.value)} />
                </div>
                <div>
                    <label className="block text-sm">Sortierung</label>
                    <select className="w-full border rounded px-2 py-2" value={sort} onChange={(e) => setSort(e.target.value as Sort)}>
                        <option value="date_desc">Datum ↓</option>
                        <option value="date_asc">Datum ↑</option>
                        <option value="amount_desc">Betrag ↓</option>
                        <option value="amount_asc">Betrag ↑</option>
                    </select>
                </div>
                <div>
                    <button className="w-full bg-blue-600 text-white rounded px-3 py-2" type="submit">Filtern</button>
                </div>
            </form>

            <div className="border rounded">
                <table className="w-full text-sm">
                    <thead className="bg-zinc-50 dark:bg-zinc-900">
                        <tr>
                            <th className="text-left p-2">Datum</th>
                            <th className="text-left p-2">Beschreibung</th>
                            <th className="text-left p-2">Kategorie</th>
                            <th className="text-right p-2">Betrag</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr><td className="p-3" colSpan={4}>Laden…</td></tr>
                        ) : items.length === 0 ? (
                            <tr><td className="p-3" colSpan={4}>Keine Treffer</td></tr>
                        ) : (
                            items.map((t) => (
                                <tr key={String(t.id)} className="border-t">
                                    <td className="p-2">{t.date ?? t.createdAt ?? ""}</td>
                                    <td className="p-2">{t.description ?? ""}</td>
                                    <td className="p-2">{t.category ?? ""}</td>
                                    <td className="p-2 text-right">€ {(t.amount ?? 0).toFixed(2)}</td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <div className="flex items-center gap-2 justify-end">
                <span className="text-sm text-zinc-500">
                    Seite {page} / {totalPages} &middot; {total} Einträge
                </span>
                <select className="border rounded px-2 py-1" value={pageSize} onChange={(e) => setPageSize(Number(e.target.value))}>
                    {[10, 20, 50].map(n => <option key={n} value={n}>{n}/Seite</option>)}
                </select>
                <button className="border rounded px-3 py-1 disabled:opacity-50" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page <= 1}>Zurück</button>
                <button className="border rounded px-3 py-1 disabled:opacity-50" onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page >= totalPages}>Weiter</button>
            </div>
        </div>
    );
}
