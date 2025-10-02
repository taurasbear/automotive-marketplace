import { ListingSearchStateValues } from "@/features/search";

export type ListingFilterStateValues = ListingSearchStateValues & {
  minMileage: string;
  maxMileage: string;
  minPower: string;
  maxPower: string;
};
