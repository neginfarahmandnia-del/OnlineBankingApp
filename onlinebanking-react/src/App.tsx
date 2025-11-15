import { Outlet } from "react-router-dom";
import NavBar from "./components/NavBar";
import React from 'react';

export default function App() {
    return (
        <div className="min-h-screen bg-white text-zinc-900 dark:bg-zinc-950 dark:text-zinc-100">
            <NavBar />
            <main className="mx-auto max-w-6xl px-4 py-6">
                <Outlet />
            </main>
        </div>
    );
}
