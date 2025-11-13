// src/components/TransferForm.tsx
import React, { useState } from "react";
import { transfer, type Account } from "../lib/api";

type TransferFormProps = {
    fromAccountId: number;
    accounts: Account[];
    onFinished: () => void;
    onCancel: () => void;
};

const TransferForm: React.FC<TransferFormProps> = ({
    fromAccountId,
    accounts,
    onFinished,
    onCancel,
}) => {
    const [toAccountId, setToAccountId] = useState<number | null>(null);
    const [amount, setAmount] = useState<number>(0);
    const [description, setDescription] = useState("");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!toAccountId) {
            setError("Bitte ein Zielkonto auswählen.");
            return;
        }
        if (amount <= 0) {
            setError("Betrag muss größer als 0 sein.");
            return;
        }

        setSaving(true);
        setError(null);
        try {
            await transfer({
                fromAccountId,
                toAccountId,
                amount,
                description,
            });
            onFinished();
        } catch (err) {
            console.error(err);
            setError("Überweisung ist fehlgeschlagen.");
        } finally {
            setSaving(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-3">
            <div>
                <label className="block text-sm mb-1">Von Konto</label>
                <input
                    type="number"
                    value={fromAccountId}
                    disabled
                    className="border rounded px-3 py-1 w-full bg-gray-100"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Auf Konto</label>
                <select
                    className="border rounded px-3 py-1 w-full"
                    value={toAccountId ?? ""}
                    onChange={(e) =>
                        setToAccountId(e.target.value ? Number(e.target.value) : null)
                    }
                >
                    <option value="">-- Konto wählen --</option>
                    {accounts
                        .filter((a) => Number(a.id) !== fromAccountId)
                        .map((a) => (
                            <option key={a.id} value={a.id}>
                                {a.name ?? a.iban ?? `Konto ${a.id}`}
                            </option>
                        ))}
                </select>
            </div>

            <div>
                <label className="block text-sm mb-1">Betrag</label>
                <input
                    type="number"
                    step="0.01"
                    value={amount}
                    onChange={(e) => setAmount(Number(e.target.value))}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Verwendungszweck</label>
                <input
                    type="text"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            {error && (
                <p className="text-sm text-red-600">
                    {error}
                </p>
            )}

            <div className="flex justify-end gap-2">
                <button
                    type="button"
                    onClick={onCancel}
                    className="border rounded px-3 py-1"
                    disabled={saving}
                >
                    Abbrechen
                </button>
                <button
                    type="submit"
                    className="border rounded px-3 py-1"
                    disabled={saving}
                >
                    {saving ? "Überweise..." : "Überweisen"}
                </button>
            </div>
        </form>
    );
};

export default TransferForm;
