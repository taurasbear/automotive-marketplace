export const ENDPOINTS = {
  LISTING: {
    GET_ALL: "/Listing/GetAll",
  },
  AUTH: {
    REFRESH: "/Auth/Refresh",
    REGISTER: "/Auth/Register",
    LOGOUT: "/Auth/Logout",
    LOGIN: "Auth/Login",
  },
  MAKE: {
    GET_ALL: "/Make/GetAll",
  },
  MODEL: {
    GET_BY_MAKE_ID: "/Model/GetByMakeId",
  },
} as const;
