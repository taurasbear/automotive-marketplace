import { CarConditionKey } from "@/constants/carConditions";

export type ListingSearchStateValues = {
  makeId: string;
  modelId: string;
  city: string;
  isUsed: CarConditionKey;
  yearFrom?: string;
  yearTo?: string;
  priceFrom?: string;
  priceTo?: string;
};
