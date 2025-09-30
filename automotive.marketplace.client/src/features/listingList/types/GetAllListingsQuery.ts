export type GetAllListingsQuery = {
  makeId?: string;
  models?: string[];
  city?: string;
  isUsed?: boolean;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  priceTo?: number;
};
