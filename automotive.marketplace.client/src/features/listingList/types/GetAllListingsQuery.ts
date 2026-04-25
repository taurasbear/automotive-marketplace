export type GetAllListingsQuery = {
  makeId?: string;
  models?: string[];
  municipalityId?: string;
  isUsed?: boolean;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
};
