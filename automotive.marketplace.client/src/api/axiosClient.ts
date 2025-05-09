import axios from "axios";

const axiosClient = axios.create({
  baseURL:
    import.meta.env.VITE_APP_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 0,
  withCredentials: true,
});

export default axiosClient;
