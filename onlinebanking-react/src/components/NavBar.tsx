import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../store/auth";
import ThemeToggle from "./ThemeToggle";
import React from "react";

const linkCls = ({ isActive }: { isActive: boolean }) =>
    "px-3 py-2 rounded text-sm " + (isActive ? "bg-zinc-200 dark:bg-zinc-800" : "hover:bg-zinc-100 dark:hover:bg-zinc-900");

export default function NavBar() {
    const { token, email, logout } = useAuth();
    return (
        <header className="border-b border-zinc-200 dark:border-zinc-800">
            <div className="mx-auto max-w-6xl px-4 h-14 flex items-center gap-3 justify-between">
                <div className="flex items-center gap-3">
                    <Link to="/" className="font-semibold text-blue-600">OnlineBanking</Link>
                    <nav className="flex items-center gap-1">
                        <NavLink to="/" className={linkCls} end>Home</NavLink>
                        {token && (
                            <>
                                <NavLink to="/dashboard" className={linkCls}>Dashboard</NavLink>
                                <NavLink to="/transactions" className={linkCls}>Transactions</NavLink>
                                <NavLink to="/accounts" className={linkCls}>Accounts</NavLink>
                                <NavLink to="/reports" className={linkCls}>Reports</NavLink> {/* 👈 NEU */}

                            </>
                        )}
                    </nav>
                </div>
                <div className="flex items-center gap-2">
                    <ThemeToggle />
                    {!token ? (
                        <NavLink to="/login" className="px-3 py-2 rounded text-sm bg-blue-600 text-white">Login</NavLink>
                    ) : (
                        <>
                            <span className="text-sm text-zinc-500">{email}</span>
                            <button onClick={logout} className="px-3 py-2 rounded text-sm border">Logout</button>
                        </>
                    )}
                </div>
            </div>
        </header>
    );
}
