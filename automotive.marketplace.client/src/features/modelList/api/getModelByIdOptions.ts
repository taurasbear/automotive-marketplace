import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetModelByIdQuery } from "../types/GetModelByIdQuery";
import { GetModelByIdResponse } from "../types/GetModelByIdResponse";

const getModelById = (query: GetModelByIdQuery) =>
  axiosClient.get<GetModelByIdResponse>(ENDPOINTS.MODEL.GET_BY_ID, {
    params: query,
  });

export const getModelByIdOptions = (query: GetModelByIdQuery) =>
  queryOptions({
    queryKey: modelKeys.byId(query.id),
    queryFn: () => getModelById(query),
  });
