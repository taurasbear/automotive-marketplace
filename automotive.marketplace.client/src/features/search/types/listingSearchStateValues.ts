import { CarConditionKey } from "@/constants/carConditions";

export type ListingSearchStateValues = {
  makeId: string;
  models: string[];
  city: string;
  isUsed: CarConditionKey;
  minYear: string;
  maxYear: string;
  minPrice: string;
  maxPrice: string;
};
