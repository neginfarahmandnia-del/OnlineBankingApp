// src/lib/api.ts
import axios, {
    AxiosHeaders,
    type InternalAxiosRequestConfig,
} from "axios";

// Basis-URL aus .env.local (z. B. VITE_API_URL=https://localhost:7202)
const API_BASE: string =
    ((import.meta as any)?.env?.VITE_API_URL as string | undefined)?.replace(/\/+$/, "") ??
    "";

export const api = axios.create({ baseURL: API_BASE });

export function setAuthToken(token: string | null) {
    if (token) {
        api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
        localStorage.setItem("auth_token", token);
    } else {
        delete api.defaults.headers.common["Authorization"];
        localStorage.removeItem("auth_token");
    }
}

// Interceptor: hängt das Token bei jedem Request automatisch an
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem("auth_token");
    if (!token) return config;

    if (!config.headers) config.headers = new AxiosHeaders();
    if (!(config.headers instanceof AxiosHeaders)) {
        config.headers = new AxiosHeaders(config.headers);
    }
    (config.headers as AxiosHeaders).set("Authorization", `Bearer ${token}`);
    return config;
});

// ===================== Auth =====================

export type LoginResponse = {
    token: string;
    email?: string;
    roles?: string[];
};

export async function login(email: string, password: string) {
    const { data } = await api.post<LoginResponse>("/api/Auth/login", {
        email,
        password,
    });
    if (data?.token) setAuthToken(data.token);
    return data;
}

export type MeDto = {
    userId?: string;
    email?: string;
    roles?: string[];
};

export async function getMe() {
    const { data } = await api.get<MeDto>("/api/Auth/me");
    return data;
}

// ===================== Accounts / Bankkonten =====================

export type Account = {
    id: string | number;
    name?: string;
    iban?: string;
    accountHolder?: string;
    balance?: number;
    warnLimit?: number;
    kontotyp?: string;
    abteilung?: string;
};

export async function getAccounts() {
    // entspricht AccountsController.GetBankAccounts() => GET api/Accounts/bankaccounts
    const { data } = await api.get<Account[]>("/api/Accounts/bankaccounts");
    return data;
}

// src/lib/api.ts – Ausschnitt

export type CreateAccountDto = {
    Name: string;
    IBAN: string;
    AccountHolder: string;
    WarnLimit: number;
    Kontotyp: string;
    Abteilung: string;
};

export async function createAccount(body: CreateAccountDto) {
    const { data } = await api.post("/api/Accounts", body);
    return data;
}


// ===================== Transaktionen =====================

export type Transaction = {
    id: string | number;
    bankAccountId?: number;
    accountId?: string | number;
    amount: number;
    type: string;
    description?: string;
    category?: string;
    date?: string;
    createdAt?: string;
};

export type TransactionQuery = {
    search?: string;
    dateFrom?: string;
    dateTo?: string;
    minAmount?: number;
    maxAmount?: number;
    page?: number;
    pageSize?: number;
    sort?: "date_desc" | "date_asc" | "amount_desc" | "amount_asc";
    accountId?: string | number;
    type?: string;
};

export type Paged<T> = {
    items: T[];
    total: number;
    page: number;
    pageSize: number;
};

export async function getTransactions(
    q: TransactionQuery = {}
): Promise<Paged<Transaction> | Transaction[]> {
    // Backend liefert aktuell eine Liste; Query-Parameter können (noch) ignoriert werden.
    const { data } = await api.get<Paged<Transaction> | Transaction[]>(
        "/api/Transactions",
        { params: q }
    );
    return data;
}

// Transaktion direkt anlegen (falls genutzt)
export type CreateTransactionDto = {
    bankAccountId: number;
    amount: number;
    type: "Deposit" | "Withdraw" | "Transfer" | string;
    category?: string;
    description?: string;
    date?: string; // ISO
};

export async function createTransaction(body: CreateTransactionDto) {
    const { data } = await api.post("/api/Transactions", body);
    return data;
}

// ===================== Ein- & Auszahlung =====================

export type MoneyMoveDto = {
    accountId: number;
    amount: number;
    description?: string;
};

export async function deposit(body: MoneyMoveDto) {
    const { data } = await api.post("/api/Accounts/deposit", body);
    return data;
}

export async function withdraw(body: MoneyMoveDto) {
    const { data } = await api.post("/api/Accounts/withdraw", body);
    return data;
}

// ===================== Überweisungen =====================

export type TransferRequest = {
    fromAccountId: number;
    toAccountId: number;
    amount: number;
    description?: string;
};

export async function transfer(req: TransferRequest) {
    // entspricht TransferController => POST api/Transfer
    const { data } = await api.post("/api/Transfer", req);
    return data;
}

// ===================== Exporte & Charts =====================

export type MonthlyPoint = { month: string; amount: number };

/**
 * Holt Monats-Daten für ein Konto.
 * Wenn kein `bankAccountId` übergeben wird, kommt einfach ein leeres Array zurück.
 */
export async function getMonthlyChartData(args?: {
    bankAccountId?: number;
    year?: number;
    month?: number;
}): Promise<MonthlyPoint[]> {
    if (!args?.bankAccountId) return [];

    const now = new Date();
    const year = args.year ?? now.getFullYear();
    const month = args.month ?? now.getMonth() + 1;

    const { data } = await api.get<any[]>(
        "/api/TransactionExport/monthly-chart-data",
        {
            params: {
                bankAccountId: args.bankAccountId,
                year,
                month,
            },
        }
    );

    if (!Array.isArray(data)) return [];

    // Backend: anonyme Objekte mit Day/Einnahmen/Ausgaben
    return data.map((d) => {
        const day = d.day ?? d.Day;
        const einnahmen = Number(d.einnahmen ?? d.Einnahmen ?? 0);
        const ausgaben = Number(d.ausgaben ?? d.Ausgaben ?? 0);
        return {
            month: `${day}.`,
            // Ausgaben sind i. d. R. negativ, daher Summe = Netto
            amount: einnahmen + ausgaben,
        };
    });
}

export async function downloadExport(path: string, fileName: string) {
    const token = localStorage.getItem("auth_token");
    const url = (api.defaults.baseURL ?? "") + path;

    const res = await fetch(url, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
    });

    if (!res.ok) throw new Error(`Download fehlgeschlagen (${res.status})`);

    const blob = await res.blob();
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(a.href);
}

export async function exportExcelForAccount(accountId: number) {
    const id = encodeURIComponent(String(accountId));
    return downloadExport(
        `/api/TransactionExport/excel?bankAccountId=${id}`,
        `Konto_${id}.xlsx`
    );
}

export async function exportPdfForAccount(accountId: number) {
    const id = encodeURIComponent(String(accountId));
    return downloadExport(
        `/api/TransactionExport/pdf?bankAccountId=${id}`,
        `Konto_${id}.pdf`
    );
}

export default api;
