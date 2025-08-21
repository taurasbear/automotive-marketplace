import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { queryOptions } from "@tanstack/react-query";
import { makeKeys } from "./makeKeys";
import { AxiosResponse } from "axios";
import { GetAllMakesResponse } from "@/shared/types/dto/make/GetAllMakesResponse";

const getAllMakes = (): Promise<AxiosResponse<GetAllMakesResponse[]>> =>
  axiosClient.get(ENDPOINTS.MAKE.GET_ALL);

export const getAllMakesOptions = queryOptions({
  queryKey: makeKeys.all(),
  queryFn: getAllMakes,
});
