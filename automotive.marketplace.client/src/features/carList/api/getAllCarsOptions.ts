import { carKeys } from "@/api/queryKeys/carKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetAllCarsResponse } from "../types/GetAllCarsResponse";

const getAllCars = () =>
  axiosClient.get<GetAllCarsResponse[]>(ENDPOINTS.CAR.GET_ALL);

export const getAllCarsOptions = queryOptions({
  queryKey: carKeys.all(),
  queryFn: getAllCars,
});
