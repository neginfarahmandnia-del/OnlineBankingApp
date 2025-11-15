import { http, HttpResponse } from 'msw'

const adminEmail = '1admin@example.com'
const adminPassword = 'Admin23!'

export const handlers = [
    http.options('*/api/Auth/login', () => new HttpResponse(null, { status: 200 })),
    http.options('*/api/Transactions', () => new HttpResponse(null, { status: 200 })),
    // src/test/handlers.ts  (oder wo du /api/Auth/me mockst)
    http.get('*/api/Auth/me', () =>
        HttpResponse.json({ userId: 'admin-1', email: adminEmail, roles: ['Admin'] })
    ),

    http.post('*/api/Auth/login', async ({ request }) => {
        const body = (await request.json()) as { email: string; password: string }
        if (body.email === adminEmail && body.password === adminPassword) {
            return HttpResponse.json({ token: 'demo-admin-token', email: adminEmail, roles: ['Admin'] })
        }
        return new HttpResponse('Unauthorized', { status: 401 })
    }),

    http.get('*/api/Auth/me', () =>
        HttpResponse.json({ email: adminEmail, roles: ['Admin'] })
    ),

    http.get('*/api/Transactions', () =>
        HttpResponse.json({
            items: [{ id: 1, amount: 150, description: 'Startguthaben', date: '2025-01-01' }],
            total: 1, page: 1, pageSize: 50
        })
    )
]
