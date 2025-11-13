import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "../store/auth";
import { useNavigate, useLocation } from "react-router-dom";
import React from "react";

export default function Login() {
    const [email, setEmail] = useState("admin@example.com");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const login = useAuth((s) => s.login);
    const navigate = useNavigate();
    const loc = useLocation() as any;

    async function onSubmit(e: FormEvent) {
        e.preventDefault();
        setError(null);
        try {
            await login(email, password);
            const to = loc.state?.from?.pathname ?? "/dashboard";
            navigate(to, { replace: true });
        } catch (e: any) {
            setError(e?.response?.data ?? "Login fehlgeschlagen");
        }
    }

    return (
        <div className="max-w-sm mx-auto">
            <h1 className="text-xl font-semibold mb-4">Login</h1>
            <form onSubmit={onSubmit} className="space-y-3">
                <div>
                    <label className="block text-sm">Email</label>
                    <input className="w-full border rounded px-3 py-2" value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
                </div>
                <div>
                    <label className="block text-sm">Passwort</label>
                    <input className="w-full border rounded px-3 py-2" value={password} onChange={(e) => setPassword(e.target.value)} type="password" required />
                </div>
                <button className="w-full bg-blue-600 text-white rounded px-3 py-2" type="submit">Anmelden</button>
            </form>
            {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
        </div>
    );
}
