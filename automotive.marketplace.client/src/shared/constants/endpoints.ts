export const ENDPOINTS = {
  LISTING: {
    GET_ALL: "/listing",
  },
  AUTH: {
    REFRESH: "/auth/refresh",
    REGISTER: "/auth/register",
    LOGOUT: "/auth/logout",
    LOGIN: "auth/login",
  },
  MAKE: {
    GET_ALL: "/make",
  },
  MODEL: {
    GET_BY_MAKE_ID: "/model",
  },
} as const;
