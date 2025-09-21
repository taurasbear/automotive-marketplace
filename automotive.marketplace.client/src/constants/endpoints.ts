export const ENDPOINTS = {
  LISTING: {
    GET_ALL: "/Listing/GetAll",
    CREATE: "/Listing/Create",
  },
  AUTH: {
    REFRESH: "/Auth/Refresh",
    REGISTER: "/Auth/Register",
    LOGOUT: "/Auth/Logout",
    LOGIN: "/Auth/Login",
  },
  MAKE: {
    GET_ALL: "/Make/GetAll",
  },
  MODEL: {
    GET_BY_MAKE_ID: "/Model/GetByMakeId",
  },
  ENUM: {
    GET_TRANSMISSIONS_TYPES: "/Enum/GetTransmissionTypes",
    GET_FUEL_TYPES: "/Enum/GetFuelTypes",
    GET_BODY_TYPES: "/Enum/GetBodyTypes",
    GET_DRIVETRAIN_TYPES: "/Enum/GetDrivetrainTypes",
  },
} as const;
