import axios from "axios";

export const authClient = axios.create({
  baseURL:
    (import.meta.env.VITE_APP_API_URL as string) ||
    "https://api.automotive-marketplace.taurasbear.me",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 10000,
  withCredentials: true,
});
