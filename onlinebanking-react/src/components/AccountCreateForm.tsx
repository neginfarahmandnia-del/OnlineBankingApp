// src/components/AccountCreateForm.tsx
import React, { useState } from "react";
import axios from "axios";
import type { Account } from "../lib/api";
import { createAccount as apiCreateAccount } from "../lib/api";

type Props = {
    onCreated: (account: Account) => void;
    onCancel: () => void;
};

const AccountCreateForm: React.FC<Props> = ({ onCreated, onCancel }) => {
    const [name, setName] = useState("");
    const [iban, setIban] = useState("");
    const [holder, setHolder] = useState("");
    const [warnLimit, setWarnLimit] = useState(0);
    const [kontotyp, setKontotyp] = useState("");
    const [abteilung, setAbteilung] = useState("");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true);
        setError(null);

        try {
            // Feldnamen genau wie im Backend: Name, IBAN, AccountHolder, WarnLimit, Kontotyp, Abteilung
            const body = {
                Name: name,
                IBAN: iban,
                AccountHolder: holder,
                WarnLimit: warnLimit,
                Kontotyp: kontotyp,
                Abteilung: abteilung,
            };

            const created = await apiCreateAccount(body);
            onCreated(created as Account);
        } catch (err) {
            console.error(err);

            if (axios.isAxiosError(err) && err.response) {
                const status = err.response.status;
                const data: any = err.response.data;

                // Versuche, konkrete Validierungsfehler zu zeigen
                if (status === 400 && data?.errors) {
                    const errors = data.errors as Record<string, string[]>;
                    if (errors.Name?.length) {
                        setError(errors.Name[0]);
                    } else if (errors.AccountHolder?.length) {
                        setError(errors.AccountHolder[0]);
                    } else {
                        setError("Die Kontodaten sind ungültig. Bitte Eingaben prüfen.");
                    }
                } else if (status === 409) {
                    setError("Ein Konto mit dieser IBAN existiert bereits.");
                } else if (status === 401) {
                    setError("Deine Sitzung ist abgelaufen. Bitte melde dich neu an.");
                } else {
                    setError("Konto konnte nicht angelegt werden.");
                }
            } else {
                setError("Konto konnte nicht angelegt werden.");
            }
        } finally {
            setSaving(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-3">
            <div>
                <label className="block text-sm mb-1">Kontoname</label>
                <input
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">IBAN</label>
                <input
                    value={iban}
                    onChange={(e) => setIban(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Kontoinhaber</label>
                <input
                    value={holder}
                    onChange={(e) => setHolder(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Warnlimit</label>
                <input
                    type="number"
                    value={warnLimit}
                    onChange={(e) => setWarnLimit(Number(e.target.value))}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Kontotyp</label>
                <input
                    value={kontotyp}
                    onChange={(e) => setKontotyp(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            <div>
                <label className="block text-sm mb-1">Abteilung</label>
                <input
                    value={abteilung}
                    onChange={(e) => setAbteilung(e.target.value)}
                    className="border rounded px-3 py-1 w-full"
                />
            </div>

            {error && (
                <p className="text-red-600 text-sm">
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
                    {saving ? "Speichern..." : "Speichern"}
                </button>
            </div>
        </form>
    );
};

export default AccountCreateForm;
