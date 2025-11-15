// src/main.tsx
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import React from "react";

import App from "./App";
import "./index.css";

import Home from "./pages/Home";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import TransactionsPage from "./pages/Transactions";
import Accounts from "./pages/Accounts";
import Reports from "./pages/Reports";
import NotFound from "./pages/NotFound";
import ProtectedRoute from "./routes/ProtectedRoute";
import RoleGuard from "./routes/RoleGuard";

const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,
        errorElement: <NotFound />,
        children: [
            { index: true, element: <Home /> },
            { path: "login", element: <Login /> },

            // geschützte Bereiche
            {
                element: <ProtectedRoute />,
                children: [
                    { path: "dashboard", element: <Dashboard /> },
                    { path: "transactions", element: <TransactionsPage /> },
                    { path: "accounts", element: <Accounts /> },
                    { path: "reports", element: <Reports /> },

                    // nur für bestimmte Rollen
                    {
                        element: <RoleGuard allow={["Admin", "Manager"]} />,
                        children: [
                            // TODO: später eigene Admin-Seite bauen
                            { path: "admin", element: <div>Adminbereich</div> },
                        ],
                    },
                ],
            },

            { path: "*", element: <NotFound /> },
        ],
    },
]);

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <RouterProvider router={router} />
    </StrictMode>
);
