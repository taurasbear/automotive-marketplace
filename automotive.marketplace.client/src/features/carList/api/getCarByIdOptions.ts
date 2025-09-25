import { carKeys } from "@/api/queryKeys/carKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetCarByIdQuery } from "../types/GetCarByIdQuery";
import { GetCarByIdResponse } from "../types/GetCarByIdResponse";

const getCarById = (query: GetCarByIdQuery) =>
  axiosClient.get<GetCarByIdResponse>(ENDPOINTS.CAR.GET_BY_ID, {
    params: query,
  });

export const getCarByIdOptions = (query: GetCarByIdQuery) =>
  queryOptions({
    queryKey: carKeys.byId(query.id),
    queryFn: () => getCarById(query),
  });
