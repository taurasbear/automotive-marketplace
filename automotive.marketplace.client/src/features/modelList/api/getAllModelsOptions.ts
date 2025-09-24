import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetAllModelsResponse } from "../types/GetAllModelsResponse";

const getAllModels = () =>
  axiosClient.get<GetAllModelsResponse[]>(ENDPOINTS.MODEL.GET_ALL);

export const getAllModelsOptions = queryOptions({
  queryKey: modelKeys.all(),
  queryFn: getAllModels,
});
