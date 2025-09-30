export type GetAllListingsQuery = {
  makeId?: string;
  models?: string[];
  city?: string;
  isUsed?: boolean;
  minYear?: number;
  maxYear?: number;
  priceFrom?: number;
  priceTo?: number;
};
