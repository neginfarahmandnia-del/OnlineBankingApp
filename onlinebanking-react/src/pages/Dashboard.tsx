import { useEffect, useMemo, useState } from "react";
import { getTransactions, type Transaction } from "../lib/api";
import React from "react";

export default function Dashboard() {
    const [tx, setTx] = useState<Transaction[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        (async () => {
            setLoading(true);
            const list = await getTransactions({ page: 1, pageSize: 10, sort: "date_desc" });
            const items = Array.isArray(list) ? list : list.items;
            setTx(items);
            setLoading(false);
        })();
    }, []);

    const totalIn = useMemo(() => tx.filter(t => /deposit/i.test(t.type)).reduce((s, t) => s + (t.amount || 0), 0), [tx]);
    const totalOut = useMemo(() => tx.filter(t => /with(draw|drawal)|transfer/i.test(t.type)).reduce((s, t) => s + (t.amount || 0), 0), [tx]);

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-semibold">Dashboard</h1>
            <div className="grid sm:grid-cols-2 gap-3">
                <div className="border rounded p-4">
                    <div className="text-sm text-zinc-500">Eingänge (letzte Seite)</div>
                    <div className="text-2xl font-semibold">€ {totalIn.toFixed(2)}</div>
                </div>
                <div className="border rounded p-4">
                    <div className="text-sm text-zinc-500">Ausgänge (letzte Seite)</div>
                    <div className="text-2xl font-semibold">€ {totalOut.toFixed(2)}</div>
                </div>
            </div>

            <div className="border rounded">
                <div className="px-4 py-2 border-b font-medium">Neueste Transaktionen</div>
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
                        ) : tx.length === 0 ? (
                            <tr><td className="p-3" colSpan={4}>Keine Daten</td></tr>
                        ) : (
                            tx.map((t) => (
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
        </div>
    );
}
