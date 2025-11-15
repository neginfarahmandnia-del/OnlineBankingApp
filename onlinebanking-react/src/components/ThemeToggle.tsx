import { useEffect, useState } from "react";
import React from "react";

export default function ThemeToggle() {
    const [dark, setDark] = useState(() => localStorage.getItem("theme") === "dark");
    useEffect(() => {
        const root = document.documentElement;
        if (dark) {
            root.classList.add("dark");
            localStorage.setItem("theme", "dark");
        } else {
            root.classList.remove("dark");
            localStorage.setItem("theme", "light");
        }
    }, [dark]);
    return (
        <button onClick={() => setDark((v) => !v)} className="px-2 py-1 rounded border text-sm">
            {dark ? "Light" : "Dark"}
        </button>
    );
}
