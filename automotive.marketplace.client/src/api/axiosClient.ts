import { selectAccessToken } from "@/shared/state/authSlice";
import { store } from "@/shared/state/store";
import axios from "axios";

const axiosClient = axios.create({
  baseURL:
    (import.meta.env.VITE_APP_API_URL as string) ||
    "https://api.automotive-marketplace.taurasbear.me",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 0,
  withCredentials: true,
});

axiosClient.interceptors.request.use((config) => {
  const accessToken = selectAccessToken(store.getState());
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

export default axiosClient;
