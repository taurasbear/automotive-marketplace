import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetAllMakesResponse } from "../types/GetAllMakesResponse";
import { makeKeys } from "./makeKeys";

const getAllMakes = () =>
  axiosClient.get<GetAllMakesResponse[]>(ENDPOINTS.MAKE.GET_ALL);

export const getAllMakesOptions = queryOptions({
  queryKey: makeKeys.all(),
  queryFn: getAllMakes,
});
