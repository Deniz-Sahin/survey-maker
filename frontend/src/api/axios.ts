import axios from 'axios'
import { store } from '../store'

// Create an axios instance pointed at the same origin.
// Adjust baseURL if your backend runs on another host/port (e.g. http://localhost:5000)
const api = axios.create({
  baseURL: 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
})

// Optional: attach token from store on each request (keeps it fresh if changed)
api.interceptors.request.use((config) => {
  const state = store.getState()
  const token = state.auth.token
  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export default api