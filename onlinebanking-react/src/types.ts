// Basis-Typen für Frontend-Aggregationen/Tabellen
export type Account = {
    id: string;
    name?: string | null;
    iban?: string | null;
    balance?: number | null;
};

export type Transaction = {
    id: string;
    accountId: string;
    type: "deposit" | "withdrawal" | "transfer";
    category?: string | null;
    amount: number;          // positiv
    createdAt: string;       // ISO-String
    description?: string | null;
};
