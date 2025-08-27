import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { makeKeys } from "./makeKeys";
import { GetAllMakesResponse } from "../types/GetAllMakesResponse";

const getAllMakes = (): Promise<AxiosResponse<GetAllMakesResponse[]>> =>
  axiosClient.get(ENDPOINTS.MAKE.GET_ALL);

export const getAllMakesOptions = queryOptions({
  queryKey: makeKeys.all(),
  queryFn: getAllMakes,
});
