// src/pages/Reports.tsx
import React, { useEffect, useMemo, useState } from "react";
import {
    getAccounts,
    getMonthlyChartData,
    exportExcelForAccount,
    exportPdfForAccount,
    getTransactions,
    type Account,
    type MonthlyPoint,
    type Transaction,
} from "../lib/api";

const currentYear = new Date().getFullYear();
const currentMonth = new Date().getMonth() + 1;

const YEARS = Array.from({ length: 6 }).map((_, i) => currentYear - i);
const MONTHS = [
    { value: 1, label: "Januar" },
    { value: 2, label: "Februar" },
    { value: 3, label: "März" },
    { value: 4, label: "April" },
    { value: 5, label: "Mai" },
    { value: 6, label: "Juni" },
    { value: 7, label: "Juli" },
    { value: 8, label: "August" },
    { value: 9, label: "September" },
    { value: 10, label: "Oktober" },
    { value: 11, label: "November" },
    { value: 12, label: "Dezember" },
];

function pad2(n: number) {
    return n.toString().padStart(2, "0");
}

function getLastDayOfMonth(year: number, month: number) {
    return new Date(year, month, 0).getDate(); // day 0 = letzter Tag Vormonat
}

const ReportsPage: React.FC = () => {
    const [accounts, setAccounts] = useState<Account[]>([]);
    const [selectedAccountId, setSelectedAccountId] = useState<number | null>(
        null
    );
    const [year, setYear] = useState<number>(currentYear);
    const [month, setMonth] = useState<number>(currentMonth);

    const [chartData, setChartData] = useState<MonthlyPoint[]>([]);
    const [transactions, setTransactions] = useState<Transaction[]>([]);

    const [loadingAccounts, setLoadingAccounts] = useState(false);
    const [loadingChart, setLoadingChart] = useState(false);
    const [loadingTx, setLoadingTx] = useState(false);

    const [error, setError] = useState<string | null>(null);

    // Konten laden
    useEffect(() => {
        let cancelled = false;
        setLoadingAccounts(true);
        setError(null);

        getAccounts()
            .then((res) => {
                if (cancelled) return;
                setAccounts(res ?? []);
                if (!selectedAccountId && res && res.length > 0) {
                    setSelectedAccountId(Number(res[0].id));
                }
            })
            .catch((err) => {
                if (!cancelled) {
                    console.error(err);
                    setError("Konten konnten nicht geladen werden.");
                }
            })
            .finally(() => {
                if (!cancelled) setLoadingAccounts(false);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    // Chart-Daten laden
    useEffect(() => {
        if (!selectedAccountId) {
            setChartData([]);
            return;
        }
        let cancelled = false;
        setLoadingChart(true);
        setError(null);

        getMonthlyChartData({
            bankAccountId: selectedAccountId,
            year,
            month,
        })
            .then((res) => {
                if (!cancelled) {
                    setChartData(res ?? []);
                }
            })
            .catch((err) => {
                if (!cancelled) {
                    console.error(err);
                    setError("Berichtsdaten konnten nicht geladen werden.");
                }
            })
            .finally(() => {
                if (!cancelled) setLoadingChart(false);
            });

        return () => {
            cancelled = true;
        };
    }, [selectedAccountId, year, month]);

    // Transaktionen für den gewählten Monat laden
    useEffect(() => {
        if (!selectedAccountId) {
            setTransactions([]);
            return;
        }

        const from = `${year}-${pad2(month)}-01`;
        const lastDay = getLastDayOfMonth(year, month);
        const to = `${year}-${pad2(month)}-${pad2(lastDay)}`;

        let cancelled = false;
        setLoadingTx(true);
        setError(null);

        getTransactions({
            accountId: selectedAccountId,
            dateFrom: from,
            dateTo: to,
            sort: "date_desc",
        })
            .then((res) => {
                if (cancelled) return;
                // getTransactions kann entweder ein Paged-Objekt oder ein Array liefern
                const items = Array.isArray(res) ? res : res.items ?? [];
                setTransactions(items);
            })
            .catch((err) => {
                if (!cancelled) {
                    console.error(err);
                    setError("Transaktionen konnten nicht geladen werden.");
                }
            })
            .finally(() => {
                if (!cancelled) setLoadingTx(false);
            });

        return () => {
            cancelled = true;
        };
    }, [selectedAccountId, year, month]);

    const selectedAccount = useMemo(
        () =>
            accounts.find((a) => Number(a.id) === selectedAccountId) ?? null,
        [accounts, selectedAccountId]
    );

    const chartTotal = useMemo(
        () => chartData.reduce((sum, p) => sum + p.amount, 0),
        [chartData]
    );

    const maxAbsValue = useMemo(
        () =>
            chartData.length > 0
                ? Math.max(...chartData.map((d) => Math.abs(d.amount))) || 1
                : 1,
        [chartData]
    );

    const handleExportExcel = async () => {
        if (!selectedAccountId) return;
        try {
            await exportExcelForAccount(selectedAccountId);
        } catch (err) {
            console.error(err);
            alert("Excel-Export fehlgeschlagen.");
        }
    };

    const handleExportPdf = async () => {
        if (!selectedAccountId) return;
        try {
            await exportPdfForAccount(selectedAccountId);
        } catch (err) {
            console.error(err);
            alert("PDF-Export fehlgeschlagen.");
        }
    };

    const monthLabel =
        MONTHS.find((m) => m.value === month)?.label ?? `Monat ${month}`;

    return (
        <div className="p-4">
            <h1 className="text-2xl font-semibold mb-4">Berichte</h1>

            {/* Filterbereich */}
            <div className="flex flex-wrap gap-3 mb-4 items-end">
                <div>
                    <label className="block text-sm mb-1">Konto</label>
                    <select
                        className="border rounded px-3 py-1 min-w-[220px]"
                        value={selectedAccountId ?? ""}
                        onChange={(e) =>
                            setSelectedAccountId(
                                e.target.value ? Number(e.target.value) : null
                            )
                        }
                    >
                        {accounts.map((a) => (
                            <option key={a.id} value={a.id}>
                                {a.name ?? a.iban ?? `Konto ${a.id}`}
                            </option>
                        ))}
                    </select>
                </div>

                <div>
                    <label className="block text-sm mb-1">Jahr</label>
                    <select
                        className="border rounded px-3 py-1"
                        value={year}
                        onChange={(e) => setYear(Number(e.target.value))}
                    >
                        {YEARS.map((y) => (
                            <option key={y} value={y}>
                                {y}
                            </option>
                        ))}
                    </select>
                </div>

                <div>
                    <label className="block text-sm mb-1">Monat</label>
                    <select
                        className="border rounded px-3 py-1"
                        value={month}
                        onChange={(e) => setMonth(Number(e.target.value))}
                    >
                        {MONTHS.map((m) => (
                            <option key={m.value} value={m.value}>
                                {m.label}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="ml-auto flex gap-2">
                    <button
                        onClick={handleExportExcel}
                        disabled={!selectedAccountId}
                        className="border rounded px-3 py-1"
                    >
                        Excel-Export
                    </button>
                    <button
                        onClick={handleExportPdf}
                        disabled={!selectedAccountId}
                        className="border rounded px-3 py-1"
                    >
                        PDF-Export
                    </button>
                </div>
            </div>

            {loadingAccounts && <p>Konten werden geladen...</p>}
            {(loadingChart || loadingTx) && (
                <p>Berichtsdaten werden geladen...</p>
            )}
            {error && (
                <p className="text-red-600 mb-2">
                    {error}
                </p>
            )}

            {selectedAccount && !error && (
                <>
                    {/* Zusammenfassung */}
                    <div className="mb-4">
                        <h2 className="font-semibold text-lg mb-1">
                            {monthLabel} {year} –{" "}
                            {selectedAccount.name ?? selectedAccount.iban}
                        </h2>
                        <p className="text-sm text-gray-600">
                            Gesamtsaldo der Bewegungen im gewählten Zeitraum:{" "}
                            <span className="font-semibold">
                                {chartTotal.toFixed(2)} €
                            </span>
                        </p>
                    </div>

                    {/* Pseudo-Chart */}
                    <div className="mb-4 border rounded p-3">
                        <h3 className="font-semibold text-sm mb-2">
                            Tageswerte (Netto-Ein-/Ausgänge)
                        </h3>
                        {chartData.length === 0 && (
                            <p className="text-gray-500 text-sm">
                                Keine Daten für diesen Zeitraum vorhanden.
                            </p>
                        )}

                        {chartData.length > 0 && (
                            <div className="space-y-1">
                                {chartData.map((point, idx) => {
                                    const widthPct = Math.max(
                                        5,
                                        (Math.abs(point.amount) / maxAbsValue) * 100
                                    );
                                    const isPositive = point.amount >= 0;
                                    return (
                                        <div key={idx} className="flex items-center gap-2">
                                            <div className="w-12 text-xs text-right">
                                                {point.month}
                                            </div>
                                            <div className="flex-1">
                                                <div
                                                    className={
                                                        "h-3 rounded " +
                                                        (isPositive ? "bg-green-400" : "bg-red-400")
                                                    }
                                                    style={{ width: `${widthPct}%` }}
                                                ></div>
                                            </div>
                                            <div className="w-20 text-xs text-right">
                                                {point.amount.toFixed(2)} €
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {/* Transaktionsliste */}
                    <div className="border rounded p-3">
                        <h3 className="font-semibold text-sm mb-2">
                            Transaktionen ({monthLabel} {year})
                        </h3>

                        {transactions.length === 0 && (
                            <p className="text-gray-500 text-sm">
                                Keine Transaktionen im gewählten Zeitraum.
                            </p>
                        )}

                        {transactions.length > 0 && (
                            <div className="overflow-x-auto">
                                <table className="min-w-full border text-sm">
                                    <thead className="bg-gray-100">
                                        <tr>
                                            <th className="border px-2 py-1 text-left">
                                                Datum
                                            </th>
                                            <th className="border px-2 py-1 text-left">
                                                Typ
                                            </th>
                                            <th className="border px-2 py-1 text-left">
                                                Kategorie
                                            </th>
                                            <th className="border px-2 py-1 text-left">
                                                Beschreibung
                                            </th>
                                            <th className="border px-2 py-1 text-right">
                                                Betrag
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {transactions.map((tx) => {
                                            const dateStr =
                                                tx.date ??
                                                tx.createdAt ??
                                                "";
                                            const dateDisplay = dateStr
                                                ? new Date(dateStr).toLocaleDateString()
                                                : "";
                                            const amount = Number(tx.amount ?? 0);
                                            const isPositive = amount >= 0;

                                            return (
                                                <tr key={tx.id}>
                                                    <td className="border px-2 py-1">
                                                        {dateDisplay}
                                                    </td>
                                                    <td className="border px-2 py-1">
                                                        {tx.type}
                                                    </td>
                                                    <td className="border px-2 py-1">
                                                        {tx.category}
                                                    </td>
                                                    <td className="border px-2 py-1">
                                                        {tx.description}
                                                    </td>
                                                    <td
                                                        className={
                                                            "border px-2 py-1 text-right " +
                                                            (isPositive
                                                                ? "text-green-600"
                                                                : "text-red-600")
                                                        }
                                                    >
                                                        {amount.toFixed(2)} €
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                </>
            )}
        </div>
    );
};

export default ReportsPage;
