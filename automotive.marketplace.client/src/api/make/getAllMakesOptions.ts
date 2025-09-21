import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetAllMakesResponse } from "@/types/make/GetAllMakesResponse";
import { queryOptions } from "@tanstack/react-query";

const getAllMakes = () =>
  axiosClient.get<GetAllMakesResponse[]>(ENDPOINTS.MAKE.GET_ALL);

export const getAllMakesOptions = queryOptions({
  queryKey: makeKeys.all(),
  queryFn: getAllMakes,
});
