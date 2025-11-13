import { Link } from "react-router-dom";
import React from "react";

export default function NotFound() {
    return (
        <div className="space-y-3">
            <h1 className="text-2xl font-semibold">404</h1>
            <p>Seite nicht gefunden.</p>
            <Link to="/" className="text-blue-600 underline">Zur Startseite</Link>
        </div>
    );
}
