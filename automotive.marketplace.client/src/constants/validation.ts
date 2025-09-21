export const VALIDATION = {
  NAME: {
    SHORT: 5,
    MEDIUM: 30,
    LONG: 125,
  },
  TEXT: {
    SHORT: 55,
    MEDIUM: 255,
    LONG: 1000,
  },
  USERNAME: {
    MIN: 3,
    MAX: 30,
  },
  PASSWORD: {
    MIN: 6,
    MAX: 100,
  },
  PRICE: {
    MIN: 50,
    MAX: 10_000_000,
  },
  POWER: {
    MIN: 5,
    MAX: 1000,
  },
  ENGINE_SIZE: {
    MIN: 300,
    MAX: 10000,
  },
  MILEAGE: {
    MIN: 0,
    MAX: 10_000_000,
  },
  YEAR: {
    MIN: 1900,
  },
  VIN: {
    REGEX: /^[A-HJ-NPR-Z0-9]{17}$/i,
  },
  GUID: {
    REGEX:
      /^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$/,
  },
  DOOR_COUNT: {
    MIN: 1,
    MAX: 9,
  },
} as const;
