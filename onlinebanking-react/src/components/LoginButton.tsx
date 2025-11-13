import { useState } from 'react'
import { login } from '../lib/api'
import React from "react";

export default function LoginButton() {
    const [email, setEmail] = useState('')
    const [msg, setMsg] = useState('')
    return (
        <div>
            <input aria-label="email" value={email} onChange={e => setEmail(e.target.value)} />
            <button onClick={async () => {
                const res = await login(email, 'Admin23!') // <— angepasst
                setMsg(`Hallo ${res.email}`)
            }}>
                Login
            </button>
            {msg && <p>{msg}</p>}
        </div>
    )
}
