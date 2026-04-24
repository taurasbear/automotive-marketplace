import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { defectKeys } from "../queryKeys/defectKeys";
import { DefectCategory } from "./types/DefectCategory";

const getDefectCategories = () =>
  axiosClient.get<DefectCategory[]>(ENDPOINTS.DEFECT.GET_CATEGORIES);

export const getDefectCategoriesOptions = queryOptions({
  queryKey: defectKeys.categories(),
  queryFn: getDefectCategories,
});
