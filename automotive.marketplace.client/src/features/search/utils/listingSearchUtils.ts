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
    modelId:
      searchValues.modelId === UI_CONSTANTS.SELECT.ALL_MODELS.VALUE
        ? undefined
        : searchValues.modelId,
    city:
      searchValues.city === UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE
        ? undefined
        : searchValues.city,
    isUsed: isUsedMapping[searchValues.isUsed],
    yearFrom: Number(searchValues.yearFrom),
    yearTo: Number(searchValues.yearTo),
    priceFrom: Number(searchValues.priceFrom),
    priceTo: Number(searchValues.priceTo),
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
    modelId: searchParams.modelId ?? UI_CONSTANTS.SELECT.ALL_MODELS.VALUE,
    city: searchParams.city ?? UI_CONSTANTS.SELECT.ANY_LOCATION.VALUE,
    isUsed: isUsedValue,
    yearFrom: String(searchParams.yearFrom),
    yearTo: String(searchParams.yearTo),
    priceFrom: String(searchParams.priceFrom),
    priceTo: String(searchParams.priceTo),
  };
};
