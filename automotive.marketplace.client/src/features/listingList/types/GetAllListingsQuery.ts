export type GetAllListingsQuery = {
  makeId?: string;
  modelId?: string;
  city?: string;
  isUsed?: boolean;
  yearFrom?: number;
  yearTo?: number;
  priceFrom?: number;
  priceTo?: number;
};
