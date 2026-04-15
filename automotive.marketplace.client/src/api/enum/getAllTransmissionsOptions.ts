// TODO: Used by the Create/Edit Variant form (not yet implemented)
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { Transmission } from "@/types/enum/Transmission";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getAllTransmissions = () =>
  axiosClient.get<Transmission[]>(ENDPOINTS.TRANSMISSION.GET_ALL);

export const getAllTransmissionsOptions = queryOptions({
  queryKey: enumKeys.transmissions(),
  queryFn: getAllTransmissions,
});
