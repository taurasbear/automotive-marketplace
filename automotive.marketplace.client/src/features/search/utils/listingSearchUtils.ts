import { UI_CONSTANTS } from "@/constants/uiConstants";
import { ListingSearchParams } from "@/features/search";
import { ListingSearchStateValues } from "../types/listingSearchStateValues";

const isUsedMapping = {
  new: false,
  used: true,
  newUsed: undefined,
};

export const getSearchParams = (
  searchValues: ListingSearchStateValues,
): ListingSearchParams => {
  return {
    makeId:
      searchValues.makeId === UI_CONSTANTS.SELECT.ALL_MAKES.VALUE
        ? undefined
        : searchValues.makeId,
    models:
      searchValues.models[0] === UI_CONSTANTS.SELECT.ALL_MODELS.VALUE ||
      searchValues.models.length === 0
        ? undefined
        : searchValues.models,
    city:
      searchValues.city === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
        ? undefined
        : searchValues.city,
    isUsed: isUsedMapping[searchValues.isUsed],
    minYear: Number(searchValues.minYear),
    maxYear: Number(searchValues.maxYear),
    minPrice: Number(searchValues.minPrice),
    maxPrice: Number(searchValues.maxPrice),
  };
};

export const getSearchValues = (
  searchParams: ListingSearchParams,
): ListingSearchStateValues => {
  const isUsedValue =
    (Object.keys(isUsedMapping) as Array<keyof typeof isUsedMapping>).find(
      (key) => isUsedMapping[key] === searchParams.isUsed,
    ) ?? "newUsed";

  return {
    makeId: searchParams.makeId ?? UI_CONSTANTS.SELECT.ALL_MAKES.VALUE,
    models: searchParams.models ?? [],
    city: searchParams.city ?? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
    isUsed: isUsedValue,
    minYear: String(searchParams.minYear),
    maxYear: String(searchParams.maxYear),
    minPrice: String(searchParams.minPrice),
    maxPrice: String(searchParams.maxPrice),
  };
};
