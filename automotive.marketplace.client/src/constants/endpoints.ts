export const ENDPOINTS = {
  LISTING: {
    GET_BY_ID: "/Listing/GetById",
    GET_ALL: "/Listing/GetAll",
    CREATE: "/Listing/Create",
    DELETE: "/Listing/Delete",
    UPDATE: "/Listing/Update",
    SEARCH: "/Listing/Search",
    COMPARE: "/Listing/Compare",
  },
  AUTH: {
    REFRESH: "/Auth/Refresh",
    REGISTER: "/Auth/Register",
    LOGOUT: "/Auth/Logout",
    LOGIN: "/Auth/Login",
  },
  MAKE: {
    GET_ALL: "/Make/GetAll",
    GET_BY_ID: "/Make/GetById",
    CREATE: "/Make/Create",
    UPDATE: "/Make/Update",
    DELETE: "/Make/Delete",
  },
  MODEL: {
    GET_BY_MAKE_ID: "/Model/GetByMakeId",
    GET_BY_ID: "/Model/GetById",
    GET_ALL: "/Model/GetAll",
    CREATE: "/Model/Create",
    DELETE: "/Model/Delete",
    UPDATE: "/Model/Update",
  },
  CAR: {
    GET_BY_ID: "/Car/GetById",
    GET_ALL: "/Car/GetAll",
    CREATE: "/Car/Create",
    DELETE: "/Car/Delete",
    UPDATE: "/Car/Update",
  },
  VARIANT: {
    GET_BY_MODEL_ID: "/Variant/GetByModelId",
    CREATE: "/Variant/Create",
    UPDATE: "/Variant/Update",
    DELETE: "/Variant/Delete",
  },
  FUEL: {
    GET_ALL: "/Fuel/GetAll",
  },
  TRANSMISSION: {
    GET_ALL: "/Transmission/GetAll",
  },
  BODY_TYPE: {
    GET_ALL: "/BodyType/GetAll",
  },
  DRIVETRAIN: {
    GET_ALL: "/Drivetrain/GetAll",
  },
  ENUM: {
    GET_TRANSMISSIONS_TYPES: "/Enum/GetTransmissionTypes",
    GET_FUEL_TYPES: "/Enum/GetFuelTypes",
    GET_BODY_TYPES: "/Enum/GetBodyTypes",
    GET_DRIVETRAIN_TYPES: "/Enum/GetDrivetrainTypes",
  },
  CHAT: {
    GET_OR_CREATE_CONVERSATION: "/Chat/GetOrCreateConversation",
    GET_CONVERSATIONS: "/Chat/GetConversations",
    GET_MESSAGES: "/Chat/GetMessages",
    MARK_MESSAGES_READ: "/Chat/MarkMessagesRead",
    GET_UNREAD_COUNT: "/Chat/GetUnreadCount",
  },
  SAVED_LISTING: {
    TOGGLE_LIKE: "/SavedListing/ToggleLike",
    GET_ALL: "/SavedListing/GetAll",
    UPSERT_NOTE: "/SavedListing/UpsertNote",
    DELETE_NOTE: "/SavedListing/DeleteNote",
  },
} as const;
