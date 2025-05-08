import axios from "axios";

const axiosClient = axios.create({
  baseURL:
    import.meta.env.VITE_APP_API_URL ??
    "https://api.automotive_marketplace.taurasbear.me",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 0,
  withCredentials: true,
});

export default axiosClient;
