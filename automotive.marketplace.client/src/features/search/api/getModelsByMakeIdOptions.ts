import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { GetModelsByMakeIdQuery } from "../types/GetModelsByMakeIdQuery";
import { GetModelsByMakeIdResponse } from "../types/GetModelsByMakeIdResponse";
import { modelKeys } from "./modelKeys";

const getModelsByMakeId = (
  query: GetModelsByMakeIdQuery,
): Promise<AxiosResponse<GetModelsByMakeIdResponse[]>> =>
  axiosClient.get(ENDPOINTS.MODEL.GET_BY_MAKE_ID, { params: query });

export const getModelsByMakeIdOptions = (query: GetModelsByMakeIdQuery) =>
  queryOptions({
    queryKey: modelKeys.byMakeId(query.makeId),
    queryFn: () => getModelsByMakeId(query),
  });
