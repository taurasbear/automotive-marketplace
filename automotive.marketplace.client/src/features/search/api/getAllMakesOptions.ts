import { ENDPOINTS } from "@/constants/endpoints";
import { GetAllMakesResponse } from "@/features/search/types/GetAllMakesResponse";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { makeKeys } from "./makeKeys";

const getAllMakes = (): Promise<AxiosResponse<GetAllMakesResponse[]>> =>
  axiosClient.get(ENDPOINTS.MAKE.GET_ALL);

export const getAllMakesOptions = queryOptions({
  queryKey: makeKeys.all(),
  queryFn: getAllMakes,
});
