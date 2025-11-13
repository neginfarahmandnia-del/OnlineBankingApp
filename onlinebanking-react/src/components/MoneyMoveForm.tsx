// src/components/MoneyMoveForm.tsx
import React, { useState } from "react";
import { deposit, withdraw } from "../lib/api";

type MoneyMoveFormProps = {
    accountId: number;
    onFinished: () => void;
    onCancel: () => void;
};

const MoneyMoveForm: React.FC<MoneyMoveFormProps> = ({
    accountId,
    onFinished,
    onCancel,
}) => {
    const [amount, setAmount] = useState<number>(0);
    const [type, setType] = useState<"deposit" | "withdraw">("deposit");
    const [description, setDescription] = useState("");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (amount <= 0) {
            setError("Betrag muss größer als 0 sein.");
            return;
        }

        setSaving(true);
        setError(null);
        try {
            if (type === "deposit") {
                await deposit({ accountId, amount, description });
            } else {
                await withdraw({ accountId, amount, description });
            }
            onFinished();
        } catch (err) {
            console.error(err);
            setError("Buchung ist fehlgeschlagen.");
        } finally {
            setSaving(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-3">
            <div>
                <label className="block text-sm mb-1">Konto-ID</label>
                <input
                    type="number"
                    value={accountId}
                    disabled
                    className="border rounded px-3 py-1 w-full bg-gray-100"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Art der Buchung</label>
                <select
                    value={type}
                    onChange={(e) =>
                        setType(e.target.value === "withdraw" ? "withdraw" : "deposit")
                    }
                    className="border rounded px-3 py-1 w-full"
                >
                    <option value="deposit">Einzahlung</option>
                    <option value="withdraw">Auszahlung</option>
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
                <label className="block text-sm mb-1">Beschreibung</label>
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
                    {saving ? "Speichern..." : "Buchen"}
                </button>
            </div>
        </form>
    );
};

export default MoneyMoveForm;
