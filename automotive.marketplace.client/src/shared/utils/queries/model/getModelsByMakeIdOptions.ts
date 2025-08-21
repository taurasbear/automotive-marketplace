import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { queryOptions } from "@tanstack/react-query";
import { modelKeys } from "./modelKeys";
import { GetModelsByMakeIdQuery } from "@/shared/types/dto/model/GetModelsByMakeIdQuery";
import { AxiosResponse } from "axios";
import { GetModelsByMakeIdResponse } from "@/shared/types/dto/model/GetModelsByMakeIdResponse";

const getModelsByMakeId = (
  query: GetModelsByMakeIdQuery,
): Promise<AxiosResponse<GetModelsByMakeIdResponse[]>> =>
  axiosClient.get(ENDPOINTS.MODEL.GET_BY_MAKE_ID, { params: query });

export const getModelsByMakeIdOptions = (query: GetModelsByMakeIdQuery) =>
  queryOptions({
    queryKey: modelKeys.byMakeId(query.makeId),
    queryFn: () => getModelsByMakeId(query),
  });
