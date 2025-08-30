import { selectAccessToken } from "@/features/auth";
import { store } from "@/lib/redux/store";
import axios, { AxiosError } from "axios";
import { handleAxiosError } from "./axiosErrorHandler";

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

axiosClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => handleAxiosError(error),
);

export default axiosClient;
