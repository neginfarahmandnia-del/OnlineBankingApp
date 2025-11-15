// src/components/LoginButton.test.tsx
import { test, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LoginButton from './LoginButton'
import React from "react";

test('loggt ein und zeigt Begrüßung', async () => {
    render(<LoginButton />)
    const user = userEvent.setup()

    await user.type(screen.getByLabelText(/email/i), '1admin@example.com')
    // ↓ Passwort muss zu handlers.ts passen:
    // Falls dein Button das Passwort intern setzt, passe die Komponente an.
    await user.click(screen.getByRole('button', { name: /login/i }))

    // Wenn die Komponente das Passwort intern falsch setzt (Passw0rd!),
    // wird login 401 liefern und der Text fehlt -> dann Komponente fixen:
    // - Entweder Passwortfeld hinzufügen und 'Admin23!' tippen
    // - Oder in der Komponente das feste Passwort auf 'Admin23!' ändern

    expect(await screen.findByText(/Hallo 1admin@example\.com/)).toBeInTheDocument()
})
