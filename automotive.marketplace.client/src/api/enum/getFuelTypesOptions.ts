import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetFuelTypesResponse } from "@/types/enum/GetFuelTypesResponse";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getFuelTypes = () =>
  axiosClient.get<GetFuelTypesResponse[]>(ENDPOINTS.ENUM.GET_FUEL_TYPES);

export const getFuelTypesOptions = queryOptions({
  queryKey: enumKeys.fuelTypes(),
  queryFn: () => getFuelTypes(),
});
