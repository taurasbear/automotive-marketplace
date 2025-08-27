import { CarConditionKey } from "@/constants/carConditions";
import { SearchParams } from "@/features/search";

export const getSearchParamFromValue = <K extends keyof SearchParams>(
  key: K,
  value: string,
) => {
  if (key === "makeId") {
    return value === "all" ? undefined : value;
  }

  if (key === "city") {
    return value === "any" ? undefined : value;
  }

  if (key === "isUsed") {
    const isUsedValue =
      {
        new: false,
        used: true,
        newused: null,
      }[value as CarConditionKey] ?? null;

    return isUsedValue;
  }
  if (
    key === "priceFrom" ||
    key === "priceTo" ||
    key === "yearFrom" ||
    key === "yearTo"
  ) {
    return Number(value);
  }

  return value;
};
