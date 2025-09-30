export type GetAllListingsQuery = {
  makeId?: string;
  models?: string[];
  city?: string;
  isUsed?: boolean;
  minYear?: number;
  yearTo?: number;
  priceFrom?: number;
  priceTo?: number;
};
