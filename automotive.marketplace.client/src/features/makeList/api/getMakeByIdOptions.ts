import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetMakeByIdQuery } from "../types/GetMakeByIdQuery";
import { GetMakeByIdResponse } from "../types/GetMakeByIdResponse";

const getMakeById = (query: GetMakeByIdQuery) =>
  axiosClient.get<GetMakeByIdResponse>(ENDPOINTS.MAKE.GET_BY_ID, {
    params: query,
  });

export const getMakeByIdOptions = (query: GetMakeByIdQuery) =>
  queryOptions({
    queryKey: makeKeys.byId(query.id),
    queryFn: () => getMakeById(query),
  });
