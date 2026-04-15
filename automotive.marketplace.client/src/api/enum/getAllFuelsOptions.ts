// TODO: Used by the Create/Edit Variant form (not yet implemented)
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { Fuel } from "@/types/enum/Fuel";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getAllFuels = () => axiosClient.get<Fuel[]>(ENDPOINTS.FUEL.GET_ALL);

export const getAllFuelsOptions = queryOptions({
  queryKey: enumKeys.fuels(),
  queryFn: getAllFuels,
});
