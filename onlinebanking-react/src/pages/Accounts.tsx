// src/pages/Accounts.tsx
import React, { useEffect, useMemo, useState } from "react";
import { getAccounts, type Account } from "../lib/api";

import AccountCreateForm from "../components/AccountCreateForm";
import MoneyMoveForm from "../components/MoneyMoveForm";
import TransferForm from "../components/TransferForm";

type Mode = "none" | "create" | "money-move" | "transfer";

const AccountsPage: React.FC = () => {
    const [accounts, setAccounts] = useState<Account[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [search, setSearch] = useState("");
    const [selectedAccountId, setSelectedAccountId] = useState<number | null>(
        null
    );
    const [mode, setMode] = useState<Mode>("none");

    // Konten einmal laden
    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        setError(null);

        getAccounts()
            .then((data) => {
                if (cancelled) return;
                const list = data ?? [];
                setAccounts(list);
            })
            .catch((err) => {
                if (!cancelled) {
                    console.error(err);
                    setError("Konten konnten nicht geladen werden.");
                }
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    // Wenn Konten geladen wurden und noch kein Konto ausgewählt ist:
    // automatisch das erste Konto auswählen
    useEffect(() => {
        if (accounts.length > 0 && selectedAccountId === null) {
            setSelectedAccountId(Number(accounts[0].id));
        }
    }, [accounts, selectedAccountId]);

    const filteredAccounts = useMemo(() => {
        if (!search.trim()) return accounts;
        const q = search.toLowerCase();
        return accounts.filter((a) => {
            const values = [
                a.name ?? "",
                a.iban ?? "",
                a.accountHolder ?? "",
                a.kontotyp ?? "",
                a.abteilung ?? "",
            ];
            return values.some((v) => v.toLowerCase().includes(q));
        });
    }, [accounts, search]);

    const selectedAccount = useMemo(
        () => accounts.find((a) => Number(a.id) === selectedAccountId) ?? null,
        [accounts, selectedAccountId]
    );

    const handleRowClick = (id: Account["id"]) => {
        setSelectedAccountId(Number(id));
    };

    const closeModals = () => setMode("none");

    const handleAccountCreated = (newAccount: Account) => {
        setAccounts((prev) => [...prev, newAccount]);
        setMode("none");
    };

    const reloadAccounts = () => {
        setLoading(true);
        getAccounts()
            .then((data) => setAccounts(data ?? []))
            .finally(() => setLoading(false));
    };

    const handleMoneyMoveFinished = () => {
        setMode("none");
        reloadAccounts();
    };

    const handleTransferFinished = () => {
        setMode("none");
        reloadAccounts();
    };

    return (
        <div className="p-4">
            <h1 className="text-2xl font-semibold mb-4">Kontenübersicht</h1>

            <div className="mb-4 flex flex-wrap gap-2 items-center">
                <input
                    type="text"
                    placeholder="Suche nach Konto / IBAN / Inhaber..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="border rounded px-3 py-1 min-w-[250px]"
                />

                <button
                    onClick={() => setMode("create")}
                    className="border rounded px-3 py-1"
                >
                    Neues Konto
                </button>

                <button
                    onClick={() => setMode("money-move")}
                    className="border rounded px-3 py-1"
                    disabled={!selectedAccount}
                    title={
                        selectedAccount
                            ? "Ein- oder Auszahlung für das ausgewählte Konto buchen"
                            : "Bitte zuerst ein Konto in der Tabelle auswählen"
                    }
                >
                    Ein-/Auszahlung
                </button>

                <button
                    onClick={() => setMode("transfer")}
                    className="border rounded px-3 py-1"
                    disabled={!selectedAccount}
                    title={
                        selectedAccount
                            ? "Überweisung vom ausgewählten Konto ausführen"
                            : "Bitte zuerst ein Konto in der Tabelle auswählen"
                    }
                >
                    Überweisung
                </button>
            </div>

            {loading && <p>Konten werden geladen...</p>}
            {error && (
                <p className="text-red-600 mb-2">
                    {error}
                </p>
            )}

            {!loading && !error && (
                <>
                    {accounts.length > 0 && (
                        <p className="text-xs text-gray-500 mb-1">
                            Tipp: Du kannst ein Konto durch Klick auf die Zeile auswählen.
                        </p>
                    )}
                    <div className="overflow-x-auto">
                        <table className="min-w-full border text-sm">
                            <thead className="bg-gray-100">
                                <tr>
                                    <th className="border px-2 py-1 text-left">#</th>
                                    <th className="border px-2 py-1 text-left">Name</th>
                                    <th className="border px-2 py-1 text-left">IBAN</th>
                                    <th className="border px-2 py-1 text-left">Inhaber</th>
                                    <th className="border px-2 py-1 text-right">Saldo</th>
                                    <th className="border px-2 py-1 text-left">Kontotyp</th>
                                    <th className="border px-2 py-1 text-left">Abteilung</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredAccounts.map((acc) => {
                                    const isSelected = Number(acc.id) === selectedAccountId;
                                    return (
                                        <tr
                                            key={acc.id}
                                            onClick={() => handleRowClick(acc.id)}
                                            className={
                                                "cursor-pointer hover:bg-gray-50 " +
                                                (isSelected ? "bg-blue-50" : "")
                                            }
                                        >
                                            <td className="border px-2 py-1">{acc.id}</td>
                                            <td className="border px-2 py-1">{acc.name}</td>
                                            <td className="border px-2 py-1">{acc.iban}</td>
                                            <td className="border px-2 py-1">
                                                {acc.accountHolder}
                                            </td>
                                            <td className="border px-2 py-1 text-right">
                                                {acc.balance?.toFixed(2)} €
                                            </td>
                                            <td className="border px-2 py-1">{acc.kontotyp}</td>
                                            <td className="border px-2 py-1">{acc.abteilung}</td>
                                        </tr>
                                    );
                                })}

                                {filteredAccounts.length === 0 && (
                                    <tr>
                                        <td
                                            colSpan={7}
                                            className="border px-2 py-3 text-center text-gray-500"
                                        >
                                            Keine Konten gefunden.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </>
            )}

            {/* Modale Bereiche */}
            {mode === "create" && (
                <div className="fixed inset-0 bg-black/30 flex items-center justify-center">
                    <div className="bg-white p-4 rounded shadow max-w-lg w-full">
                        <div className="flex justify-between items-center mb-2">
                            <h2 className="font-semibold">Neues Konto anlegen</h2>
                            <button onClick={closeModals}>✕</button>
                        </div>
                        <AccountCreateForm
                            onCreated={handleAccountCreated}
                            onCancel={closeModals}
                        />
                    </div>
                </div>
            )}

            {mode === "money-move" && selectedAccount && (
                <div className="fixed inset-0 bg-black/30 flex items-center justify-center">
                    <div className="bg-white p-4 rounded shadow max-w-lg w-full">
                        <div className="flex justify-between items-center mb-2">
                            <h2 className="font-semibold">
                                Ein-/Auszahlung für Konto {selectedAccount.name}
                            </h2>
                            <button onClick={closeModals}>✕</button>
                        </div>
                        <MoneyMoveForm
                            accountId={Number(selectedAccount.id)}
                            onFinished={handleMoneyMoveFinished}
                            onCancel={closeModals}
                        />
                    </div>
                </div>
            )}

            {mode === "transfer" && selectedAccount && (
                <div className="fixed inset-0 bg-black/30 flex items-center justify-center">
                    <div className="bg-white p-4 rounded shadow max-w-lg w-full">
                        <div className="flex justify-between items-center mb-2">
                            <h2 className="font-semibold">
                                Überweisung von Konto {selectedAccount.name}
                            </h2>
                            <button onClick={closeModals}>✕</button>
                        </div>
                        <TransferForm
                            fromAccountId={Number(selectedAccount.id)}
                            accounts={accounts}
                            onFinished={handleTransferFinished}
                            onCancel={closeModals}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};

export default AccountsPage;
