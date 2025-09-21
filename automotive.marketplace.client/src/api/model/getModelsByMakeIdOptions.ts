import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetModelsByMakeIdQuery } from "@/types/model/GetModelsByMakeIdQuery";
import { GetModelsByMakeIdResponse } from "@/types/model/GetModelsByMakeIdResponse";
import { queryOptions } from "@tanstack/react-query";

const getModelsByMakeId = (query: GetModelsByMakeIdQuery) =>
  axiosClient.get<GetModelsByMakeIdResponse[]>(ENDPOINTS.MODEL.GET_BY_MAKE_ID, {
    params: query,
  });

export const getModelsByMakeIdOptions = (query: GetModelsByMakeIdQuery) =>
  queryOptions({
    queryKey: modelKeys.byMakeId(query.makeId),
    queryFn: () => getModelsByMakeId(query),
  });
