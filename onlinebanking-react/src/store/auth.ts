// src/store/auth.ts
import { create } from "zustand";
import { login as apiLogin, getMe } from "../lib/api";

type Me = { email?: string; roles?: string[] };

type AuthState = {
    token: string | null;
    email: string | null;
    roles: string[] | null;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    me: () => Promise<Me | null>;
};

export const useAuth = create<AuthState>((set, get) => ({
    token: localStorage.getItem("auth_token"),
    email: localStorage.getItem("auth_email"),
    roles: null,

    async login(email, password) {
        const res = await apiLogin(email, password);
        // apiLogin setzt das Token bereits in localStorage, wir spiegeln es im Zustand
        localStorage.setItem("auth_token", res.token);
        localStorage.setItem("auth_email", res.email ?? email);

        set({
            token: res.token,
            email: res.email ?? email,
            roles: res.roles ?? null,
        });
    },

    logout() {
        localStorage.removeItem("auth_token");
        localStorage.removeItem("auth_email");
        set({ token: null, email: null, roles: null });
    },

    async me() {
        if (!get().token) return null;
        try {
            const data = await getMe();
            if (data?.roles) set({ roles: data.roles });
            return data;
        } catch {
            return null;
        }
    },
}));
