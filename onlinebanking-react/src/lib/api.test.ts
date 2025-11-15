// src/lib/api.test.ts
import { test, expect } from "vitest";
import { login, getTransactions } from "./api";

test("login liefert Token", async () => {
    const res = await login("1admin@example.com", "Admin23!");
    expect(res.token).toBeTruthy();
});

test("getTransactions liefert Liste", async () => {
    const res: any = await getTransactions();
    const items = Array.isArray(res) ? res : res.items;
    expect(Array.isArray(items)).toBe(true);
});
