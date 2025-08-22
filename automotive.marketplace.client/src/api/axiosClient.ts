import { useAppSelector } from "@/shared/hooks/redux";
import { selectAccessToken } from "@/shared/state/authSlice";
import axios from "axios";

const axiosClient = axios.create({
  baseURL:
    import.meta.env.VITE_APP_API_URL ??
    "https://api.automotive-marketplace.taurasbear.me",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 0,
  withCredentials: true,
});

axiosClient.interceptors.request.use((config) => {
  const token = useAppSelector(selectAccessToken);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default axiosClient;
