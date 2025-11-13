import { useMemo, useState } from "react";
import React from "react";

export type Column<T> = {
    key: keyof T | string;
    header: string;
    render?: (row: T) => React.ReactNode;
    sortAccessor?: (row: T) => string | number;
};

type Props<T> = {
    data: T[];
    columns: Column<T>[];
    filterPlaceholder?: string;
    pageSizeOptions?: number[];
};

export default function DataTable<T extends Record<string, any>>({
    data,
    columns,
    filterPlaceholder = "Suchen…",
    pageSizeOptions = [10, 20, 50],
}: Props<T>) {
    const [query, setQuery] = useState("");
    const [sortKey, setSortKey] = useState<string | null>(null);
    const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
    const [pageSize, setPageSize] = useState(pageSizeOptions[0]);
    const [page, setPage] = useState(0);

    const filtered = useMemo(() => {
        if (!query.trim()) return data;
        const q = query.toLowerCase();
        return data.filter(row =>
            Object.values(row).some(v => String(v ?? "").toLowerCase().includes(q))
        );
    }, [data, query]);

    const sorted = useMemo(() => {
        if (!sortKey) return filtered;
        const factor = sortDir === "asc" ? 1 : -1;
        return [...filtered].sort((a, b) => {
            const av = a[sortKey as keyof T];
            const bv = b[sortKey as keyof T];
            const an = typeof av === "number" ? av : Number(av);
            const bn = typeof bv === "number" ? bv : Number(bv);
            if (!Number.isNaN(an) && !Number.isNaN(bn)) return (an - bn) * factor;
            return String(av ?? "").localeCompare(String(bv ?? "")) * factor;
        });
    }, [filtered, sortKey, sortDir]);

    const totalPages = Math.max(1, Math.ceil(sorted.length / pageSize));
    const pageData = useMemo(() => {
        const start = page * pageSize;
        return sorted.slice(start, start + pageSize);
    }, [sorted, page, pageSize]);

    const toggleSort = (key: string) => {
        if (sortKey !== key) { setSortKey(key); setSortDir("asc"); return; }
        setSortDir(d => (d === "asc" ? "desc" : "asc"));
    };

    return (
        <div className="space-y-3">
            <div className="flex items-center justify-between gap-2">
                <input
                    className="w-full max-w-xs rounded-md border px-3 py-2 text-sm bg-transparent"
                    placeholder={filterPlaceholder}
                    value={query}
                    onChange={(e) => { setQuery(e.target.value); setPage(0); }}
                />
                <select
                    className="rounded-md border px-2 py-1 text-sm"
                    value={pageSize}
                    onChange={(e) => { setPageSize(Number(e.target.value)); setPage(0); }}
                >
                    {pageSizeOptions.map(n => <option key={n} value={n}>{n}/Seite</option>)}
                </select>
            </div>

            <div className="overflow-x-auto rounded-lg border">
                <table className="min-w-full text-sm">
                    <thead className="bg-black/5 dark:bg-white/10">
                        <tr>
                            {columns.map(c => (
                                <th
                                    key={String(c.key)}
                                    className="px-3 py-2 text-left font-medium cursor-pointer select-none"
                                    onClick={() => toggleSort(String(c.key))}
                                    title="Sortieren"
                                >
                                    {c.header}
                                    {sortKey === c.key && (sortDir === "asc" ? " ↑" : " ↓")}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {pageData.map((row, idx) => (
                            <tr key={idx} className="border-t hover:bg-black/5 dark:hover:bg-white/5">
                                {columns.map(c => (
                                    <td key={String(c.key)} className="px-3 py-2">
                                        {c.render ? c.render(row) : String(row[c.key as keyof T] ?? "")}
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className="flex items-center justify-between">
                <span className="text-xs opacity-70">
                    Seite {page + 1} / {totalPages}
                </span>
                <div className="flex gap-2">
                    <button
                        className="rounded-md border px-3 py-1 text-sm disabled:opacity-50"
                        onClick={() => setPage(p => Math.max(0, p - 1))}
                        disabled={page === 0}
                    >
                        Zurück
                    </button>
                    <button
                        className="rounded-md border px-3 py-1 text-sm disabled:opacity-50"
                        onClick={() => setPage(p => Math.min(totalPages - 1, p + 1))}
                        disabled={page >= totalPages - 1}
                    >
                        Weiter
                    </button>
                </div>
            </div>
        </div>
    );
}
