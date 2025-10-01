import { UI_CONSTANTS } from "@/constants/uiConstants";
import { ListingFilterStateValues } from "@/features/listingList";
import { ListingSearchParams } from "@/features/search";
import { ListingSearchStateValues } from "../types/listingSearchStateValues";

const isUsedMapping = {
  new: false,
  used: true,
  newUsed: undefined,
};

export const mapFilterValuesToSearchParams = (
  searchValues: ListingFilterStateValues,
): ListingSearchParams => {
  const searchParams = mapSearchValuesToSearchParams(searchValues);
  return {
    ...searchParams,
    minMileage: Number(searchValues.minMileage),
    maxMileage: Number(searchValues.maxMileage),
    minPower: Number(searchValues.minPower),
    maxPower: Number(searchValues.maxPower),
  };
};

export const mapSearchValuesToSearchParams = (
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

export const mapSearchParamsToFilterValues = (
  searchParams: ListingSearchParams,
): ListingFilterStateValues => {
  const filterValues = mapSearchParamsToSearchValues(searchParams);

  return {
    ...filterValues,
    minMileage: searchParams.minMileage?.toString() || "",
    maxMileage: searchParams.maxMileage?.toString() || "",
    minPower: searchParams.minPower?.toString() || "",
    maxPower: searchParams.maxPower?.toString() || "",
  };
};

export const mapSearchParamsToSearchValues = (
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
    minYear: searchParams.minYear?.toString() || "",
    maxYear: searchParams.maxYear?.toString() || "",
    minPrice: searchParams.minPrice?.toString() || "",
    maxPrice: searchParams.maxPrice?.toString() || "",
  };
};
