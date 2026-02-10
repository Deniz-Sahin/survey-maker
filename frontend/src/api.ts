import axios from 'axios'

/**
 * Centralized axios instance and token helpers.
 *
 * - Import the default `api` instance to make requests.
 * - Call `setAuthToken(token)` after login (or when token changes) to attach the Authorization header.
 * - Call `clearAuthToken()` to remove the header on logout.
 *
 * This keeps all HTTP defaults in one place and avoids per-component axios setup.
 */

const api = axios.create({
    baseURL: 'http://localhost:5000', // leave empty to use same origin; set to API URL if needed
  headers: {
    'Content-Type': 'application/json',
  },
})

export function setAuthToken(token?: string | null) {
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
  } else {
    delete api.defaults.headers.common['Authorization']
  }
}

export function clearAuthToken() {
  delete api.defaults.headers.common['Authorization']
}

export default api