import { Link } from "react-router-dom";
import { useAuth } from "../store/auth";
import React from "react";

export default function Home() {
    const { token } = useAuth();
    return (
        <div className="space-y-4">
            <h1 className="text-2xl font-semibold">Willkommen 👋</h1>
            {!token ? (
                <p>
                    Bitte <Link to="/login" className="text-blue-600 underline">einloggen</Link>, um dein Dashboard zu sehen.
                </p>
            ) : (
                <p>Nutze die Navigation oben: Dashboard, Transactions, Accounts.</p>
            )}
        </div>
    );
}
