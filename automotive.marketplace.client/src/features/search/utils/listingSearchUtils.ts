import { UI_CONSTANTS } from "@/constants/uiConstants";
import { SearchParams } from "@/features/search";
import { SearchStateValues } from "../types/searchStateValues";

const isUsedMapping = {
  new: false,
  used: true,
  newUsed: undefined,
};

export const getSearchParams = (
  searchValues: SearchStateValues,
): SearchParams => {
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
