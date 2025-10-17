/**
 * MSW (Mock Service Worker) request handlers
 * Defines mock responses for API endpoints used in tests
 */

import { http, HttpResponse } from 'msw'

const API_BASE_URL = '/api'

/**
 * Admin authentication API handlers
 */
export const adminAuthHandlers = [
  // POST /api/admin/auth/login - Successful login
  http.post(`${API_BASE_URL}/admin/auth/login`, async ({ request }) => {
    const body = await request.json() as { username: string; password: string }

    // Mock success for valid credentials
    if (body.username === 'admin' && body.password === 'password123') {
      return HttpResponse.json({
        success: true,
        message: 'Authentication successful',
      }, {
        status: 200,
        headers: {
          'Set-Cookie': 'admin_token=mock-jwt-token; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=86400',
        },
      })
    }

    // Mock failure for invalid credentials
    if (body.username && body.password) {
      return HttpResponse.json({
        success: false,
        message: 'Invalid username or password',
      }, { status: 401 })
    }

    // Mock validation error for missing fields
    return HttpResponse.json({
      success: false,
      message: 'Username and password are required',
    }, { status: 400 })
  }),

  // POST /api/admin/auth/logout - Successful logout
  http.post(`${API_BASE_URL}/admin/auth/logout`, () => {
    return HttpResponse.json({
      success: true,
      message: 'Logout successful',
    }, {
      status: 200,
      headers: {
        'Set-Cookie': 'admin_token=; HttpOnly; Secure; SameSite=Strict; Path=/; Max-Age=0',
      },
    })
  }),

  // GET /api/admin/auth/me - Get current user (optional, for future use)
  http.get(`${API_BASE_URL}/admin/auth/me`, ({ cookies }) => {
    // Check if admin_token cookie exists
    if (cookies.admin_token) {
      return HttpResponse.json({
        success: true,
        data: {
          username: 'admin',
          role: 'admin',
        },
      }, { status: 200 })
    }

    return HttpResponse.json({
      success: false,
      message: 'Unauthorized',
    }, { status: 401 })
  }),
]

/**
 * All request handlers (can add more handler groups here)
 */
export const handlers = [
  ...adminAuthHandlers,
]
